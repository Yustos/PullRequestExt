using System;
using System.Linq;
using System.Windows.Controls;
using YL.PullRequestService.Dtos;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using YL.PullRequestViewer.Controls.EventArguments;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;

namespace YL.PullRequestViewer.Controls
{
	public partial class PullRequestThreadsControl : UserControl
	{
		public ObservableCollection<PullRequestThread> PullRequestThreads { get; } = new ObservableCollection<PullRequestThread>();

		internal event EventHandler<PullRequestThread> OpenPullRequestThread;

		internal event EventHandler<OpenPullRequestEventArguments> OpenPullRequestThreads;

		internal event EventHandler<PullRequestThread> CreatePullRequestThread;

		public EventHandler<PullRequestThread> OpenComments;

		private readonly Microsoft.VisualStudio.Shell.SelectionContainer _selectionContainer = new Microsoft.VisualStudio.Shell.SelectionContainer();
		private readonly List<CustomTypeDescriptorWrapper<PullRequestThread>> _wrappers = new List<CustomTypeDescriptorWrapper<PullRequestThread>>();
		private readonly Action _showProperties;
		private ITrackSelection _trackSelection;
		private int _pullRequestId;
		private bool _showAll;
		private bool _selectionFreeze;

		public bool ShowAll
		{
			get { return _showAll; }
			set
			{
				_showAll = value;
				OnPropertyChanged();
				OpenPullRequestThreads?.Invoke(this, new OpenPullRequestEventArguments(_pullRequestId, _showAll));
			}
		}

		public PullRequestThreadsControl(Action showProperties)
		{
			InitializeComponent();
			_showProperties = showProperties;
			DataContext = this;
		}

		internal void InitTrackSelection(ITrackSelection trackSelection)
		{
			_trackSelection = trackSelection;
			_selectionContainer.SelectableObjects = null;
			_selectionContainer.SelectedObjects = null;
			_trackSelection.OnSelectChange(_selectionContainer);
			_selectionContainer.SelectedObjectsChanged += SelectedObjectsChanged;
		}

		internal void RefreshThreadsData(int pullRequestId, bool showInactive)
		{
			_pullRequestId = pullRequestId;
			var service = PullRequestServiceFactory.GetPullRequestService();
			Task.Run(() => service.GetPullRequestThreads(pullRequestId, !showInactive)).
				ContinueWith(t => PopulateListView(t.Result), TaskScheduler.FromCurrentSynchronizationContext());
		}

		private void PopulateListView(PullRequestThread[] pullRequestThreads)
		{
			PullRequestThreads.Clear();
			_wrappers.Clear();
			foreach (var prt in pullRequestThreads)
			{
				PullRequestThreads.Add(prt);
				_wrappers.Add(new CustomTypeDescriptorWrapper<PullRequestThread>(prt, $"{prt.Id}: {prt.Title}"));
			}
		}

		private void ListViewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var prThread = listView.SelectedItem as PullRequestThread;
			if (OpenPullRequestThread != null && prThread != null)
			{
				OpenPullRequestThread(sender, prThread);
			}
		}

		private void MenuItemClick(object sender, System.Windows.RoutedEventArgs e)
		{
			var prThread = listView.SelectedItem as PullRequestThread;
			if (OpenComments != null && prThread != null)
			{
				OpenComments(sender, prThread);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName]string prop = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
		}

		private void MenuItemSetThreadStateClick(object sender, System.Windows.RoutedEventArgs e)
		{
			var prThread = listView.SelectedItem as PullRequestThread;
			if (prThread == null)
			{
				return;
			}

			var menuItem = sender as MenuItem;
			var status = (ThreadStatus)Enum.Parse(typeof(ThreadStatus), menuItem.Tag as string);

			var service = PullRequestServiceFactory.GetPullRequestService();
			Task.Run(async () =>
			{
				await service.SetPullRequestThreadStatus(prThread.PullRequestId, prThread.Id, status);
				return await service.GetPullRequestThreads(prThread.PullRequestId, !ShowAll);
			})
			.ContinueWith(t => PopulateListView(t.Result), TaskScheduler.FromCurrentSynchronizationContext());
		}

		private void MenuItemThreadReplyClick(object sender, System.Windows.RoutedEventArgs e)
		{
			var text = InputForm.ShowAndGetInput();
			if (!string.IsNullOrEmpty(text))
			{
				var prThread = listView.SelectedItem as PullRequestThread;
				if (prThread == null)
				{
					return;
				}

				var service = PullRequestServiceFactory.GetPullRequestService();

				Task.Run(async () =>
				{
					await service.ReplyToPullRequestThread(prThread.PullRequestId, prThread.Id, text);
					return await service.GetPullRequestThreads(prThread.PullRequestId, !ShowAll);
				})
				.ContinueWith(t =>
				{
					PopulateListView(t.Result);
				}, TaskScheduler.FromCurrentSynchronizationContext());
			}
		}

		private void MenuItemCreateThreadClick(object sender, System.Windows.RoutedEventArgs e)
		{
			var text = InputForm.ShowAndGetInput();
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			var prThread = listView.SelectedItem as PullRequestThread;
			if (prThread == null)
			{
				return;
			}

			if (CreatePullRequestThread == null)
			{
				return;
			}
			CreatePullRequestThread(this, prThread);
		}

		private void MenuItemPropertiesClick(object sender, System.Windows.RoutedEventArgs e)
		{
			_showProperties();
			SelectionChanged();
		}

		private void SelectedObjectsChanged(object sender, EventArgs e)
		{
			if (_selectionFreeze)
			{
				return;
			}
			_selectionFreeze = true;
			var selectedWrapper = _selectionContainer.SelectedObjects
				.OfType<CustomTypeDescriptorWrapper<PullRequestThread>>()
				.FirstOrDefault();
			if (selectedWrapper != null)
			{
				var idx = _wrappers.IndexOf(selectedWrapper);
				listView.SelectedIndex = idx;
			}
			_selectionFreeze = false;
		}

		private void ListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectionChanged();
		}

		private void SelectionChanged()
		{
			if (_selectionFreeze || listView.SelectedIndex == -1)
			{
				return;
			}
			_selectionFreeze = true;
			var wrapper = _wrappers[listView.SelectedIndex];
			_selectionContainer.SelectedObjects = new[] { wrapper };
			_selectionContainer.SelectableObjects = _wrappers;
			_trackSelection.OnSelectChange(_selectionContainer);
			_selectionFreeze = false;
		}
	}
}
