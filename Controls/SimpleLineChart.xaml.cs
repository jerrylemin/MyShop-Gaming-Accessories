using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using ProjectTest.Models;
using Windows.Foundation;
using Windows.UI;

namespace ProjectTest.Controls;

public sealed partial class SimpleLineChart : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<ChartPoint>), typeof(SimpleLineChart), new PropertyMetadata(null, OnItemsSourceChanged));

    public SimpleLineChart()
    {
        InitializeComponent();
        Loaded += (_, _) => Render();
    }

    public IEnumerable<ChartPoint>? ItemsSource
    {
        get => (IEnumerable<ChartPoint>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SimpleLineChart)d).Render();
    }

    private void ChartCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        Render();
    }

    private void Render()
    {
        if (ChartCanvas.ActualWidth <= 0 || ChartCanvas.ActualHeight <= 0)
        {
            return;
        }

        ChartCanvas.Children.Clear();
        LabelsGrid.Children.Clear();
        LabelsGrid.ColumnDefinitions.Clear();

        var points = ItemsSource?.ToList() ?? [];
        if (points.Count == 0)
        {
            return;
        }

        var width = ChartCanvas.ActualWidth;
        var height = ChartCanvas.ActualHeight;
        var chartPadding = 16d;
        var drawableWidth = Math.Max(1, width - chartPadding * 2);
        var drawableHeight = Math.Max(1, height - chartPadding * 2);
        var maxValue = Math.Max(1d, points.Max(x => x.Value));
        var strokeBrush = new SolidColorBrush(Color.FromArgb(255, 28, 107, 93));
        var markerBrush = new SolidColorBrush(Color.FromArgb(255, 196, 106, 45));

        for (var i = 0; i < 4; i++)
        {
            var y = chartPadding + (drawableHeight / 3d) * i;
            ChartCanvas.Children.Add(new Line
            {
                X1 = chartPadding,
                X2 = chartPadding + drawableWidth,
                Y1 = y,
                Y2 = y,
                Stroke = new SolidColorBrush(Color.FromArgb(40, 50, 50, 50)),
                StrokeThickness = 1
            });
        }

        var polyline = new Polyline
        {
            Stroke = strokeBrush,
            StrokeThickness = 3,
            StrokeLineJoin = PenLineJoin.Round
        };

        for (var index = 0; index < points.Count; index++)
        {
            var point = points[index];
            var x = chartPadding + (points.Count == 1 ? drawableWidth / 2d : drawableWidth * index / (points.Count - 1d));
            var y = chartPadding + drawableHeight - (point.Value / maxValue * drawableHeight);
            var chartPoint = new Point(x, y);
            polyline.Points.Add(chartPoint);

            var marker = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = markerBrush
            };

            Canvas.SetLeft(marker, x - 5);
            Canvas.SetTop(marker, y - 5);
            ChartCanvas.Children.Add(marker);
        }

        ChartCanvas.Children.Add(polyline);

        for (var index = 0; index < points.Count; index++)
        {
            LabelsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            var label = new TextBlock
            {
                Text = points[index].Label,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"]
            };

            Grid.SetColumn(label, index);
            LabelsGrid.Children.Add(label);
        }
    }
}
