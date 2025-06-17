namespace AndroidRedirect.Builder.Services
{
    public interface IApplicationBuilderService
    {
        Task BuildApplication(
            string packageName, 
            string appName, 
            string foregroundImagePath, 
            string backgroundImagePath, 
            string monochromaticImagePath);

        Task GetAppHostDirectory();
    }
}