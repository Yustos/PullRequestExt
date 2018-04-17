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
	[ValueConversion(typeof(bool), typeof(string))]
	public class IsDeletedToTextConverter : MarkupExtension, IValueConverter
	{
		private static IsDeletedToTextConverter _instance;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var v = (bool)value;
			return v ? "X" : null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException("Cannot convert back");
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return _instance ?? (_instance = new IsDeletedToTextConverter());
		}
	}
}
