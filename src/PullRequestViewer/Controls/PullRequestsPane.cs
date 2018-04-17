using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using YL.PullRequestService.Dtos;
using Microsoft.VisualStudio;
using System.IO;
using Microsoft.VisualStudio.TextManager.Interop;
using YL.PullRequestViewer.Controls.EventArguments;
using System.Windows.Controls;

namespace YL.PullRequestViewer.Controls
{
	[Guid("D97D905B-2E4B-406F-814B-DF60FC4FF9D9")]
	class PullRequestsPane : ToolWindowPane
	{
		private readonly PullRequestsControl _pullRequestsControl;
		private readonly PullRequestThreadsControl _pullRequestThreadsControl;
		private readonly CommentsControl _commentsControl;
		private readonly PullRequestChangesControl _changesControl;
		private readonly PullRequestsHost _host;
		private readonly Lazy<IVsOutputWindowPane> _outputWindowsPane;

		private IVsOutputWindowPane OutputWindowPane => _outputWindowsPane.Value;

		private UserControl _currentControl;
		private int _currentPullRequestId;

		public PullRequestsPane()
			: base(null)
		{
			_outputWindowsPane = new Lazy<IVsOutputWindowPane>(CreateOutputWindowPane);

			BitmapImageMoniker = Microsoft.VisualStudio.Imaging.KnownMonikers.Search;
			ToolBar = new CommandID(GuidsList.guidClientCmdSet, PkgCmdId.IDM_PullRequestsToolbar);
			ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
			_pullRequestsControl = new PullRequestsControl(WriteOutput, ShowPropertiesWindowPane);
			_pullRequestsControl.OpenPullRequestThreads += PullRequestsControlOpenPullRequest;
			_pullRequestsControl.OpenPullRequestChanges += PullRequestsControlOpenPullRequestChanges;
			_pullRequestThreadsControl = new PullRequestThreadsControl(ShowPropertiesWindowPane);
			_pullRequestThreadsControl.OpenPullRequestThread += PullRequestThreadsControlOpenPullRequest;
			_pullRequestThreadsControl.OpenComments += PullRequestThreadsControlOpenComments;
			_pullRequestThreadsControl.OpenPullRequestThreads += PullRequestsControlOpenPullRequest;
			_commentsControl = new CommentsControl();
			_changesControl = new PullRequestChangesControl();
			_changesControl.OpenChange += ChangesControlOpenChange;
			_host = new PullRequestsHost();
			_host.frame.Navigate(_pullRequestsControl);
			Content = _host;
		}

		public override void OnToolWindowCreated()
		{
			base.OnToolWindowCreated();

			var package = (PullRequestPluginPackage)Package;

			_pullRequestsControl.InitTrackSelection((ITrackSelection)GetService(typeof(STrackSelection)));
			_pullRequestThreadsControl.InitTrackSelection((ITrackSelection)GetService(typeof(STrackSelection)));

			Caption = package.GetResourceString("@100");

			BindCommandHandler(Home, PkgCmdId.cmdidHome);
			BindCommandHandler(Refresh, PkgCmdId.cmdidRefresh);
			BindCommandHandler(Backward, PkgCmdId.cmdidBackward);
			BindCommandHandler(CreatePullRequestThread, PkgCmdId.cmdidCreatePullRequestThread);

			Navigate(_pullRequestsControl);
			_pullRequestsControl.Refresh();

			var solutionService = GetService(typeof(SVsSolution)) as IVsSolution;
			var seHandler = new SolutionEventsHandler(() =>
			{
				Navigate(_pullRequestsControl);
				_pullRequestsControl.Refresh();
			});
			solutionService.AdviseSolutionEvents(seHandler, out uint cookie);
		}

		private void BindCommandHandler(EventHandler handler, int commandId)
		{
			DefineCommandHandler(handler, new CommandID(GuidsList.guidClientCmdSet, commandId));
		}

