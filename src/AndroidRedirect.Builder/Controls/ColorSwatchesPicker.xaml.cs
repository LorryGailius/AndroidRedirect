using AndroidRedirect.Builder.Extensions;
using Microsoft.Maui.Controls.Shapes;

namespace AndroidRedirect.Builder.Controls
{
    public partial class ColorSwatchesPicker
    {
        public static readonly List<MauiColor> AvailableColors =
        [
            MauiColor.FromArgb("#F6C177"),
            MauiColor.FromArgb("#50C878"), 
            MauiColor.FromArgb("#EB6F92"), 
            MauiColor.FromArgb("#9CCFD8")
        ];

        public event EventHandler<ColorSelectedEventArgs>? ColorSelected;

        public ColorSwatchesPicker()
        {
            InitializeComponent();
        }

        public void Init()

        {
            LoadColorSwatches();
        }

        private int _selectedColorIndex;

        private void LoadColorSwatches()
        {
            ColorSwatchesContainer.Children.Clear();

            for (var i = 0; i < AvailableColors.Count; i++)
            {
                var color = AvailableColors[i];

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
            
            UpdateColorSwatchHighlight(_selectedColorIndex);
            NotifyColorSelected();
        }

        private void OnColorSelected(object sender, TappedEventArgs e)
        {
            if (sender is Border { BindingContext: int colorIndex and >= 0 } && 
                colorIndex < AvailableColors.Count)
            {
                _selectedColorIndex = colorIndex;
                UpdateColorSwatchHighlight(colorIndex);
                NotifyColorSelected();
            }
        }

        private void UpdateColorSwatchHighlight(int selectedIndex)
        {
            foreach (var child in ColorSwatchesContainer.Children)
            {
                if (child is Border { BindingContext: int colorIndex } border)
                {
                    if (colorIndex == selectedIndex)
                    {
                        border.StrokeThickness = 5;
                        border.Stroke = MauiColor.FromArgb("#F6C177");
                    }
                    else
                    {
                        border.StrokeThickness = 3;
                        border.Stroke = Colors.White;
                    }
                }
            }
        }

        private void NotifyColorSelected()
        {
            var foregroundColor = AvailableColors[_selectedColorIndex].ToSystemColor();
            var backgroundColor = foregroundColor.MuteColor();

            ColorSelected?.Invoke(this, new ColorSelectedEventArgs(foregroundColor, backgroundColor));
        }
    }

    public class ColorSelectedEventArgs(SystemColor foregroundColor, SystemColor backgroundColor) : EventArgs
    {
        public SystemColor ForegroundColor { get; } = foregroundColor;
        public SystemColor BackgroundColor { get; } = backgroundColor;
    }
}