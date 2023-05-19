using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineShop.Models
{
    public class OrderDetailsVM:Order
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        [NotMapped]
        public bool DeliverNotDeliver { get; set; }
        [NotMapped]
        public DateTime DeliveredDate { get; set; }

    }
}
