using System.Diagnostics;
using AndroidRedirect.Builder.Extensions;
using Path = System.IO.Path;

namespace AndroidRedirect.Builder.Services
{
    /// <summary>
    /// Service responsible for building Android redirect applications
    /// </summary>
    public class ApplicationBuilderService : IApplicationBuilderService
    {
        private static string _appHostDirectory;
        private const string BuildErrorTitle = "Build Error";
        private Page _loadingPage;

        /// <summary>
        /// Builds an application with the specified parameters
        /// </summary>
        public async Task BuildApplication(
            string packageName,
            string appName,
            string foregroundImagePath,
            string backgroundImagePath,
            string monochromaticImagePath)
        {
            try
            {
                _loadingPage = await Application.Current.MainPage.ShowLoadingAsync("Building...");

                if (!await ValidateNetVersion())
                {
                    await Application.Current.MainPage.HideLoadingAsync(_loadingPage);
                    return;
                }

                var buildDirectory = await CreateBuild(
                    packageName,
                    appName,
                    foregroundImagePath,
                    backgroundImagePath,
                    monochromaticImagePath);

                if (buildDirectory == null)
                {
                    await Application.Current.MainPage.HideLoadingAsync(_loadingPage);
                    await ShowAlert(BuildErrorTitle, "Failed to create build directory. Please check the logs for more details.");
                    return;
                }

                var buildSuccess = await ExecuteBuildProcess(buildDirectory);

                await Application.Current.MainPage.HideLoadingAsync(_loadingPage);

                if (buildSuccess)
                {
                    await ShowAlert("Build Success", "Application built successfully.");

                    await PromptToSaveApkFile(buildDirectory, packageName, appName);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.HideLoadingAsync(_loadingPage);
                await ShowAlert(BuildErrorTitle, $"An unexpected error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Finds the built APK file and prompts the user to save it
        /// </summary>
        private async Task PromptToSaveApkFile(string buildDirectory, string packageName, string appName)
        {
            try
            {
                var searchPath = Path.Combine(buildDirectory, "bin");

                string apkFile = null;
                if (Directory.Exists(searchPath))
                {
                    var apkFiles = Directory.GetFiles(searchPath, "*.apk", SearchOption.AllDirectories);
                    if (apkFiles.Length > 0)
                    {
                        // Prefer signed APK if available (usually has -Signed suffix)
                        apkFile = apkFiles.FirstOrDefault(f => f.Contains("-Signed")) ?? apkFiles[0];
                    }
                }


                if (apkFile == null)
                {
                    await ShowAlert("APK Not Found", "The built APK file could not be found. Please check the build directory manually.");
                    return;
                }

                var suggestedFileName = $"{appName.Replace(" ", "")}-{packageName}.apk";

                var saveDialogResult = await Application.Current.MainPage.DisplayAlert(
                    "Save APK File",
                    "The application was built successfully. Would you like to save the APK file?",
                    "Yes", "No");

                if (saveDialogResult)
                {
                    await SaveApkFile(apkFile, suggestedFileName);
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Save APK Error", $"An error occurred while trying to save the APK: {ex.Message}");
            }
        }

        /// <summary>
        /// Opens a file picker to let the user select where to save the APK file
        /// </summary>
        private async Task SaveApkFile(string sourceApkPath, string suggestedFileName)
        {
            try
            {
                var downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                downloadsPath = Path.Combine(downloadsPath, "Downloads");

                // Fallback to Documents if Downloads doesn't exist
                if (!Directory.Exists(downloadsPath))
                {
                    downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }

                var destinationPath = Path.Combine(downloadsPath, suggestedFileName);
                File.Copy(sourceApkPath, destinationPath, true);

                await ShowAlert("APK Saved", $"The APK file has been saved to: {destinationPath}");

                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadsPath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
                }
                catch
                {
                    // ignored
                }
            }
            catch (Exception ex)
            {
                await ShowAlert("Save Error", $"Failed to save the APK file: {ex.Message}");
            }
        }

        /// <summary>
        /// Allows the user to select the app host directory
        /// </summary>
        public async Task GetAppHostDirectory()
        {
            if (HasValidAppHostDirectory())
            {
                return;
            }

            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select App Host Directory",
            });

            if (result != null)
            {
                if (result.FileName.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                {
                    _appHostDirectory = Path.GetDirectoryName(result.FullPath) ?? string.Empty;
                }
                else
                {
                    await ShowExitAlertAsync("Please select a valid app host directory. The application will now exit.");
                }
            }
            else
            {
                await ShowExitAlertAsync("Please select a valid app host directory. The application will now exit.");
            }
        }

        /// <summary>
        /// Checks if the app host directory is already set and valid
        /// </summary>
        /// <returns>True if app host directory is set, otherwise false</returns>
        public bool HasValidAppHostDirectory()
        {
            return !string.IsNullOrEmpty(_appHostDirectory) &&
                   Directory.Exists(_appHostDirectory) &&
                   Directory.GetFiles(_appHostDirectory, "*.csproj").Length > 0;
        }

        /// <summary>
        /// Validates that the installed .NET SDK version is compatible
        /// </summary>
        private async Task<bool> ValidateNetVersion()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                using var process = new Process();
                process.StartInfo = startInfo;
                process.Start();
                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    await ShowAlert(BuildErrorTitle, "Failed to check .NET SDK version. Make sure .NET SDK is installed.");
                    return false;
                }

                if (!string.IsNullOrEmpty(output) && Version.TryParse(output.Trim(), out var version))
                {
                    if (version.Major < 9)
                    {
                        await ShowAlert(BuildErrorTitle, $"Incompatible .NET SDK version: {version}. Version 9.0 or higher is required.");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                await ShowAlert(BuildErrorTitle, $"Error fetching dotnet version: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Executes the actual 'dotnet build' command
        /// </summary>
        private async Task<bool> ExecuteBuildProcess(string buildDir)
        {
            var projectFile = Directory.GetFiles(buildDir, "*.csproj").FirstOrDefault();
            if (string.IsNullOrEmpty(projectFile))
            {
                await ShowAlert(BuildErrorTitle, "No project file (.csproj) found in the build directory. Make sure the project file was copied correctly.");
                return false;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{projectFile}\" -c Release",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = buildDir
            };

            using var process = new Process();
            process.StartInfo = startInfo;

            try
            {
                process.Start();
                await process.WaitForExitAsync();

                var error = await process.StandardError.ReadToEndAsync();

                if (process.ExitCode == 0)
                {
                    return true;
                }
                else
                {
                    await ShowAlert(BuildErrorTitle, $"Build failed: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                await ShowAlert(BuildErrorTitle, $"Error during build process: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates the build directory and prepares all application files
        /// </summary>
        private async Task<string?> CreateBuild(
            string packageName,
            string appName,
            string foregroundImagePath,
            string backgroundImagePath,
            string monochromaticImagePath)
        {
            var mainActivityPath = Path.Combine(_appHostDirectory, "MainActivity.cs");
            if (!await ValidateFileExists(mainActivityPath))
            {
                return null;
            }

            // Create app host build directory with copied files
            var appHostBuildDirectory = await CreateAppHostBuildDirectory(mainActivityPath, packageName);
            if (appHostBuildDirectory == null)
            {
                return null;
            }

            // Update files with app specific information in the copied directory
            var buildMainActivityPath = Path.Combine(appHostBuildDirectory, "MainActivity.cs");
            await UpdateMainActivityFile(buildMainActivityPath, packageName);
            await UpdateManifestFile(appHostBuildDirectory, packageName);
            await UpdateStringsFile(appHostBuildDirectory, appName);
            await UpdateIcons(appHostBuildDirectory, foregroundImagePath, backgroundImagePath, monochromaticImagePath);

            return appHostBuildDirectory;
        }

        /// <summary>
        /// Creates and prepares the build directory
        /// </summary>
        private async Task<string?> CreateAppHostBuildDirectory(string mainActivityPath, string packageName)
        {
            var appHostBuildDirectory = Path.Combine(_appHostDirectory, "AppHostApplications", $"AppHost_{packageName}");
            if (Directory.Exists(appHostBuildDirectory))
            {
                var overwrite = await Application.Current.MainPage.DisplayAlert(
                    "Directory Exists",
                    $"The directory {appHostBuildDirectory} already exists. Do you want to overwrite it?",
                    "Yes", "No");

                if (!overwrite)
                {
                    return null;
                }

                // Delete the existing directory to ensure a clean state
                try
                {
                    Directory.Delete(appHostBuildDirectory, true);
                }
                catch (Exception ex)
                {
                    await ShowAlert(BuildErrorTitle, $"Error deleting existing directory: {ex.Message}");
                    return null;
                }
            }

            if (!await CopyAppHostFilesAsync(_appHostDirectory, appHostBuildDirectory))
            {
                return null;
            }

            return appHostBuildDirectory;
        }

        /// <summary>
        /// Copies all necessary files from the source to destination directory
        /// </summary>
        private async Task<bool> CopyAppHostFilesAsync(string sourceDirectory, string destinationDirectory)
        {
            try
            {
                Directory.CreateDirectory(destinationDirectory);

                var projectFiles = Directory.GetFiles(sourceDirectory, "*.csproj");
                if (projectFiles.Length == 0)
                {
                    await ShowAlert(BuildErrorTitle, "No project file (.csproj) found in the app host directory.");
                    return false;
                }

                foreach (var file in Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(sourceDirectory, file);
                    var destinationPath = Path.Combine(destinationDirectory, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? string.Empty);
                    File.Copy(file, destinationPath, true);
                }

                var copiedProjectFiles = Directory.GetFiles(destinationDirectory, "*.csproj");
                if (copiedProjectFiles.Length == 0)
                {
                    await ShowAlert(BuildErrorTitle, "Project file (.csproj) was not copied to the build directory.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                await ShowAlert(BuildErrorTitle, $"Error copying files: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Updates the app icons in the build directory
        /// </summary>
        private async Task UpdateIcons(
            string appHostBuildDirectory,
            string foregroundImagePath,
            string backgroundImagePath,
            string monochromaticImagePath)
        {
            var drawableDirectory = Path.Combine(appHostBuildDirectory, "Resources", "drawable");
            Directory.CreateDirectory(drawableDirectory);

            await CopyIconIfProvided(foregroundImagePath, drawableDirectory);
            await CopyIconIfProvided(backgroundImagePath, drawableDirectory);
            await CopyIconIfProvided(monochromaticImagePath, drawableDirectory);
        }

        /// <summary>
        /// Copies an icon file to the destination directory if the path is provided
        /// </summary>
        private Task CopyIconIfProvided(string imagePath, string destinationDirectory)
        {
            if (string.IsNullOrEmpty(imagePath))
                return Task.CompletedTask;

            try
            {
                var imageName = Path.GetFileName(imagePath);
                var destinationPath = Path.Combine(destinationDirectory, imageName);
                File.Copy(imagePath, destinationPath, true);
                return Task.CompletedTask;
            }
            catch (Exception)
            {
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Updates the MainActivity.cs file with package name and required imports
        /// </summary>
        private async Task UpdateMainActivityFile(string mainActivityPath, string packageName)
        {
            try
            {
                var mainActivityContent = await File.ReadAllTextAsync(mainActivityPath);

                mainActivityContent = mainActivityContent.Replace("?package_name?", packageName);

                await File.WriteAllTextAsync(mainActivityPath, mainActivityContent);
            }
            catch (Exception ex)
            {
                await ShowAlert(BuildErrorTitle, $"Error updating MainActivity.cs: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the AndroidManifest.xml file with package name
        /// </summary>
        private async Task UpdateManifestFile(string appHostBuildDirectory, string packageName)
        {
            var manifestPath = Path.Combine(appHostBuildDirectory, "AndroidManifest.xml");
            if (!await ValidateFileExists(manifestPath))
            {
                return;
            }

            try
            {
                var manifestContent = await File.ReadAllTextAsync(manifestPath);
                manifestContent = manifestContent.Replace("?redirect_package_name?", $"{packageName}.redirect");
                await File.WriteAllTextAsync(manifestPath, manifestContent);
            }
            catch (Exception ex)
            {
                await ShowAlert(BuildErrorTitle, $"Error updating AndroidManifest.xml: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the strings.xml file with app name
        /// </summary>
        private async Task UpdateStringsFile(string appHostBuildDirectory, string appName)
        {
            var stringsPath = Path.Combine(appHostBuildDirectory, "Resources", "values", "strings.xml");
            if (!await ValidateFileExists(stringsPath))
            {
                return;
            }

            try
            {
                var stringsContent = await File.ReadAllTextAsync(stringsPath);
                stringsContent = stringsContent.Replace("AndroidRedirect.AppHost", appName);
                await File.WriteAllTextAsync(stringsPath, stringsContent);
            }
            catch (Exception ex)
            {
                await ShowAlert(BuildErrorTitle, $"Error updating strings.xml: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates that a file exists
        /// </summary>
        private async Task<bool> ValidateFileExists(string filePath)
        {
            if (File.Exists(filePath))
            {
                return true;
            }

            await ShowAlert(BuildErrorTitle, $"{Path.GetFileName(filePath)} not found in the app host directory. Please ensure the correct directory is selected.");
            return false;
        }

        /// <summary>
        /// Shows an alert dialog and exits the application
        /// </summary>
        private async Task ShowExitAlertAsync(string message)
        {
            await ShowAlert("App Host Required", message);
            Application.Current.Quit();
        }

        /// <summary>
        /// Shows an alert dialog
        /// </summary>
        private async Task ShowAlert(string title, string message)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, "OK");
        }
    }
}