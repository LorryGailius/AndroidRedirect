namespace AndroidRedirect.Builder.Resources.Controls
{
    public partial class ImagePicker
    {
        public static readonly BindableProperty ImageSourceProperty = BindableProperty.Create(
            nameof(ImageSource),
            typeof(string),
            typeof(ImagePicker),
            null,
            defaultBindingMode: BindingMode.TwoWay,
            propertyChanged: OnImageSourceChanged);

        // Top Label properties
        public static readonly BindableProperty TopLabelTextProperty = BindableProperty.Create(
            nameof(TopLabelText),
            typeof(string),
            typeof(ImagePicker),
            string.Empty);

        // Bottom Label properties
        public static readonly BindableProperty BottomLabelTextProperty = BindableProperty.Create(
            nameof(BottomLabelText),
            typeof(string),
            typeof(ImagePicker),
            string.Empty);

        // Size property
        public static readonly BindableProperty FrameSizeProperty = BindableProperty.Create(
            nameof(FrameSize),
            typeof(double),
            typeof(ImagePicker),
            120.0);

        public ImagePicker()
        {
            InitializeComponent();
        }

        // Image Source property
        public string ImageSource
        {
            get => (string)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        // Top Label properties
        public string TopLabelText
        {
            get => (string)GetValue(TopLabelTextProperty);
            set => SetValue(TopLabelTextProperty, value);
        }

        // Bottom Label properties
        public string BottomLabelText
        {
            get => (string)GetValue(BottomLabelTextProperty);
            set => SetValue(BottomLabelTextProperty, value);
        }

        // Size property
        public double FrameSize
        {
            get => (double)GetValue(FrameSizeProperty);
            set => SetValue(FrameSizeProperty, value);
        }

        private static void OnImageSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is ImagePicker imagePicker)
            {
                imagePicker.UpdateImageDisplay();
            }
        }

        private void UpdateImageDisplay()
        {
            if (string.IsNullOrEmpty(ImageSource))
            {
                ImageOverlay.IsVisible = true;
                ImagePreview.Source = null;
            }
            else
            {
                ImageOverlay.IsVisible = false;
                // Use explicit FileImageSource creation to avoid any type conflicts
                ImagePreview.Source = new FileImageSource { File = ImageSource };
            }
        }

        private async void OnSelectImageClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await PickImageAsync();
                if (result != null)
                {
                    ImageSource = result;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to select image", "OK");
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
                        await Application.Current.MainPage.DisplayAlert("Invalid File Type", 
                            "Please select a valid image file (jpg, png, jpeg).", "OK");
                    }
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}