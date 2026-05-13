using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using ProjectTest.Models;
using Windows.Foundation;
using Windows.UI;

namespace ProjectTest.Controls;

public sealed partial class SimplePieChart : UserControl
{
    private const int MaxRenderedItems = 6;

    private static readonly Color[] Palette =
    [
        Color.FromArgb(255, 28, 107, 93),
        Color.FromArgb(255, 196, 106, 45),
        Color.FromArgb(255, 55, 88, 154),
        Color.FromArgb(255, 140, 73, 129),
        Color.FromArgb(255, 108, 148, 60),
        Color.FromArgb(255, 204, 89, 89)
    ];

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<PieChartItem>), typeof(SimplePieChart), new PropertyMetadata(null, OnItemsSourceChanged));

    public SimplePieChart()
    {
        InitializeComponent();
        Loaded += (_, _) => Render();
    }

    public IEnumerable<PieChartItem>? ItemsSource
    {
        get => (IEnumerable<PieChartItem>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SimplePieChart)d).Render();
    }

    private void Render()
    {
        PieCanvas.Children.Clear();
        LegendPanel.Children.Clear();

        var items = ItemsSource?.Where(x => x.Value > 0).Take(MaxRenderedItems).ToList() ?? [];
        if (items.Count == 0)
        {
            LegendPanel.Children.Add(new TextBlock
            {
                Text = "No data in the selected date range.",
                Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"]
            });
            return;
        }

        var total = items.Sum(x => x.Value);
        var startAngle = -90d;
        const double size = 240d;
        const double radius = 120d;
        const double center = size / 2d;

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var sweepAngle = 360d * (item.Value / total);
            var color = Palette[index % Palette.Length];
            var path = new Microsoft.UI.Xaml.Shapes.Path
            {
                Fill = new SolidColorBrush(color),
                Data = CreateSliceGeometry(center, center, radius, startAngle, sweepAngle)
            };

            PieCanvas.Children.Add(path);
            startAngle += sweepAngle;

            var legendRow = new Grid
            {
                ColumnSpacing = 10
            };

            legendRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            legendRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            legendRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            legendRow.Children.Add(new Border
            {
                Width = 14,
                Height = 14,
                CornerRadius = new CornerRadius(7),
                Background = new SolidColorBrush(color),
                VerticalAlignment = VerticalAlignment.Center
            });

            var labelStack = new StackPanel
            {
                Spacing = 2
            };

            labelStack.Children.Add(new TextBlock
            {
                Text = item.Label,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            if (!string.IsNullOrWhiteSpace(item.Subtitle))
            {
                labelStack.Children.Add(new TextBlock
                {
                    Text = item.Subtitle,
                    Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"]
                });
            }

            Grid.SetColumn(labelStack, 1);
            legendRow.Children.Add(labelStack);

            var percent = Math.Round(item.Value / total * 100d, 1);
            var valueText = new TextBlock
            {
                Text = $"{item.ValueLabel} ({percent:0.#}%)",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(valueText, 2);
            legendRow.Children.Add(valueText);
            LegendPanel.Children.Add(legendRow);
        }
    }

    private static Geometry CreateSliceGeometry(double centerX, double centerY, double radius, double startAngle, double sweepAngle)
    {
        if (sweepAngle >= 359.99d)
        {
            return new EllipseGeometry
            {
                Center = new Point(centerX, centerY),
                RadiusX = radius,
                RadiusY = radius
            };
        }

        var startRadians = Math.PI * startAngle / 180d;
        var endRadians = Math.PI * (startAngle + sweepAngle) / 180d;
        var startPoint = new Point(centerX + radius * Math.Cos(startRadians), centerY + radius * Math.Sin(startRadians));
        var endPoint = new Point(centerX + radius * Math.Cos(endRadians), centerY + radius * Math.Sin(endRadians));

        var figure = new PathFigure
        {
            StartPoint = new Point(centerX, centerY),
            IsClosed = true
        };

        figure.Segments.Add(new LineSegment { Point = startPoint });
        figure.Segments.Add(new ArcSegment
        {
            Point = endPoint,
            Size = new Size(radius, radius),
            IsLargeArc = sweepAngle > 180d,
            SweepDirection = SweepDirection.Clockwise
        });

        var geometry = new PathGeometry();
        geometry.Figures.Add(figure);
        return geometry;
    }
}
