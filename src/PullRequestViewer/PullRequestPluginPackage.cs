using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;
using YL.PullRequestViewer.Controls;

namespace YL.PullRequestViewer
{
	[ProvideToolWindow(typeof(PullRequestsPane), Style = VsDockStyle.Tabbed, Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")]

	// TFS
#warning Not working.
	[ProvideAutoLoad("{e13eedef-b531-4afe-9725-28a69fa4f896}")]

	[ProvideMenuResource(1000, 1)]
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[Guid("4E68D689-2127-473C-8AE9-F615BD0B4432")]
	public class PullRequestPluginPackage : Package
	{
		private OleMenuCommandService menuService;

		protected override void Initialize()
		{
			base.Initialize();
			var id = new CommandID(GuidsList.guidClientCmdSet, PkgCmdId.cmdidShowPullRequestsWindow);
			DefineCommandHandler(new EventHandler(ShowPullRequestWindow), id);
		}

		/// <summary>
		/// Define a command handler.
		/// When the user press the button corresponding to the CommandID
		/// the EventHandler will be called.
		/// </summary>
		/// <param name="id">The CommandID (Guid/ID pair) as defined in the .vsct file</param>
		/// <param name="handler">Method that should be called to implement the command</param>
		/// <returns>The menu command. This can be used to set parameter such as the default visibility once the package is loaded</returns>
		internal OleMenuCommand DefineCommandHandler(EventHandler handler, CommandID id)
		{
			// if the package is zombied, we don't want to add commands
			if (Zombied)
				return null;

			// Make sure we have the service
			if (menuService == null)
			{
				// Get the OleCommandService object provided by the MPF; this object is the one
				// responsible for handling the collection of commands implemented by the package.
				menuService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			}
			OleMenuCommand command = null;
			if (null != menuService)
			{
				// Add the command handler
				command = new OleMenuCommand(handler, id);
				menuService.AddCommand(command);
			}
			return command;
		}

		/// <summary>
		/// This method loads a localized string based on the specified resource.
		/// </summary>
		/// <param name="resourceName">Resource to load</param>
		/// <returns>String loaded for the specified resource</returns>
		internal string GetResourceString(string resourceName)
		{
			string resourceValue;
			IVsResourceManager resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
			if (resourceManager == null)
			{
				throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
			}
			Guid packageGuid = GetType().GUID;
			int hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
			ErrorHandler.ThrowOnFailure(hr);
			return resourceValue;
		}

		private void ShowPullRequestWindow(object sender, EventArgs arguments)
		{
			var pane = FindToolWindow(typeof(PullRequestsPane), 0, true);
			if (pane == null)
			{
				throw new COMException(GetResourceString("@101"));
			}
			var frame = pane.Frame as IVsWindowFrame;
			if (frame == null)
			{
				throw new COMException(GetResourceString("@102"));
			}
			ErrorHandler.ThrowOnFailure(frame.Show());
		}
	}
}
