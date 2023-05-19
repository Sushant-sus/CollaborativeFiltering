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
    public class SearchLog
    {

        public int Id { get; set; }
        public int ProductTypeId { get; set; }
        public string Keyword { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime AddedOn { get; set; }


    }
}
