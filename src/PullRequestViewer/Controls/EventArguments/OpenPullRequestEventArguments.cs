using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YL.PullRequestViewer.Controls.EventArguments
{
	public class OpenPullRequestEventArguments
	{
		public int PullRequestId { get; }

		public bool ShowInactive { get; }

		internal OpenPullRequestEventArguments(int pullRequestId, bool showInactive)
		{
			PullRequestId = pullRequestId;
			ShowInactive = showInactive;
		}
	}
}
