namespace AndroidRedirect.Builder.Extensions;

public static class ImageExtensions
{
    public static void UpdateImageSource(this Image image, string imagePath) => 
        image.Source = string.IsNullOrEmpty(imagePath) ? null : imagePath;
}