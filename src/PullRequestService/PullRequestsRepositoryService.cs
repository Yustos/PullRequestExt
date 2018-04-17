using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YL.PullRequestService.Dtos;

namespace YL.PullRequestService
{
	public class PullRequestsRepositoryService : PullRequestsService
	{
		private readonly string _projectName;
		private readonly string _repositoryName;

		public PullRequestsRepositoryService(Uri tfsCollectionUri, string projectName, string repositoryName)
			: base(tfsCollectionUri)
		{
			_projectName = projectName;
			_repositoryName = repositoryName;
		}

		public async Task<PullRequest[]> GetBranchPullRequests()
		{
			return await GetBranchPullRequests(_projectName, _repositoryName);
		}

		public async Task<Change[]> GetPullRequestChanges(int pullRequestId)
		{
			return await GetPullRequestChanges(_projectName, _repositoryName, pullRequestId);
		}

		public async Task ReplyToPullRequestThread(int pullRequestId, int threadId, string comment)
		{
			await ReplyToPullRequestThread(_projectName, _repositoryName, pullRequestId, threadId, comment);
		}

		public async Task<PullRequestThread[]> GetPullRequestThreads(int pullRequestId, bool onlyActive)
		{
			return await GetPullRequestThreads(_projectName, _repositoryName, pullRequestId, onlyActive);
		}

		public async Task SetPullRequestThreadStatus(int pullRequestId, int threadId, ThreadStatus threadStatus)
		{
			await SetPullRequestThreadStatus(_projectName, _repositoryName, pullRequestId, threadId, threadStatus);
		}

		public async Task CreatePullRequestThread(int pullRequestId, string filePath, string text,
			Position start, Position end)
		{
			await CreatePullRequestThread(_projectName, _repositoryName, pullRequestId, filePath, text, start, end);
		}
	}
}
