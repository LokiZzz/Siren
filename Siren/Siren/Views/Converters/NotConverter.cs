using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace Siren.Views.Converters
{
    public class NotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? val = value as bool?;

            return !val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            bool? val = value as bool?;

            return !val;
        }
    }
}
