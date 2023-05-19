using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace OnlineShop.Models
{
	public class Review
	{
        [Key]
		public int ReviewId { get; set; }
		public decimal Rating { get; set; }
		public string Message { get; set; }
		public string UserId { get; set; }
		public int SalesId { get; set; }
		public int ProductId { get; set; }
		public bool IsApproved { get; set; }
		public bool IsActive { get; set; }
		public int AddedBy { get; set; }
		public DateTime AddedOn { get; set; }
		public int UpDatedBy { get; set; }
		public DateTime UpdatedOn { get; set; }
		public bool IsDeleted { get; set; }
		public DateTime DeletedOn { get; set; }
		public int DeletedBy { get; set; }
	}
}
