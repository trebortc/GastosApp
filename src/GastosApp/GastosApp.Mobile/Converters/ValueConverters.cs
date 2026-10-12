using System.Globalization;

namespace GastosApp.Mobile.Converters;

/// <summary>Devuelve true si el valor no es null ni una cadena vacia.</summary>
public sealed class NotNullToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s ? !string.IsNullOrEmpty(s) : value is not null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Divide el valor decimal entre 100 para usarlo como Progress (0-1).</summary>
public sealed class DivideBy100Converter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double d ? d / 100.0 : 0.0;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Convierte bool a string. ConverterParameter="TextoTrue|TextoFalso"
/// </summary>
public sealed class BoolToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var parts = (parameter as string ?? "|").Split('|');
        return value is true ? (parts.Length > 0 ? parts[0] : string.Empty)
                             : (parts.Length > 1 ? parts[1] : string.Empty);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
