using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YL.GitRepository
{
	public static class ConfigReader
	{
		private static readonly Regex TfsUrlRegex = new Regex(@"(?<Protocol>.*):\/\/(?<Collection>.*)\/(?<Project>.*)\/_git\/(?<Repository>.*)", RegexOptions.Compiled);

		public static GitConfig ReadConfig(string path)
		{
			var gitFolder = GetGitFolder(path);
			if (string.IsNullOrWhiteSpace(gitFolder))
			{
				return null;
			}

			var branchName = GetCurrentBranchName(gitFolder);

			var parser = new Parser(Path.Combine(gitFolder, "config"));
			var remoteUrl = parser.GetString("remote \"origin\"", "url");

			var parsedUrl = TfsUrlRegex.Match(remoteUrl);
			return new GitConfig(
				new Uri($"{parsedUrl.Groups["Protocol"]}://{parsedUrl.Groups["Collection"].Value}"),
				parsedUrl.Groups["Project"].Value,
				parsedUrl.Groups["Repository"].Value,
				branchName);
		}

		public static string GetGitFolder(string path)
		{
			if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) return string.Empty;

			var currentPath = new DirectoryInfo(path);
			bool isAGitRepo;
			string gitFolder;
			do
			{
				gitFolder = Path.Combine(currentPath.FullName, ".git");
				isAGitRepo = Directory.Exists(gitFolder);
				if (isAGitRepo) break;

				currentPath = currentPath.Parent;
			} while (currentPath != null && currentPath.Exists);

			return isAGitRepo ? gitFolder : string.Empty;
		}

		internal static string GetCurrentBranchName(string gitFolder)
		{
			var headFile = GetHeadFile(gitFolder);
			if (string.IsNullOrWhiteSpace(headFile)) return string.Empty;

			var headFileContent = File.ReadAllText(headFile);
			return ExtractBranchNameFromHeadFile(headFileContent);
		}

		private static string GetHeadFile(string gitFolder)
		{
			var headFile = Path.Combine(gitFolder, "HEAD");
			var headFileExists = File.Exists(headFile);

			return headFileExists ? headFile : string.Empty;
		}

		private static string ExtractBranchNameFromHeadFile(string headFileContent)
		{
			return headFileContent.Replace(@"ref: ", string.Empty)
				.Replace(Environment.NewLine, string.Empty)
				.Trim();
		}
	}
}
