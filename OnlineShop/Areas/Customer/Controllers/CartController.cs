using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Helpers;
using OnlineShop.Models;
using System.Collections.Generic;
using System.Linq;

 

namespace OnlineShop.Areas.Customer.Controllers
{
    [Route("cart")]
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }
        [Route("Index")]
        public IActionResult Index()
        
        {
            var cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            ViewBag.cart = cart;
            ViewBag.ReturnUrl = "/checkout";
            if (cart == null)
            {
                ViewBag.total = 0;
            }
            else
            {
                ViewBag.total = cart.Sum(item => item.Products.Price * item.Quantity);

            }
            return View(cart);
        }

        [Route("buy/{id}")]
        public IActionResult Buy(int id, int Quantity)
        {
            if (Quantity == 0)
            {
                Quantity = 1;
                //return RedirectToAction("Index", "Home", new { area = "Customer" });

            }
            
            var productModel = _db.Products.Include(c => c.ProductTypes).FirstOrDefault(c => c.Id == id);
            productModel.Price = (productModel.Price - (productModel.Price * 20) / 100);
            if (SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart") == null)
            {
                List<Item> cart = new List<Item>();

                cart.Add(new Item { Products = productModel, Quantity = Quantity });
               //cart.Add(new Item { Products = productModel, Quantity = 1 });
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            }
            else
            {
                List<Item> cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
                var id1=id.ToString(); 
                int index = isExist(id);
                if (index != -1)
                {
                    cart[index].Quantity= cart[index].Quantity+ Quantity;
                }
                else
                {
                    cart.Add(new Item { Products = productModel, Quantity = Quantity });
                }
                SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            }
            TempData["Cart"] = "Product has been added to Cart";
            return RedirectToAction("Index", "Home", new { area = "Customer" });
            

        }

        [Route("remove/{id}")]
        public IActionResult Remove(int id)
        {
            List<Item> cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            int index = isExist(id);
            cart.RemoveAt(index);
            SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            return RedirectToAction("Index");
        }
        [Route("decrease/{id}")]
        public IActionResult Decrease(int id)
        {
            List<Item> cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            int index = isExist(id);
            if (cart[index].Quantity == 1)
            {
                cart.RemoveAt(index);

            }
            else
            {

                if (index != -1)
                {
                    cart[index].Quantity = cart[index].Quantity - 1;
                }
            }
            //cart.RemoveAt(index);
            SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            return RedirectToAction("Index", "Cart", new { area = "Customer" });
        }
        [Route("addquantity/{id}")]
        public IActionResult AddQuantity(int id)
        {
            List<Item> cart = SessionHelper.GetObjectFromJson<List<Item>>(HttpContext.Session, "cart");
            int index = isExist(id); 
                if (index != -1)
                {
                    cart[index].Quantity = cart[index].Quantity + 1;
                }
            SessionHelper.SetObjectAsJson(HttpContext.Session, "cart", cart);
            return RedirectToAction("Index", "Cart", new { area = "Customer" });
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
