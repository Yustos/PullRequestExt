using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YL.PullRequestService.Dtos
{
	public enum ThreadStatus
	{
		Unknown,
		Active,
		Fixed,
		WontFix,
		Closed,
		ByDesign,
		Pending
	}
}
