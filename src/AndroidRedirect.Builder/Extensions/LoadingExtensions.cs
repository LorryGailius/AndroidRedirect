using Microsoft.Maui.Controls.Shapes;

namespace AndroidRedirect.Builder.Extensions
{
    /// <summary>
    /// Extension methods to show and hide loading overlays
    /// </summary>
    public static class LoadingExtensions
    {
        /// <summary>
        /// Shows a loading overlay with the specified message
        /// </summary>
        /// <param name="page">The page to show the loading overlay on</param>
        /// <param name="message">The message to display (optional)</param>
        /// <returns>A reference to the loading page that was created</returns>
        public static Task<Page> ShowLoadingAsync(this Page page, string message = "Loading...")
        {
            var activityIndicator = new ActivityIndicator
            {
                IsRunning = true,
                Color = Colors.White,
                HeightRequest = 50,
                WidthRequest = 50,
                HorizontalOptions = LayoutOptions.Center
            };

            var messageLabel = new Label
            {
                Text = message,
                TextColor = Colors.White,
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };

            var contentStack = new VerticalStackLayout
            {
                Children = 
                {
                    activityIndicator,
                    messageLabel
                },
                Spacing = 10,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var contentBackground = new Border
            {
                Content = contentStack,
                BackgroundColor = new Color(0, 0, 0, 0.8f),
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(10)
                },
                Padding = new Thickness(30),
                WidthRequest = 220,
                HeightRequest = 160,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            var grid = new Grid
            {
                BackgroundColor = new Color(0, 0, 0, 0.5f)
            };

            grid.Add(contentBackground);

            var loadingPage = new ContentPage
            {
                Content = grid,
                BackgroundColor = Colors.Transparent
            };

            try
            {
                if (page?.Navigation != null)
                {
                    page.Navigation.PushModalAsync(loadingPage, false);
                }
                else if (Application.Current?.MainPage?.Navigation != null)
                {
                    Application.Current.MainPage.Navigation.PushModalAsync(loadingPage, false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing loading page: {ex.Message}");
            }

            return Task.FromResult((Page)loadingPage);
        }

        /// <summary>
        /// Hides the loading overlay
        /// </summary>
        /// <param name="page">The page that showed the loading overlay</param>
        /// <param name="loadingPage">The loading page reference returned by ShowLoadingAsync</param>
        public static async Task HideLoadingAsync(this Page page, Page loadingPage)
        {
            if (loadingPage == null)
            {
                return;
            }

            try
            {
                if (page?.Navigation != null)
                {
                    await page.Navigation.RemoveModalPageAsync(loadingPage);
                }
                else if (Application.Current?.MainPage?.Navigation != null)
                {
                    await Application.Current.MainPage.Navigation.RemoveModalPageAsync(loadingPage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error hiding loading page: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes a modal page from the navigation stack
        /// </summary>
        private static async Task RemoveModalPageAsync(this INavigation navigation, Page pageToRemove)
        {
            if (navigation != null && pageToRemove != null && navigation.ModalStack.Contains(pageToRemove))
            {
                await navigation.PopModalAsync(false);
            }
        }
    }
}