using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using YL.PullRequestService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace YL.PullRequestService
{
	public class PullRequestsService
	{
		private readonly Uri _tfsCollectionUri;
		private readonly VssConnection _connection;
		private readonly GitHttpClient _gitClient;

		public PullRequestsService(Uri tfsCollectionUri)
		{
			_tfsCollectionUri = tfsCollectionUri;
			_connection = new VssConnection(tfsCollectionUri, new VssCredentials());
			_gitClient = _connection.GetClient<GitHttpClient>();
		}

		public async Task<PullRequest[]> GetBranchPullRequests(string projectName, string repositoryName)
		{
			var searchCriteria = new GitPullRequestSearchCriteria
			{
				Status = PullRequestStatus.Active
			};
			var pullRequests = await _gitClient.GetPullRequestsAsync(projectName, repositoryName, searchCriteria);
			return pullRequests
				.Select(pr => new PullRequest(pr.PullRequestId, pr.Title, pr.CreatedBy.DisplayName, pr.CreationDate, pr.Description, pr.Url, pr.SourceRefName, pr.TargetRefName))
				.ToArray();
		}

		public async Task<PullRequestThread[]> GetPullRequestThreads(string projectName, string repositoryName, int pullRequestId, bool onlyActive)
		{
			var threads = await _gitClient.GetThreadsAsync(projectName, repositoryName, pullRequestId);
			return threads
				.Where(t => !onlyActive || t.Status == CommentThreadStatus.Active)
				.Select(t => new
				{
					Thread = t,
					UserComments = t.Comments.Where(c => c.CommentType == CommentType.Text).ToArray()
				})
				.Where(t => t.UserComments.Any())
				.Select(t =>
					new PullRequestThread(t.Thread.Id,
						pullRequestId,
						MapThreadStatus(t.Thread.Status),
						t.Thread.IsDeleted,
						t.UserComments.First().Content,
						t.UserComments.First().Author.DisplayName,
						t.Thread.ThreadContext.FilePath,
						t.UserComments.Length,
						MapCommentPosition(t.Thread.ThreadContext.RightFileStart),
						MapCommentPosition(t.Thread.ThreadContext.RightFileEnd))
				)
				.ToArray();
		}

		public async Task<Change[]> GetPullRequestChanges(string projectName, string repositoryName, int pullRequestId)
		{
			var iterations = await _gitClient.GetPullRequestIterationsAsync(projectName, repositoryName, pullRequestId);
			var iterationId = iterations.Max(i => i.Id.GetValueOrDefault(0));
			var changes = await FetchIterationChanges(projectName, repositoryName, pullRequestId, iterationId);
			return changes
				.Select(ce =>
					new Change(ce.Item.Path, MapChangeType(ce.ChangeType), ce.Item.ObjectId, ce.Item.OriginalObjectId))
				.ToArray();
		}

		public async Task<Stream> GetFileContent(string projectName, string repositoryName, int pullRequestId, string sha1)
		{
			return await _gitClient.GetBlobContentAsync(projectName, repositoryName, sha1);
		}

		public async Task SetPullRequestThreadStatus(string projectName, string repositoryName, int pullRequestId, int threadId, ThreadStatus threadStatus)
		{
			var threadUpdate = new GitPullRequestCommentThread
			{
				Status = MapThreadStatus(threadStatus)
			};
			await _gitClient.UpdateThreadAsync(threadUpdate, projectName, repositoryName, pullRequestId, threadId);
		}

		public async Task CreatePullRequestThread(string projectName, string repositoryName, int pullRequestId, string filePath, string text,
			Position start, Position end)
		{
			var comment = new Microsoft.TeamFoundation.SourceControl.WebApi.Comment
			{
				ParentCommentId = 0,
				Content = text,
				CommentType = CommentType.Text
			};

			var threadCreate = new GitPullRequestCommentThread
			{
				Comments = new List<Microsoft.TeamFoundation.SourceControl.WebApi.Comment> { comment },
				Status = CommentThreadStatus.Active,
				ThreadContext = new CommentThreadContext
				{
					FilePath = filePath,
					RightFileStart = new CommentPosition
					{
						Line = start.Line,
						Offset = start.Offset
					},
					RightFileEnd = new CommentPosition
					{
						Line = end.Line,
						Offset = end.Offset
					}
				}
			};
			await _gitClient.CreateThreadAsync(threadCreate, projectName, repositoryName, pullRequestId);
		}

		public Dtos.Comment[] GetComments(string projectName, string repositoryName, int pullRequestId, int threadId)
		{
			var searchCriteria = new GitPullRequestSearchCriteria
			{
				Status = PullRequestStatus.Active
			};
			var comments = _gitClient.GetCommentsAsync(projectName, repositoryName, pullRequestId, threadId).Result;
			return comments
				.Where(c => c.CommentType == CommentType.Text)
				.Select(c => new Dtos.Comment(c.Id, c.Content, c.Author.DisplayName))
				.ToArray();
		}

		public async Task ReplyToPullRequestThread(string projectName, string repositoryName, int pullRequestId, int threadId, string comment)
		{
			var threadComment = new Microsoft.TeamFoundation.SourceControl.WebApi.Comment
			{
				ParentCommentId = 1,
				Content = comment,
				CommentType = CommentType.Text
			};
			await _gitClient.CreateCommentAsync(threadComment, projectName, repositoryName, pullRequestId, threadId);
		}

		private async Task<IEnumerable<GitPullRequestChange>> FetchIterationChanges(string projectName, string repositoryName,
			int pullRequestId, int iterationId)
		{
			int? nextTop = null;
			int? nextSkip = null;

			var result = new List<GitPullRequestChange>();
			do
			{
				var changes = await _gitClient.GetPullRequestIterationChangesAsync(projectName, repositoryName, pullRequestId, iterationId,
					top: nextTop, skip: nextSkip);
				nextTop = changes.NextTop;
				nextSkip = changes.NextSkip;
				result.AddRange(changes.ChangeEntries);
			}
			while (nextTop.GetValueOrDefault(0) != 0);
			return result;
		}

		private ThreadStatus MapThreadStatus(CommentThreadStatus threadStatus)
		{
			switch (threadStatus)
			{
				case CommentThreadStatus.Active:
					return ThreadStatus.Active;
				case CommentThreadStatus.Fixed:
					return ThreadStatus.Fixed;
				case CommentThreadStatus.WontFix:
					return ThreadStatus.WontFix;
				case CommentThreadStatus.Closed:
					return ThreadStatus.Closed;
				case CommentThreadStatus.ByDesign:
					return ThreadStatus.ByDesign;
				case CommentThreadStatus.Pending:
					return ThreadStatus.Pending;
			}
			return ThreadStatus.Unknown;
		}

		private CommentThreadStatus MapThreadStatus(ThreadStatus threadStatus)
		{
			switch (threadStatus)
			{
				case ThreadStatus.Active:
					return CommentThreadStatus.Active;
				case ThreadStatus.Fixed:
					return CommentThreadStatus.Fixed;
				case ThreadStatus.WontFix:
					return CommentThreadStatus.WontFix;
				case ThreadStatus.Closed:
					return CommentThreadStatus.Closed;
				case ThreadStatus.ByDesign:
					return CommentThreadStatus.ByDesign;
				case ThreadStatus.Pending:
					return CommentThreadStatus.Pending;
			}
			throw new ApplicationException($"Unknown thread status: {threadStatus}");
		}

		private ChangeType MapChangeType(VersionControlChangeType changeType)
		{
			switch (changeType)
			{
				case VersionControlChangeType.Add:
					return ChangeType.Add;
				case VersionControlChangeType.Edit:
					return ChangeType.Edit;
				case VersionControlChangeType.Delete:
					return ChangeType.Delete;
			}
			return ChangeType.Unknown;
		}

		private Position MapCommentPosition(CommentPosition position)
		{
			if (position == null)
			{
				return null;
			}

			return new Position(position.Line, position.Offset);
		}
	}
}
