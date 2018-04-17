using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace YL.PullRequestViewer.Controls.Converters
{
	[ValueConversion(typeof(bool), typeof(SolidColorBrush))]
	public class IsDeletedColorConverter : MarkupExtension, IValueConverter
	{
		private static IsDeletedColorConverter _instance;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var v = (bool)value;
			return v ? new SolidColorBrush(Colors.Red) : null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("Cannot convert back");
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return _instance ?? (_instance = new IsDeletedColorConverter());
		}
	}
}
