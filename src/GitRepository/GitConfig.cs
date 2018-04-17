using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YL.GitRepository
{
	public class GitConfig
	{
		public Uri TfsCollectionUri { get; }

		public string ProjectName { get; }

		public string RepositoryName { get; }

		public string BranchName { get; }

		internal GitConfig(Uri tfsCollectionUri, string projectName, string repositoryName, string branchName)
		{
			TfsCollectionUri = tfsCollectionUri;
			ProjectName = projectName;
			RepositoryName = repositoryName;
			BranchName = branchName;
		}
	}
}
