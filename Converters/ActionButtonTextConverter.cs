using System;
using System.Globalization;
using System.Windows.Data;

namespace structIQe_Application_Manager.Launcher.Converters
{
    public class ActionButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var app = value as dynamic;
            if (app == null)
                return "Install";
            if (app.IsInstalled && app.IsUpdateAvailable) return "Update";
            if (app.IsInstalled) return "Installed";
            return "Install";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
