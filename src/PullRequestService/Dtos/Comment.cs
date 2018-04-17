using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YL.PullRequestService.Dtos
{
	public class Comment
	{
		public short Id { get; }

		public string Content { get; }

		public string Author { get; }

		internal Comment(short id, string content, string author)
		{
			Id = id;
			Content = content;
			Author = author;
		}
	}
}
