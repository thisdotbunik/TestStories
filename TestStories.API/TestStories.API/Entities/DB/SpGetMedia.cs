using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MillionStories.API.Entities.DB
{
	public class SpGetMedia
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Topic { get; set; }
		public string MediaStatus { get; set; }
		public string MediaType { get; set; }
		public string PublishedByUser { get; set; }
		public string UploadedByUser { get; set; }
		public DateTime? PublishDate { get; set; }
		public int TotalCount { get; set; }
	}
}
