using System;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;
using YL.PullRequestService;
using YL.PullRequestService.Dtos;
using System.IO;
using YL.GitRepository;
using System.Collections.ObjectModel;

namespace YL.PullRequestViewer.Controls
{
	public partial class PullRequestChangesControl : UserControl
	{
		public ObservableCollection<Change> PullRequestChanges { get; } = new ObservableCollection<Change>();

		internal event EventHandler<Change> OpenChange;

		private int _pullRequestId;

		public PullRequestChangesControl()
		{
			InitializeComponent();
			DataContext = this;
		}

		internal async void RefreshChangesData(int pullRequestId)
		{
			_pullRequestId = pullRequestId;
			var service = PullRequestServiceFactory.GetPullRequestService();
			var changes = await System.Threading.Tasks.Task.Run(() => service.GetPullRequestChanges(pullRequestId));
			PopulateListView(changes);
		}

		private void PopulateListView(Change[] changes)
		{
			PullRequestChanges.Clear();
			foreach (var prt in changes)
			{
				PullRequestChanges.Add(prt);
			}
		}

		private void ListViewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			var change = listView.SelectedItem as Change;
			if (OpenChange != null && change != null)
			{
				OpenChange(sender, change);
			}
		}

		private async void MenuItemOpenDiffClick(object sender, System.Windows.RoutedEventArgs e)
		{
			var change = listView.SelectedItem as Change;
			if (change == null)
			{
				return;
			}
			var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

			var gitConfig = ConfigReader.ReadConfig(new FileInfo(dte.Solution.FileName).DirectoryName);

			if (string.IsNullOrEmpty(change.OriginalObjectId))
			{
				OpenChange?.Invoke(sender, change);
			}
			else
			{
				var service = new PullRequestsService(gitConfig.TfsCollectionUri);
				var stream = await service.GetFileContent(gitConfig.ProjectName, gitConfig.RepositoryName, _pullRequestId, change.OriginalObjectId);
				var tmpFileName = Path.GetTempFileName();
				using (var writeStream = File.Create(tmpFileName))
				{
					stream.CopyTo(writeStream);
				}

				var gitFolder = ConfigReader.GetGitFolder(new FileInfo(dte.Solution.FileName).DirectoryName);
				var currentFilePath = Path.Combine(gitFolder.TrimSuffix(".git"), change.Path.TrimStart('/').Replace('/', '\\'));
				ShowDiff(tmpFileName, currentFilePath);
			}
		}

		private void ShowDiff(string originalFilePath, string currentFilePath)
		{
			var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
			dte.ExecuteCommand("Tools.DiffFiles",
				string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\"",
					originalFilePath, currentFilePath,
					"Original", "Current"));
		}
	}
}
