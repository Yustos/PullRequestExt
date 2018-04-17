using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace YL.PullRequestViewer.Controls
{
	/// <summary>
	/// Interaction logic for InputForm.xaml
	/// </summary>
	public partial class InputForm : Window
	{
		public InputForm()
		{
			InitializeComponent();
		}

		private void OkButtonClick(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		internal static string ShowAndGetInput()
		{
			var dialog = new InputForm();
			dialog.Owner = Application.Current.MainWindow;
			if (dialog.ShowDialog().GetValueOrDefault(false))
			{
				return dialog.inputTextBox.Text;
			}
			return null;
		}
	}
}
