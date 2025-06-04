using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using AndroidRedirect.Builder.Controls;
using AndroidRedirect.Builder.Extensions;

namespace AndroidRedirect.Builder
{
    public partial class MainPage : INotifyPropertyChanged
    {
        private readonly ILogger<MainPage> _logger;
        private string _packageName;
        private string _appName;
        private string _foregroundImagePath;
        private string _backgroundImagePath;
        private string _monochromaticImagePath;
        private SystemColor _foregroundIconColor;
        private SystemColor _backgroundIconColor;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ColorPicker.Init();
        }

        public string PackageName
        {
            get => _packageName;
            set
            {
                if (_packageName != value)
                {
                    _packageName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AppName
        {
            get => _appName;
            set
            {
                if (_appName != value)
                {
                    _appName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ForegroundImagePath
        {
            get => _foregroundImagePath;
            set
            {
                if (_foregroundImagePath != value)
                {
                    _foregroundImagePath = value;
                    OnPropertyChanged();
                    UpdateForegroundImageDisplay();
                }
            }
        }

        public string BackgroundImagePath
        {
            get => _backgroundImagePath;
            set
            {
                if (_backgroundImagePath != value)
                {
                    _backgroundImagePath = value;
                    OnPropertyChanged();
                    UpdateBackgroundImageDisplay();
                }
            }
        }

        public string MonochromaticImagePath
        {
            get => _monochromaticImagePath;
            set
            {
                if (_monochromaticImagePath != value)
                {
                    _monochromaticImagePath = value;
                    OnPropertyChanged();
                    UpdateMonochromaticImageDisplay();
                }
            }
        }

        public MainPage(ILogger<MainPage> logger)
        {
            InitializeComponent();
            _logger = logger;
            BindingContext = this;
        }

        private async void OnSelectForegroundImageClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await PickImageAsync();
                if (result != null)
                {
                    ForegroundImagePath = result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting foreground image");
                await DisplayAlert("Error", "Failed to select image", "OK");
            }
        }

        private async void OnSelectBackgroundImageClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await PickImageAsync();
                if (result != null)
                {
                    BackgroundImagePath = result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting background image");
                await DisplayAlert("Error", "Failed to select image", "OK");
            }
        }

        private async void OnSelectMonochromaticImageClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await PickImageAsync();
                if (result != null)
                {
                    MonochromaticImagePath = result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting monochromatic image");
                await DisplayAlert("Error", "Failed to select image", "OK");
            }
        }

        private void UpdateForegroundImageDisplay()
        {
            if (string.IsNullOrEmpty(ForegroundImagePath))
            {
                ForegroundImageOverlay.IsVisible = true;
                ForegroundImagePreview.Source = null;
                CombinedForegroundPreview.Source = null;
            }
            else
            {
                ForegroundImageOverlay.IsVisible = false;
                ForegroundImagePreview.Source = ForegroundImagePath;
                CombinedForegroundPreview.Source = ForegroundImagePath;
            }

            UpdateCombinedPreview();
        }

        private void UpdateBackgroundImageDisplay()
        {
            if (string.IsNullOrEmpty(BackgroundImagePath))
            {
                BackgroundImageOverlay.IsVisible = true;
                BackgroundImagePreview.Source = null;
                CombinedBackgroundPreview.Source = null;
            }
            else
            {
                BackgroundImageOverlay.IsVisible = false;
                BackgroundImagePreview.Source = BackgroundImagePath;
                CombinedBackgroundPreview.Source = BackgroundImagePath;
            }

            UpdateCombinedPreview();
        }

        private void UpdateMonochromaticImageDisplay()
        {
            if (string.IsNullOrEmpty(MonochromaticImagePath))
            {
                MonochromaticImageOverlay.IsVisible = true;
                MonochromaticImagePreview.Source = null;
                AdaptiveIconPreview.Source = null;
            }
            else
            {
                MonochromaticImageOverlay.IsVisible = false;
                MonochromaticImagePreview.Source = MonochromaticImagePath;
                AdaptiveIconPreview.Source = MonochromaticImagePath;

                UpdateAdaptiveIconPreview();
            }
        }

        private void OnColorPicker_ColorSelected(object sender, ColorSelectedEventArgs e)
        {
            _foregroundIconColor = e.ForegroundColor;
            _backgroundIconColor = e.BackgroundColor;

            UpdateAdaptiveIconPreview();
        }

        private void UpdateAdaptiveIconPreview()
        {
            if (string.IsNullOrEmpty(MonochromaticImagePath))
                return;

            try
            {
                using var bitmap = new Bitmap(MonochromaticImagePath);

                for (var y = 0; y < bitmap.Height; y++)
                {
                    for (var x = 0; x < bitmap.Width; x++)
                    {
                        var pixelColor = bitmap.GetPixel(x, y);
                        if (pixelColor.A > 0)
                        {
                            pixelColor = _foregroundIconColor.Alpha(pixelColor.A); // Apply tint color with original alpha
                        }
                        else
                        {
                            pixelColor = _backgroundIconColor;
                        }

                        bitmap.SetPixel(x, y, pixelColor);
                    }
                }

                var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                AdaptiveIconPreview.Source = ImageSource.FromStream(() => memoryStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating monochromatic tint color");
            }
        }

        private void UpdateCombinedPreview()
        {
            if (string.IsNullOrEmpty(BackgroundImagePath))
            {
                CombinedBackgroundPreview.BackgroundColor = Colors.LightGray;
            }
            else
            {
                CombinedBackgroundPreview.BackgroundColor = Colors.Transparent;
            }
        }

        private async Task<string> PickImageAsync()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select an image"
                });

                if (result != null)
                {
                    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase))
                    {
                        return result.FullPath;
                    }
                    else
                    {
                        await DisplayAlert("Invalid File Type", "Please select a valid image file (jpg, png, jpeg).", "OK");
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PickImageAsync");
                throw;
            }
        }

        private void OnBuildClicked(object sender, EventArgs e)
        {
            BuildRedirectApplication();
        }

        private void BuildRedirectApplication()
        {
            _logger.LogInformation("Building redirect application with the following information:");
            _logger.LogInformation($"Package Name: {PackageName}");
            _logger.LogInformation($"App Name: {AppName}");
            _logger.LogInformation($"Foreground Image: {ForegroundImagePath}");
            _logger.LogInformation($"Background Image: {BackgroundImagePath}");
            _logger.LogInformation($"Monochromatic Image: {MonochromaticImagePath}");

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Build Initiated",
                    $"Building app with:\nPackage: {PackageName}\nName: {AppName}",
                    "OK");
            });
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}