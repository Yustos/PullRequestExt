using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YL.PullRequestService.Dtos
{
	public class Change
	{
		public string Path { get; }

		public ChangeType ChangeType { get; }

		public string ObjectId { get; }

		public string OriginalObjectId { get; }

		internal Change(string path, ChangeType changeType, string objectId, string originalObjectId)
		{
			Path = path;
			ChangeType = changeType;
			ObjectId = objectId;
			OriginalObjectId = originalObjectId;
		}
	}
}
