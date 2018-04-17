using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YL.PullRequestService.Dtos
{
	public class PullRequestThread
	{
		public int Id { get; }

		public int PullRequestId { get; }

		public ThreadStatus ThreadStatus { get; }

		public bool IsDeleted { get; }

		public string Title { get; }

		public string FilePath { get; }

		public string Author { get; }

		public int CommentsCount { get; }

		public Position Start { get; }

		public Position End { get; }

		internal PullRequestThread(int id, int pullRequestId, ThreadStatus threadStatus, bool isDeleted, string title, string author, string filePath, int commentsCount, Position start, Position end)
		{
			Id = id;
			PullRequestId = pullRequestId;
			ThreadStatus = threadStatus;
			IsDeleted = isDeleted;
			Title = title;
			Author = author;
			FilePath = filePath;
			CommentsCount = commentsCount;
			Start = start;
			End = end;
		}
	}
}
