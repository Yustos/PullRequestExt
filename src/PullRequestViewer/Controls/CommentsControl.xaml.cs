using System;
using System.Windows.Controls;
using System.Collections;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using YL.PullRequestService;
using Microsoft.VisualStudio.TeamFoundation;
using YL.PullRequestService.Dtos;
using System.IO;
using YL.GitRepository;
using System.Collections.ObjectModel;

namespace YL.PullRequestViewer.Controls
{
	public partial class CommentsControl : UserControl
	{
		private readonly ObservableCollection<Comment> _comments =
new ObservableCollection<Comment>();

		public ObservableCollection<Comment> Comments
		{
			get { return _comments; }
		}

		public CommentsControl()
		{
			InitializeComponent();
			this.DataContext = this;
		}

		internal void RefreshCommentsData(int pullRequestId, int threadId)
		{
			var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

			var gitConfig = ConfigReader.ReadConfig(new FileInfo(dte.Solution.FileName).DirectoryName);
			
			var service = new PullRequestsService(gitConfig.TfsCollectionUri);
			var comments = service.GetComments(gitConfig.ProjectName, gitConfig.RepositoryName, pullRequestId, threadId);
			PopulateListView(comments);
		}

		private ProjectContextExt GetCurrentProjectContext()
		{
			var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

			TeamFoundationServerExt ext = dte.GetObject("Microsoft.VisualStudio.TeamFoundation.TeamFoundationServerExt") as TeamFoundationServerExt;

			if (ext.ActiveProjectContext == null ||
				string.IsNullOrEmpty(ext.ActiveProjectContext.DomainUri) ||
				string.IsNullOrEmpty(ext.ActiveProjectContext.ProjectName))
			{
				return null;
			}
			return ext.ActiveProjectContext;
		}

		private void PopulateListView(Comment[] comments)
		{
			_comments.Clear();
			foreach (var prt in comments)
			{
				_comments.Add(prt);
			}
		}
	}
}
