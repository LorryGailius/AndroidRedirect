using System.Drawing;
using AndroidRedirect.Builder.Controls;
using AndroidRedirect.Builder.Extensions;
using AndroidRedirect.Builder.Services;
using Microsoft.Extensions.Logging;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace AndroidRedirect.Builder
{
    public partial class MainPage
    {
        private readonly ILogger<MainPage> _logger;
        private readonly IApplicationBuilderService _applicationBuilder;
        private SystemColor _foregroundIconColor;
        private SystemColor _backgroundIconColor;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ColorPicker.Init();
        }

        public string PackageName { get; set; }

        public string AppName { get; set; }

        public string ForegroundImagePath
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    CombinedForegroundPreview.UpdateImageSource(ForegroundImagePath);
                    UpdateIconPreview();
                }
            }
        }

        public string BackgroundImagePath
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    CombinedBackgroundPreview.UpdateImageSource(BackgroundImagePath);
                    UpdateIconPreview();
                }
            }
        }

        public string MonochromaticImagePath
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    AdaptiveIconPreview.UpdateImageSource(MonochromaticImagePath);
                    UpdateAdaptiveIconPreview();
                }
            }
        }

        public MainPage(ILogger<MainPage> logger, IApplicationBuilderService applicationBuilder)
        {
            InitializeComponent();
            _logger = logger;
            _applicationBuilder = applicationBuilder;
            BindingContext = this;
        }

        private void UpdateIconPreview()
        {
            CombinedBackgroundPreview.BackgroundColor = 
                string.IsNullOrEmpty(BackgroundImagePath) 
                    ? Colors.LightGray 
                    : Colors.Transparent;
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

                        pixelColor = pixelColor.A > 0 
                            ? _foregroundIconColor.Alpha(pixelColor.A) // Apply tint color with original alpha
                            : _backgroundIconColor;

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

        private void OnBuildClicked(object sender, EventArgs e)
        { 
            _applicationBuilder.BuildApplicationAsync(
                PackageName,
                AppName,
                ForegroundImagePath,
                BackgroundImagePath,
                MonochromaticImagePath);
        }
    }
}