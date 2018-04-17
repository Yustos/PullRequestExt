using System;
using System.Windows.Controls;
using System.Collections;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using YL.PullRequestService;
using YL.PullRequestService.Dtos;
using System.IO;
using YL.GitRepository;
using System.Collections.ObjectModel;
using YL.PullRequestViewer.Controls.EventArguments;
using System.Threading.Tasks;
using YL.PullRequestViewer.Controls.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace YL.PullRequestViewer.Controls
{
	public partial class PullRequestsControl : UserControl, IRefreshControl
	{
		public ObservableCollection<PullRequestViewModel> PullRequests { get; } = new ObservableCollection<PullRequestViewModel>();

		internal event EventHandler<OpenPullRequestEventArguments> OpenPullRequestThreads;
		internal event EventHandler<int> OpenPullRequestChanges;

		private readonly Action<string> _log;
		private readonly Action _showProperties;
		private readonly SelectionContainer _selectionContainer = new SelectionContainer();
		private readonly List<CustomTypeDescriptorWrapper<PullRequest>> _wrappers = new List<CustomTypeDescriptorWrapper<PullRequest>>();
		private ITrackSelection _trackSelection;
		private bool _selectionFreeze;

		public PullRequestsControl(Action<string> log, Action showProperties)
		{
			InitializeComponent();
			DataContext = this;
			_log = log;
			_showProperties = showProperties;
		}

		public void Refresh()
		{
			var service = GetPullRequestService();
			if (service == null)
			{
				_log("Git repository missing");
				return;
			}
			var gitConfig = GetGitConfig();
			System.Threading.Tasks.Task.Run(() => service.GetBranchPullRequests())
				.ContinueWith((t) =>
				{
					PopulateListView(t.Result, gitConfig.BranchName);
				}, TaskScheduler.FromCurrentSynchronizationContext());
		}

		internal void InitTrackSelection(ITrackSelection trackSelection)
		{
			_trackSelection = trackSelection;
			_selectionContainer.SelectableObjects = null;
			_selectionContainer.SelectedObjects = null;
			_trackSelection.OnSelectChange(_selectionContainer);
			_selectionContainer.SelectedObjectsChanged += SelectedObjectsChanged;
		}

		private void PopulateListView(PullRequest[] pullRequests, string currentGitRefName)
		{
			PullRequests.Clear();
			_wrappers.Clear();
			foreach (var pr in pullRequests)
			{
				PullRequests.Add(MapPullRequest(pr, currentGitRefName));
				_wrappers.Add(new CustomTypeDescriptorWrapper<PullRequest>(pr, $"{pr.Id}: {pr.Title}"));
			}
			listView.SelectedItems.Clear();
		}

		private PullRequestViewModel MapPullRequest(PullRequest pullRequest, string currentGitRefName)
		{
			return new PullRequestViewModel(pullRequest, MapPullRequestType(pullRequest, currentGitRefName));
		}

		private PullRequestType MapPullRequestType(PullRequest pullRequest, string currentGitRefName)
		{
			switch (pullRequest)
			{
				case var _ when pullRequest.SourceRefName == currentGitRefName && pullRequest.TargetRefName == currentGitRefName:
					return PullRequestType.Inner;
				case var _ when pullRequest.SourceRefName == currentGitRefName:
					return PullRequestType.Outbound;
				case var _ when pullRequest.TargetRefName == currentGitRefName:
					return PullRequestType.Inbound;
				default:
					return PullRequestType.Common;
			}
		}

		private void ListViewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (OpenPullRequestThreads != null)
			{
				var pr = listView.SelectedItem as PullRequestViewModel;
				OpenPullRequestThreads(sender, new OpenPullRequestEventArguments(pr.PullRequest.Id, false));
			}
		}

		private void MenuItemOpenChangesClick(object sender, System.Windows.RoutedEventArgs e)
		{
			if (OpenPullRequestChanges != null)
			{
				var pr = listView.SelectedItem as PullRequestViewModel;
				OpenPullRequestChanges(sender, pr.PullRequest.Id);
			}
		}

		private GitConfig GetGitConfig()
		{
			var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
			if (string.IsNullOrEmpty(dte.Solution.FileName))
			{
				return null;
			}
			var gitConfig = ConfigReader.ReadConfig(new FileInfo(dte.Solution.FileName).DirectoryName);
			return gitConfig;
		}

		private PullRequestsRepositoryService GetPullRequestService()
		{
			var gitConfig = GetGitConfig();
			if (gitConfig == null)
			{
				return null;
			}
			return new PullRequestsRepositoryService(gitConfig.TfsCollectionUri, gitConfig.ProjectName, gitConfig.RepositoryName);
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
				.OfType<CustomTypeDescriptorWrapper<PullRequest>>()
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
