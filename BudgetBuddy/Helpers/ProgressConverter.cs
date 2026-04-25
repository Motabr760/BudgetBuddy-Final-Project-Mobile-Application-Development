using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace BudgetBuddy.Helpers;

public class ProgressConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2) return 0;

        double current = System.Convert.ToDouble(values[0]);
        double target = System.Convert.ToDouble(values[1]);

        if (target == 0) return 0;
        return current / target;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
