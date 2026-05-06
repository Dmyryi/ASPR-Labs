using System.Globalization;
using System.Windows.Data;

namespace Lab01.App.Converters;

/// <summary>
/// Повертає <c>true</c>, якщо значення (наприклад, SelectedIndex) дорівнює числу,
/// що передане у параметрі. Використовується для підсвічування активного пункту меню.
/// </summary>
public sealed class IndexEqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null || parameter is null) return false;
        if (!int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var current)) return false;
        if (!int.TryParse(parameter.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var expected)) return false;
        return current == expected;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
