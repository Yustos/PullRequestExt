using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YL.PullRequestService.Dtos
{
	public class Position
	{
		public int Line { get; }

		public int Offset { get; }

		public Position(int line, int offset)
		{
			Line = line;
			Offset = offset;
		}
	}
}
