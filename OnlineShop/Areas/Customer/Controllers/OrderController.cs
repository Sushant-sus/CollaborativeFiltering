using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using OnlineShop.Helpers;
using OnlineShop.Models;
using OnlineShop.Utility;

namespace OnlineShop.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private ApplicationDbContext _db;
        private readonly IDbConnection _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public OrderController(ApplicationDbContext db, IDbConnection configuration, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _configuration = configuration;
            _userManager = userManager;
        }

        //GET Checkout actioin method
        [Route("/order/summary")]
        [Authorize(Roles = "Admin,Super User,User")]
        public async Task<IActionResult> OrderSummary()
        {
            if(User.IsInRole("Super User") || User.IsInRole("Admin"))
            {
                var data = await _configuration.QueryAsync<OrderDetailsVM>(@"select o.*,p.Name as 'ProductName',ord.Quantity,(case when (getdate()-2)>OrderDate then 1 else 0  end) as 'DeliverNotDeliver' from Orders o
                                                                        left join OrderDetails ord on o.Id = ord.OrderId
                                                                        left join Products p on ord.PorductId = p.Id 
                                                                        order by CAST(o.OrderDate as date) desc");
                return View(data);
            }
            else
            {
                var user = await _userManager.GetUserAsync(User);
                var data = await _configuration.QueryAsync<OrderDetailsVM>(@"select o.*,p.Name as 'ProductName',ord.Quantity,(case when (getdate()-2)>OrderDate then 1 else 0  end) as 'DeliverNotDeliver' from Orders o
                                                                        left join OrderDetails ord on o.Id = ord.OrderId
                                                                        left join Products p on ord.PorductId = p.Id 
                                                                        where o.UserId=@userId
                                                                        order by CAST(o.OrderDate as date) desc", new { userId = user.Id });
                return View(data);

            }

        }
        [Authorize(Roles = "Admin,Super User,User")]
        public async Task<IActionResult> OrderSingleDetails(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }
            var data = await _configuration.QueryFirstOrDefaultAsync<OrderDetailsVM>(@"select o.*,p.Name as 'ProductName',ord.Quantity,
                                                                                    (case when (getdate()-2)>OrderDate then 1 else 0  end) as 'DeliverNotDeliver'
                                                                                    ,(DATEADD(DAY,2,cast(o.OrderDate as date))) as 'DeliveredDate'
                                                                                    from Orders o
                                                                                    left join OrderDetails ord on o.Id = ord.OrderId
                                                                                     left join Products p on ord.PorductId = p.Id
                                                                                     where o.Id=@orderId
                                                                                     order by CAST(o.OrderDate as date) desc", new { orderId = id });
            if (data == null)
            {
                return NotFound();
            }
            return View(data);
        }




        //GET Checkout actioin method
        [Route("checkout")]
        //[Authorize]
        public IActionResult Checkout(string Total)
        {
            if (!User.Identity.IsAuthenticated)
            {
                //return Redirect("/account/login?returnUrl=/booking/detail?schedule=" + schedule);
                return Redirect("/Account/Login?returnUrl=/checkout");
            }
            ////List<Item> products = HttpContextAccessor.HttpContext.Session.Get<List<Item>>("products");
            var cart =SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            ViewBag.Total = Total;
            ViewBag.cart = cart;
            return View();
            //return View();
        }
      
        //POST Checkout action method

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("checkout")]
        public async Task<IActionResult> Checkout(Order anOrder)
        {
            var user = await _userManager.GetUserAsync(User);
            //List<Products> products = HttpContext.Session.Get<List<Products>>("products");
            var cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            if (cart != null)
            {
                foreach (var items in cart)
                {
                    OrderDetails orderDetails=new OrderDetails();
                    orderDetails.PorductId = items.Products.Id;
                    orderDetails.TransactionDate = DateTime.Now;
                    orderDetails.Quantity = items.Quantity; 
                    orderDetails.UserId = user.Id; 
                    anOrder.OrderDetails.Add(orderDetails);
                    //int inde22x = isExist(items.Products.Id);
                    //cart.RemoveAt(inde22x);
                    //SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
                }
            }
            anOrder.UserId=user.Id;
            anOrder.OrderDate = DateTime.Now;
            anOrder.OrderNo = GetOrderNo();
            _db.Orders.Add(anOrder);
            await _db.SaveChangesAsync();
            HttpContext.Session.Remove("cart");
            var orderid = anOrder.Id;
            var productid = anOrder.OrderDetails.FirstOrDefault().PorductId;
            //SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            return Redirect("/review/new/"+productid+"/"+orderid+"");
        }


        public string GetOrderNo()
        {
            int rowCount = _db.Orders.ToList().Count()+1;
            return rowCount.ToString("000");
        }
       
        private int isExist(int id)
        {
            List<Item> cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            for (int i = 0; i < cart.Count; i++)
            {
                if (cart[i].Products.Id.Equals(id))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
