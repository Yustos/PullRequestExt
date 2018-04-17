using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using YL.PullRequestService.Dtos;

namespace YL.PullRequestViewer.Controls.Converters
{
	[ValueConversion(typeof(ThreadStatus), typeof(string))]
	public class StatusToTextConverter : MarkupExtension, IValueConverter
	{
		private static StatusToTextConverter _instance;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var status = (ThreadStatus)value;
			return status.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Enum.Parse(typeof(ThreadStatus), value as string);
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return _instance ?? (_instance = new StatusToTextConverter());
		}
	}
}