		private OleMenuCommand DefineCommandHandler(EventHandler handler, CommandID id)
		{
			var package = (PullRequestPluginPackage)Package;
			var command = package.DefineCommandHandler(handler, id);
			if (command == null)
				return command;

			if (GetService(typeof(IMenuCommandService)) is OleMenuCommandService menuService)
			{
				menuService.AddCommand(command);
			}
			return command;
		}

		private void Navigate(UserControl control)
		{
			_host.frame.Navigate(control);
			_currentControl = control;
		}

		private void Home(object sender, EventArgs arguments)
		{
			Navigate(_pullRequestsControl);
		}

		private void Refresh(object sender, EventArgs args)
		{
			if (_currentControl is IRefreshControl refreshControl)
			{
				refreshControl.Refresh();
			}
		}

		private void Backward(object sender, EventArgs args)
		{
			_host.frame.Navigate(_pullRequestsControl);
		}

		private void CreatePullRequestThread(object sender, EventArgs args)
		{
			var text = InputForm.ShowAndGetInput();
			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			System.Threading.Tasks.Task.Run(() => CreateThreadForCurrentDocumentAndPosition(text, _currentPullRequestId))
				.ContinueWith((t) =>
				{
					Refresh(null, null);
				}, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());
		}

		private void PullRequestsControlOpenPullRequest(object sender, OpenPullRequestEventArguments args)
		{
			_currentPullRequestId = args.PullRequestId;
			_host.frame.Navigate(_pullRequestThreadsControl);
			_pullRequestThreadsControl.RefreshThreadsData(args.PullRequestId, args.ShowInactive);
		}

		private void PullRequestThreadsControlOpenComments(object sender, PullRequestThread e)
		{
			_host.frame.Navigate(_commentsControl);
			_commentsControl.RefreshCommentsData(e.PullRequestId, e.Id);
		}

		private void PullRequestThreadsControlOpenPullRequest(object sender, PullRequestThread pullRequestThread)
		{
			var gitFolder = PullRequestConfigFactory.GetGitFolder();
			var filePath = Path.Combine(gitFolder.TrimSuffix(".git"), pullRequestThread.FilePath.TrimStart('/').Replace('/', '\\'));
			OpenFilePosition(filePath, pullRequestThread.Start, pullRequestThread.End);
		}

		private void PullRequestsControlOpenPullRequestChanges(object sender, int pullRequestId)
		{
			_currentPullRequestId = pullRequestId;
			_host.frame.Navigate(_changesControl);
			_changesControl.RefreshChangesData(pullRequestId);
		}

		private void ChangesControlOpenChange(object sender, Change change)
		{
			var gitFolder = PullRequestConfigFactory.GetGitFolder();
			var filePath = Path.Combine(gitFolder.TrimSuffix(".git"), change.Path.TrimStart('/').Replace('/', '\\'));
			OpenFilePosition(filePath, default(Position), default(Position));
		}

		private async void CreateThreadForCurrentDocumentAndPosition(string text, int pullRequestId)
		{
			var dte = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
			EnvDTE.TextSelection te = dte.ActiveDocument.Selection;

			var service = PullRequestServiceFactory.GetPullRequestService();

			var start = te.BottomPoint.AbsoluteCharOffset > te.TopPoint.AbsoluteCharOffset
				? te.TopPoint : te.BottomPoint;
			var end = te.BottomPoint.AbsoluteCharOffset > te.TopPoint.AbsoluteCharOffset
				? te.BottomPoint : te.TopPoint;

			var projectFolder = PackageUtilities.TrimSuffix(PullRequestConfigFactory.GetGitFolder(), ".git");
			var filePath = new Uri(dte.ActiveDocument.FullName);
			var projectPath = new Uri(projectFolder);
			var relativePath = Path.Combine("/", Uri.UnescapeDataString(projectPath.MakeRelativeUri(filePath).ToString()));

			await service.CreatePullRequestThread(pullRequestId,
					relativePath,
					text,
					new Position(start.Line, start.LineCharOffset),
					new Position(end.Line, end.LineCharOffset));
		}

