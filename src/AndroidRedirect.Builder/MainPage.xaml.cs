using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Shapes;
using Color = Microsoft.Maui.Graphics.Color;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace AndroidRedirect.Builder
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        // Static list of colors for monochromatic tinting
        public static readonly List<Color> MonochromaticColors = new List<Color>
        {
            Color.FromArgb("#50C878"), // Emerald Green (first color is default)
            Color.FromArgb("#F6C177"), // Primary (Yellow)
            Color.FromArgb("#EB6F92"), // Tertiary (Rose)
            Color.FromArgb("#9CCFD8"), // Light Blue
        };

        private readonly ILogger<MainPage> _logger;
        private string _packageName;
        private string _appName;
        private string _foregroundImagePath;
        private string _backgroundImagePath;
        private string _monochromaticImagePath;
        private Color _monochromaticTintColor = MonochromaticColors[0]; // Default is first color in the list

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

        public Color MonochromaticTintColor
        {
            get => _monochromaticTintColor;
            set
            {
                if (_monochromaticTintColor != value)
                {
                    _monochromaticTintColor = value;
                    OnPropertyChanged();
                    UpdateMonochromaticTintColor();
                }
            }
        }

        public MainPage(ILogger<MainPage> logger)
        {
            InitializeComponent();
            _logger = logger;
            BindingContext = this;
        }

        public void OnColorSelected(object sender, TappedEventArgs e)
        {
            if (sender is Border frame && frame.BindingContext is int colorIndex &&
                colorIndex >= 0 && colorIndex < MonochromaticColors.Count)
            {
                MonochromaticTintColor = MonochromaticColors[colorIndex];
                UpdateColorSwatchHighlight(colorIndex);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ColorSwatchesContainer.Children.Clear();

            for (int i = 0; i < MonochromaticColors.Count; i++)
            {
                var color = MonochromaticColors[i];

                var box = new BoxView
                {
                    WidthRequest = 30,
                    HeightRequest = 30,
                    CornerRadius = 60,
                    BackgroundColor = color,
                };

                var border = new Border
                {
                    Content = box,
                    WidthRequest = 40,
                    HeightRequest = 40,
                    Padding = 0,
                    BindingContext = i,
                    StrokeThickness = 3,
                    StrokeShape = new RoundRectangle { CornerRadius = 60 },
                    Stroke = Colors.White,
                    Background = Colors.Transparent
                };


                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += OnColorSelected;
                border.GestureRecognizers.Add(tapGesture);

                ColorSwatchesContainer.Children.Add(border);
            }
            UpdateColorSwatchHighlight(MonochromaticColors.IndexOf(MonochromaticTintColor));
        }

        private void UpdateColorSwatchHighlight(int selectedIndex)
        {
            // Update the visual appearance of all color swatches
            for (int i = 0; i < ColorSwatchesContainer.Children.Count; i++)
            {
                if (ColorSwatchesContainer.Children[i] is Border frame)
                {
                    if (frame.BindingContext is int colorIndex)
                    {
                        if (colorIndex == selectedIndex)
                        {
                            frame.StrokeThickness = 5;
                        }
                        else
                        {
                            frame.StrokeThickness = 3;
                        }
                    }
                }
            }
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
                MonochromaticPreview.Source = null;
            }
            else
            {
                MonochromaticImageOverlay.IsVisible = false;
                MonochromaticImagePreview.Source = MonochromaticImagePath;
                MonochromaticPreview.Source = MonochromaticImagePath;

                // Apply tint color to the monochromatic preview
                UpdateMonochromaticTintColor();
            }
        }

        private void UpdateMonochromaticTintColor()
        {
            if (string.IsNullOrEmpty(MonochromaticImagePath))
                return;

            try
            {
                using var bitmap = new Bitmap(MonochromaticImagePath);

                var tintColor = System.Drawing.Color.FromArgb(
                    255, // Full alpha
                    (int)(MonochromaticTintColor.Red * 255),
                    (int)(MonochromaticTintColor.Green * 255),
                    (int)(MonochromaticTintColor.Blue * 255));

                var tintColorBG = GetAdaptiveIconBackgroundColor(tintColor);

                for (int y = 0; y < bitmap.Height; y++)
                {
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        var pixelColor = bitmap.GetPixel(x, y);
                        if (pixelColor.A > 0)
                        {
                            pixelColor = System.Drawing.Color.FromArgb(
                                pixelColor.A, // Preserve original alpha
                                tintColor.R, // Apply tint color
                                tintColor.G,
                                tintColor.B);

                            bitmap.SetPixel(x, y, pixelColor);
                        }
                        else
                        {
                            pixelColor = tintColorBG;

                            bitmap.SetPixel(x, y, pixelColor);
                        }
                    }
                }

                var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, ImageFormat.Png);
                memoryStream.Seek(0, SeekOrigin.Begin);
                MonochromaticPreview.Source = ImageSource.FromStream(() => memoryStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating monochromatic tint color");
            }
        }

        public static System.Drawing.Color GetAdaptiveIconBackgroundColor(System.Drawing.Color themeColor)
        {
            float r = themeColor.R / 255f;
            float g = themeColor.G / 255f;
            float b = themeColor.B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float h = 0, s = 0, l = (max + min) / 2f;

            if (max != min)
            {
                float d = max - min;
                s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

                if (max == r)
                    h = (g - b) / d + (g < b ? 6f : 0f);
                else if (max == g)
                    h = (b - r) / d + 2f;
                else
                    h = (r - g) / d + 4f;

                h /= 6f;
            }

            s *= 0.2f;
            l *= 0.25f;

            Func<float, float, float, float> hue2rgb = (p, q, t) =>
            {
                if (t < 0f) t += 1f;
                if (t > 1f) t -= 1f;
                if (t < 1f / 6f) return p + (q - p) * 6f * t;
                if (t < 1f / 2f) return q;
                if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
                return p;
            };

            float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
            float p = 2f * l - q;

            r = hue2rgb(p, q, h + 1f / 3f);
            g = hue2rgb(p, q, h);
            b = hue2rgb(p, q, h - 1f / 3f);

            return System.Drawing.Color.FromArgb(
                255,
                (int)(r * 255),
                (int)(g * 255),
                (int)(b * 255)
            );
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
