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
    public class ReviewController : Controller
    {
        private ApplicationDbContext _db;
        private readonly IDbConnection _configuration;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewController(ApplicationDbContext db, IDbConnection configuration, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _configuration = configuration;
            _userManager = userManager;
        }

        [Route("review/new/{id}/{orderid}")]
        public async Task<IActionResult> Create(int id,int orderid)
        {
            ViewBag.productid = id;
            ViewBag.orderid = orderid;

            //ViewData["productTypeId"] = await _configuration.QueryAsync<ProductTypes>("select * from ProductTypes where IsActive=1 and IsDeleted=0");/*new SelectList(_db.ProductTypes.ToList(), "Id", "ProductType");*/
            //ViewData["TagId"] = await _configuration.QueryAsync<SpecialTag>("select * from SpecialTags where IsActive=1 and IsDeleted=0");
            return View();
        }

        [HttpPost]
        [Route("review/new")]
        public async Task<IActionResult> Create(Review review)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                review.AddedOn=DateTime.Now; 
                review.UpdatedOn=DateTime.Now; 
                review.DeletedOn=DateTime.Now; 
                review.IsActive = true;
                review.UserId = user.Id;
                await _configuration.InsertAsync<Review>(review);
                //_notificationService.Notify("Success", "Review submitted successfully!", NotificationType.Success);
                return Redirect("/");
            }
            return Redirect("/");
        }
    }
}
