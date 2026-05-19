using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Lab01.Logic.NetworkPlanning;

namespace Lab01.App.Views;

public partial class NetworkPlanningCharts
{
    private static readonly Brush BarBrush = new SolidColorBrush(Color.FromRgb(70, 130, 220));
    private static readonly Brush SelectedBrush = new SolidColorBrush(Color.FromRgb(100, 200, 255));
    private static readonly Brush SlackBrush = new SolidColorBrush(Color.FromArgb(100, 200, 200, 200));
    private static readonly Brush LoadBrush = new SolidColorBrush(Color.FromRgb(60, 160, 90));
    private static readonly Brush TextBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230));
    private static readonly Brush GridBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));

    private NetworkSchedule? _schedule;
    private int? _selectedTaskId;

    public NetworkPlanningCharts()
    {
        InitializeComponent();
    }

    public void Render(NetworkSchedule? schedule, int? selectedTaskId)
    {
        _schedule = schedule;
        _selectedTaskId = selectedTaskId;
        Redraw();
    }

    private void NetworkPlanningCharts_OnSizeChanged(object sender, SizeChangedEventArgs e) => Redraw();

    private void Redraw()
    {
        GanttCanvas.Children.Clear();
        ResourceCanvas.Children.Clear();
        if (_schedule is null || _schedule.Result.ProjectDuration <= 0)
            return;

        DrawGantt(_schedule);
        DrawResourceLoad(_schedule);
    }

    private void DrawGantt(NetworkSchedule schedule)
    {
        CriticalPathSolveResult result = schedule.Result;
        double width = Math.Max(GanttCanvas.ActualWidth, 200);
        double height = Math.Max(GanttCanvas.ActualHeight, 200);
        double left = 56;
        double top = 28;
        double bottomPad = 24;
        var ordered = result.Tasks.OrderBy(t => t.Id).ToList();
        double rowH = Math.Max(28, (height - top - bottomPad) / ordered.Count);
        double scale = (width - left - 24) / Math.Max(1, result.ProjectDuration);

        for (int i = 0; i <= result.ProjectDuration; i += Math.Max(1, result.ProjectDuration / 10))
        {
            double x = left + i * scale;
            GanttCanvas.Children.Add(new Line
            {
                X1 = x, Y1 = top - 4, X2 = x, Y2 = height - bottomPad,
                Stroke = GridBrush, StrokeThickness = 1
            });
            var tickLabel = new TextBlock { Text = i.ToString(), Foreground = TextBrush, FontSize = 10 };
            Canvas.SetLeft(tickLabel, x - 6);
            Canvas.SetTop(tickLabel, 4);
            GanttCanvas.Children.Add(tickLabel);
        }

        for (int idx = 0; idx < ordered.Count; idx++)
        {
            NetworkTask task = ordered[idx];
            bool selected = _selectedTaskId == task.Id;
            double y = top + idx * rowH;
            int start = schedule.GetStart(task.Id);
            int finish = schedule.GetFinish(task.Id);

            var label = new TextBlock
            {
                Text = $"Р{task.Id}",
                Foreground = selected ? SelectedBrush : TextBrush,
                FontSize = 12,
                FontWeight = selected || task.IsCritical ? FontWeights.SemiBold : FontWeights.Normal
            };
            Canvas.SetLeft(label, 8);
            Canvas.SetTop(label, y + (rowH - 16) / 2);
            GanttCanvas.Children.Add(label);

            double x1 = left + start * scale;
            double w = Math.Max(3, task.Duration * scale);
            var bar = new Rectangle
            {
                Width = w,
                Height = rowH - 10,
                Fill = selected ? SelectedBrush : (task.IsCritical ? BarBrush : new SolidColorBrush(Color.FromRgb(90, 100, 150))),
                RadiusX = 3,
                RadiusY = 3,
                Stroke = selected ? Brushes.White : null,
                StrokeThickness = selected ? 2 : 0
            };
            Canvas.SetLeft(bar, x1);
            Canvas.SetTop(bar, y + 5);
            GanttCanvas.Children.Add(bar);

            if (task.Reserve > 0)
            {
                double slackW = Math.Max(2, (task.LateFinish - finish) * scale);
                var slack = new Rectangle
                {
                    Width = slackW,
                    Height = rowH - 10,
                    Fill = SlackBrush,
                    Stroke = GridBrush,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection { 4, 3 }
                };
                Canvas.SetLeft(slack, x1 + w);
                Canvas.SetTop(slack, y + 5);
                GanttCanvas.Children.Add(slack);
            }

            var people = new TextBlock
            {
                Text = $"{task.People} чол.",
                Foreground = TextBrush,
                FontSize = 10
            };
            Canvas.SetLeft(people, x1 + 6);
            Canvas.SetTop(people, y + 8);
            GanttCanvas.Children.Add(people);
        }
    }

    private void DrawResourceLoad(NetworkSchedule schedule)
    {
        CriticalPathSolveResult result = schedule.Result;
        double width = Math.Max(ResourceCanvas.ActualWidth, 200);
        double height = Math.Max(ResourceCanvas.ActualHeight, 200);
        double left = 48;
        double bottom = height - 32;
        double top = 24;
        double chartH = bottom - top;
        double scale = (width - left - 24) / Math.Max(1, result.ProjectDuration);
        var loads = ResourceLoadCalculator.Compute(schedule);
        double maxLoad = Math.Max(1, loads.Max(l => l.People));

        for (int i = 0; i <= result.ProjectDuration; i += Math.Max(1, result.ProjectDuration / 10))
        {
            double x = left + i * scale;
            ResourceCanvas.Children.Add(new Line
            {
                X1 = x, Y1 = top, X2 = x, Y2 = bottom,
                Stroke = GridBrush, StrokeThickness = 1
            });
        }

        for (int p = 0; p <= maxLoad; p++)
        {
            double y = bottom - p / maxLoad * chartH;
            ResourceCanvas.Children.Add(new Line
            {
                X1 = left, Y1 = y, X2 = width - 12, Y2 = y,
                Stroke = GridBrush, StrokeThickness = 1
            });
            var label = new TextBlock { Text = p.ToString(), Foreground = TextBrush, FontSize = 10 };
            Canvas.SetLeft(label, 8);
            Canvas.SetTop(label, y - 8);
            ResourceCanvas.Children.Add(label);
        }

        foreach ((int from, int to, int people) in loads)
        {
            double x1 = left + from * scale;
            double x2 = left + to * scale;
            double h = people / maxLoad * chartH;
            var rect = new Rectangle
            {
                Width = Math.Max(3, x2 - x1),
                Height = h,
                Fill = LoadBrush,
                RadiusX = 1,
                RadiusY = 1
            };
            Canvas.SetLeft(rect, x1);
            Canvas.SetTop(rect, bottom - h);
            ResourceCanvas.Children.Add(rect);

            var tag = new TextBlock
            {
                Text = $"{people} чол.",
                Foreground = TextBrush,
                FontSize = 9
            };
            Canvas.SetLeft(tag, x1 + 4);
            Canvas.SetTop(tag, bottom - h - 14);
            ResourceCanvas.Children.Add(tag);
        }
    }
}
