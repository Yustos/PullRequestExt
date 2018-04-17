using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using YL.PullRequestService.Dtos;
using YL.PullRequestViewer.Controls.ViewModels;

namespace YL.PullRequestViewer.Controls.Converters
{
	[ValueConversion(typeof(PullRequestType), typeof(string))]
	public class PullRequestTypeToTextConverter : MarkupExtension, IValueConverter
	{
		private static PullRequestTypeToTextConverter _instance;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch ((PullRequestType)value)
			{
				case PullRequestType.Common:
					return ".";
				case PullRequestType.Inner:
					return "!";
				case PullRequestType.Inbound:
					return "->";
				case PullRequestType.Outbound:
					return "<-";
				default:
					return "?";
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Enum.Parse(typeof(PullRequestType), value as string);
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return _instance ?? (_instance = new PullRequestTypeToTextConverter());
		}
	}
}
