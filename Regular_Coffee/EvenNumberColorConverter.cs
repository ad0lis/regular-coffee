namespace Regular_Coffee
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    public class EvenNumberColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value % 2 == 0 ? new SolidColorBrush(ProjectColors.Blue.ToSWMColor()) : new SolidColorBrush(ProjectColors.Orange.ToSWMColor());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
