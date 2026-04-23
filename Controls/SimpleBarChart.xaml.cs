using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ProjectTest.Models;
using Windows.UI;

namespace ProjectTest.Controls;

public sealed partial class SimpleBarChart : UserControl
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<BarChartItem>), typeof(SimpleBarChart), new PropertyMetadata(null, OnItemsSourceChanged));

    public SimpleBarChart()
    {
        InitializeComponent();
        Loaded += (_, _) => Render();
        SizeChanged += (_, _) => Render();
    }

    public IEnumerable<BarChartItem>? ItemsSource
    {
        get => (IEnumerable<BarChartItem>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SimpleBarChart)d).Render();
    }

    private void Render()
    {
        BarsPanel.Children.Clear();

        var items = ItemsSource?.ToList() ?? [];
        if (items.Count == 0)
        {
            return;
        }

        var maxValue = Math.Max(1d, items.Max(x => x.Value));
        var availableWidth = Math.Max(180, ActualWidth - 220);

        foreach (var item in items)
        {
            var row = new Grid
            {
                ColumnSpacing = 12
            };

            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(170) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var labelStack = new StackPanel { Spacing = 2 };
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

            var barHost = new Border
            {
                Height = 18,
                Background = new SolidColorBrush(Color.FromArgb(30, 28, 107, 93)),
                CornerRadius = new CornerRadius(9),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var barFill = new Border
            {
                Height = 18,
                Width = availableWidth * (item.Value / maxValue),
                Background = new SolidColorBrush(Color.FromArgb(255, 28, 107, 93)),
                CornerRadius = new CornerRadius(9),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            var barGrid = new Grid();
            barGrid.Children.Add(barHost);
            barGrid.Children.Add(barFill);

            row.Children.Add(labelStack);
            row.Children.Add(barGrid);
            var valueText = new TextBlock
            {
                Text = item.ValueLabel,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            };

            row.Children.Add(valueText);

            Grid.SetColumn(barGrid, 1);
            Grid.SetColumn(valueText, 2);
            BarsPanel.Children.Add(row);
        }
    }
}
