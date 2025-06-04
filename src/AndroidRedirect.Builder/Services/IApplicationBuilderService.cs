namespace AndroidRedirect.Builder.Services
{
    public interface IApplicationBuilderService
    {
        Task BuildApplicationAsync(
            string packageName, 
            string appName, 
            string foregroundImagePath, 
            string backgroundImagePath, 
            string monochromaticImagePath);

        Task GetAppHostDirectory();
    }
}