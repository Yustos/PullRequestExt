using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YL.GitRepository;
using YL.PullRequestService;

namespace YL.PullRequestViewer.Controls
{
	internal static class PullRequestConfigFactory
	{
		internal static string GetGitFolder()
		{
			var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
			var gitFolder = ConfigReader.GetGitFolder(new FileInfo(dte.Solution.FileName).DirectoryName);
			return gitFolder;
		}

		internal static GitConfig GetGitConfig()
		{
			var dte = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
			var gitConfig = ConfigReader.ReadConfig(new FileInfo(dte.Solution.FileName).DirectoryName);
			return gitConfig;
		}
	}
}
