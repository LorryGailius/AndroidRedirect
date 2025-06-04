using Microsoft.Extensions.Logging;

namespace AndroidRedirect.Builder.Services
{
    public class ApplicationBuilderService : IApplicationBuilderService
    {
        private readonly ILogger<ApplicationBuilderService> _logger;
        private static string _appHostDirectory;

        public ApplicationBuilderService(ILogger<ApplicationBuilderService> logger)
        {
            _logger = logger;
        }

        public async Task BuildApplicationAsync(
            string packageName,
            string appName,
            string foregroundImagePath,
            string backgroundImagePath,
            string monochromaticImagePath)
        {
            _logger.LogInformation("Building redirect application with the following information:");
            _logger.LogInformation($"Package Name: {packageName}");
            _logger.LogInformation($"App Name: {appName}");
            _logger.LogInformation($"Foreground Image: {foregroundImagePath}");
            _logger.LogInformation($"Background Image: {backgroundImagePath}");
            _logger.LogInformation($"Monochromatic Image: {monochromaticImagePath}");

            // Find MainActivity.cs in the app host directory
            var mainActivityPath = Path.Combine(_appHostDirectory, "MainActivity.cs");

            if (!File.Exists(mainActivityPath))
            {
                await ShowAlertAsync(
                    "Build Error",
                    "MainActivity.cs not found in the app host directory. Please ensure the correct directory is selected.");
                return;
            }

            var mainActivityContent = await File.ReadAllTextAsync(mainActivityPath);
            mainActivityContent = mainActivityContent.Replace("?package_name?", packageName);
            await File.WriteAllTextAsync(mainActivityPath, mainActivityContent);

            _logger.LogInformation("MainActivity.cs updated with package name: {PackageName}", packageName);

            var manifestPath = Path.Combine(_appHostDirectory, "AndroidManifest.xml");
            if (!File.Exists(manifestPath))
            {
                await ShowAlertAsync(
                    "Build Error",
                    "AndroidManifest.xml not found in the app host directory. Please ensure the correct directory is selected.");
                return;
            }

            var manifestContent = await File.ReadAllTextAsync(manifestPath);
            manifestContent = manifestContent.Replace("?redirect_package_name?", $"{packageName}.redirect");
            await File.WriteAllTextAsync(manifestPath, manifestContent);

            _logger.LogInformation("AndroidManifest.xml updated with redirect package name");

            // get \Resources\values\strings.xml file
            var stringsPath = Path.Combine(_appHostDirectory, "Resources", "values", "strings.xml");

            if (!File.Exists(stringsPath))
            {
                await ShowAlertAsync(
                    "Build Error",
                    "strings.xml not found in the app host directory. Please ensure the correct directory is selected.");
                return;
            }

            var stringsContent = await File.ReadAllTextAsync(stringsPath);
            stringsContent = stringsContent.Replace("AndroidRedirect.AppHost", appName);
            await File.WriteAllTextAsync(stringsPath, stringsContent);

            _logger.LogInformation("strings.xml updated with app name: {AppName}", appName);

            await ShowAlertAsync(
                "Build Initiated",
                $"Building app with:\nPackage: {packageName}\nName: {appName}");
        }

        public async Task GetAppHostDirectory()
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select App Host Directory",
            });

            if (result != null)
            {
                // Validate that a .csproj file is selected
                if (result.FileName.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                {
                    _appHostDirectory = Path.GetDirectoryName(result.FullPath) ?? string.Empty;
                }
                else
                {
                    await ShowAlertAsync(
                        "App Host Required",
                        "Please select a valid app host directory. The application will now exit.");

                    Application.Current.Quit();
                }
            }
            else
            {
                await ShowAlertAsync(
                    "App Host Required",
                    "Please select a valid app host directory. The application will now exit.");

                Application.Current.Quit();
            }
        }

        private async Task ShowAlertAsync(string title, string message)
        {
            await Application.Current.MainPage.DisplayAlert(title, message, "OK");
        }
    }
}