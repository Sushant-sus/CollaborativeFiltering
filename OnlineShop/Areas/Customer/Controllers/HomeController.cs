using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Models;
using OnlineShop.Services;
using OnlineShop.Utility;
using X.PagedList;
using System.Globalization;
using CsvHelper.Configuration;

namespace OnlineShop.Controllers
{
    [Area("Customer")]

    public class HomeController : Controller
    {
        private ApplicationDbContext _db;
        private readonly IDbConnection _configuration;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IRecommendationService _recommendationService;
        public static string dataLocation = "./Data";

        public HomeController(ApplicationDbContext db, IDbConnection configuration, UserManager<IdentityUser> userManager, IRecommendationService recommendationService)
        {
            _db = db;
            _configuration = configuration;
            _userManager = userManager;
            _recommendationService = recommendationService;
        }

     
        public async Task<IActionResult> Index(int? page, [FromQuery] int ProductTypeId = 0, [FromQuery] string Query = "")
        {  
            if (Query == null)
            {
                Query = "";
            }
            ViewBag.producttypeId = ProductTypeId;
            var product = await _configuration.QueryAsync<Products>("select * from products where (ProductTypeId=@producttypeId or @producttypeId=0) and (Name like '%'+@query+ '%') and IsActive=1 and IsDeleted=0", new { producttypeId = ProductTypeId, query = Query });

            ViewBag.producttypes = await _configuration.GetListAsync<ProductTypes>();
            ViewData["TagId"] = await _configuration.GetListAsync<SpecialTag>();
            var product1 = product.ToPagedList(page ?? 1, 10);
            var data = _db.Products.Include(c => c.ProductTypes).Include(c => c.SpecialTag).ToList().ToPagedList(page ?? 1, 9);

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var recommendingProduct11 = await _configuration.QueryAsync<Products>("select Rating,UserId,ProductId from ratingdataset");

                // Write the products to a CSV file
                var csvDirectory = "Data/";
                var csvPath = csvDirectory + "ProductRatings.csv";
                if (!System.IO.Directory.Exists(csvDirectory))
                {
                    System.IO.Directory.CreateDirectory(csvDirectory);
                }

                using (var writer = new System.IO.StreamWriter(csvPath))
                using (var csv = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<RatingMap>();
                    csv.WriteRecords(recommendingProduct11);
                }


                var productmapping = await _configuration.QueryAsync<Products>("select * from Products");

                // Write the products to a CSV file 
                var csvPathforproduct = csvDirectory + "ProductDetails.csv";
                if (!System.IO.Directory.Exists(csvDirectory))
                {
                    System.IO.Directory.CreateDirectory(csvDirectory);
                }

                using (var writer = new System.IO.StreamWriter(csvPathforproduct))
                using (var csv = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Context.RegisterClassMap<ProductsMap>();
                    csv.WriteRecords(productmapping);
                }

                var recommendationdata = _recommendationService.GetProductRecommendations(user.Id);

                //                    var recommendationdata = await _configuration.QueryAsync<Products>(@" SELECT top 10 p.Id,
                //       MIN(sl.addedon) AS 'AddedOn',
                //       MIN(p.Name) AS 'Name',
                //       MIN(p.Price) AS 'Price',
                //       MIN(p.Image) AS 'Image',
                //       MIN(p.ProductColor) AS 'ProductColor',
                //       MIN(p.ProductTypeId) AS 'ProductTypeId',
                //       MIN(p.SpecialTagId) AS 'SpecialTagId',
                //       MIN(p.Description) AS 'Description',
                //       MIN(p.SecondaryImage1) AS 'SecondaryImage1',
                //       MIN(p.SecondaryImage2) AS 'SecondaryImage2',
                //       MIN(p.SecondaryImage3) AS 'SecondaryImage3'
                //FROM searchlog sl
                //INNER JOIN Products p on sl.ProductTypeId=p.ProductTypeId
                //LEFT JOIN OrderDetails od ON p.Id = od.PorductId and od.UserId=@userid
                //left join Review r on r.ProductId=p.Id
                //WHERE sl.UserId = @userid
                //  AND od.PorductId IS NULL and p.IsActive=1
                //GROUP BY p.Id
                //ORDER BY (case when min(r.Rating)=null then min(sl.AddedOn) else min(r.Rating) end),min(p.Price) DESC
                //", new { userid = user.Id });
                ViewBag.recommendationdata = recommendationdata;

            }
            //else
            //{
            //    var recommendationdata = await _configuration.QueryAsync<Products>(@" select distinct top 10 p.*,r.Rating from orders o
            //    inner join OrderDetails od on o.Id=od.OrderId
            //    inner join Products p on od.PorductId=p.Id and p.IsActive=1
            //    inner join ratingdataset r on o.Id=r.SalesId 
            //    order by r.Rating desc");
            //    ViewBag.recommendationdata = recommendationdata;
            //}

            ViewBag.userdata = user;

            ViewBag.query = Query;


            return View(product1);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //GET product detail acation method

        public async Task<IActionResult> Detail(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var product = _db.Products.Include(c => c.ProductTypes).FirstOrDefault(c => c.Id == id);
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {

                var searchlog = new SearchLog();
                searchlog.AddedOn = DateTime.Now;
                searchlog.ProductId = product.Id;
                searchlog.UserId = user.Id;
                searchlog.Keyword = product.Name;
                searchlog.ProductTypeId = product.ProductTypeId;
                await _configuration.InsertAsync<SearchLog>(searchlog);
            }
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        //POST product detail acation method
        [HttpPost]
        [ActionName("Detail")]
        public ActionResult ProductDetail(int? id)
        {
            List<Products> products = new List<Products>();
            if (id == null)
            {
                return NotFound();
            }

            var product = _db.Products.Include(c => c.ProductTypes).FirstOrDefault(c => c.Id == id);
            product.Price = (product.Price - (product.Price * 20) / 100);
            if (product == null)
            {
                return NotFound();
            }

            products = HttpContext.Session.Get<List<Products>>("products ");
            if (products == null)
            {
                products = new List<Products>();
            }
            products.Add(product);
            HttpContext.Session.Set("products", products);
            return RedirectToAction(nameof(Index));
        }
        public sealed class RatingMap : ClassMap<Products>
        {
            public RatingMap()
            {
                Map(p => p.Rating).Name("ratings");
                Map(p => p.UserId).Name("user");
                Map(p => p.ProductId).Name("productid");
            }
        }
        public sealed class ProductsMap : ClassMap<Products>
        {
            public ProductsMap()
            {
                Map(p => p.Id).Name("Id");
                Map(p => p.Name).Name("Name");
                Map(p => p.Price).Name("Price");
                Map(p => p.Image).Name("Image");
                Map(p => p.ProductColor).Name("ProductColor");
                Map(p => p.IsAvailable).Name("IsAvailable");
                Map(p => p.ProductTypeId).Name("ProductTypeId");
                Map(p => p.SpecialTagId).Name("SpecialTagId");
                Map(p => p.Description).Name("Description");
                Map(p => p.SecondaryImage1).Name("SecondaryImage1");
                Map(p => p.SecondaryImage2).Name("SecondaryImage2");
                Map(p => p.SecondaryImage3).Name("SecondaryImage3");
                Map(p => p.IsActive).Name("IsActive");
                Map(p => p.IsDeleted).Name("IsDeleted");
            }
        }

    }
}
