using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YL.PullRequestService.Dtos
{
	public class PullRequest
	{
		public int Id { get; }

		public string Title { get; }

		public string SourceRefName { get; }

		public string TargetRefName { get; }

		public string DisplayName { get; }

		public DateTime CreationDate { get; }

		public string Description { get; }

		public string Url { get; }

		internal PullRequest(int id, string title, string displayName, DateTime creationDate, string description, string url, string sourceRefName, string targetRefName)
		{
			Id = id;
			Title = title;
			DisplayName = displayName;
			CreationDate = creationDate;
			Description = description;
			Url = url;
			SourceRefName = sourceRefName;
			TargetRefName = targetRefName;
		}
	}
}
