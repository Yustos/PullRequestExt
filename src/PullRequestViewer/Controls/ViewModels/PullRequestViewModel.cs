using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YL.PullRequestService.Dtos;

namespace YL.PullRequestViewer.Controls.ViewModels
{
	public class PullRequestViewModel
	{
		public PullRequest PullRequest { get; }

		public PullRequestType PullRequestType { get; }

		public PullRequestViewModel(PullRequest pullRequest, PullRequestType pullRequestType)
		{
			PullRequest = pullRequest;
			PullRequestType = pullRequestType;
		}
	}
}