		private bool OpenFile(string filePath, out IVsWindowFrame frame, out Guid logicalView)
		{
			var openDoc =
				Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(IVsUIShellOpenDocument))
				as IVsUIShellOpenDocument;

			logicalView = VSConstants.LOGVIEWID_Code;
			if (ErrorHandler.Failed(
			  openDoc.OpenDocumentViaProject(filePath, ref logicalView, out Microsoft.VisualStudio.OLE.Interop.IServiceProvider sp,
				out IVsUIHierarchy hier, out uint itemid, out frame))
				  || frame == null)
			{
				return false;
			}
			return true;
		}

		private void OpenFilePosition(string filePath, Position start, Position end)
		{
			if (!OpenFile(filePath, out IVsWindowFrame frame, out Guid logicalView))
			{
				WriteOutput($"File missing: {filePath}");
				return;
			}

			frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out object docData);

			var buffer = GetTextBuffer(docData);
			NavigateToLine(buffer, logicalView, start, end);
		}

		private IVsTextBuffer GetTextBuffer(object docData)
		{
			var buffer = docData as VsTextBuffer;
			if (buffer == null)
			{
				if (docData is IVsTextBufferProvider bufferProvider)
				{
					ErrorHandler.ThrowOnFailure(bufferProvider.GetTextBuffer(
						out IVsTextLines lines));
					buffer = lines as VsTextBuffer;
					if (buffer == null)
					{
						return null;
					}
				}
			}
			return buffer;
		}

		private void NavigateToLine(IVsTextBuffer buffer, Guid logicalView, Position start, Position end)
		{
			var mgr = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(VsTextManagerClass)) as IVsTextManager;
			if (start == null && end == null)
			{
				mgr.NavigateToPosition(buffer, ref logicalView, 0, 0);
			}
			else
			{
				start = start ?? end;
				end = end ?? start;
				mgr.NavigateToLineAndColumn(buffer, ref logicalView, start.Line - 1,
					start.Offset, end.Line - 1, end.Offset);
			}
		}

		private void WriteOutput(string message)
		{
			OutputWindowPane.OutputString(message);
			OutputWindowPane.OutputString(Environment.NewLine);
		}

		private IVsOutputWindowPane CreateOutputWindowPane()
		{
			IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
			var outputWindowGuid = GuidsList.guidOutputWindowFrame;
			ErrorHandler.ThrowOnFailure(uiShell.FindToolWindow((uint)__VSCREATETOOLWIN.CTW_fForceCreate, ref outputWindowGuid,
				out IVsWindowFrame outputWindowFrame));
			if (outputWindowFrame != null)
				outputWindowFrame.Show();

			var outputWindow = (IVsOutputWindow)GetService(typeof(SVsOutputWindow));
			var paneGuid = new Guid("098BA943-8273-4766-BFA5-7DCC45E241FE");
			var package = (PullRequestPluginPackage)Package;
			string paneName = package.GetResourceString("PullRequestEvents");
			ErrorHandler.ThrowOnFailure(outputWindow.CreatePane(ref paneGuid, paneName, 1, 0));
			ErrorHandler.ThrowOnFailure(outputWindow.GetPane(ref paneGuid, out IVsOutputWindowPane outputWindowPane));
			return outputWindowPane;
		}

		private void ShowPropertiesWindowPane()
		{
			IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
			var propertiesWindowGuid = GuidsList.guidPropertiesWindowFrame;
			ErrorHandler.ThrowOnFailure(uiShell.FindToolWindow((uint)__VSCREATETOOLWIN.CTW_fForceCreate, ref propertiesWindowGuid,
				out IVsWindowFrame propertiesWindowFrame));
			if (propertiesWindowFrame != null)
				propertiesWindowFrame.Show();
		}
	}
}
