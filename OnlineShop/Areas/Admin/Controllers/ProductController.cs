using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OnlineShop.Data;
using OnlineShop.Models;
using Dapper;
using System.Data;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Super User")]
    public class ProductController : Controller
    {
        private ApplicationDbContext _db;
        private IHostingEnvironment _he;

        private readonly IDbConnection _configuration;

        public ProductController(ApplicationDbContext db,IHostingEnvironment he,IDbConnection configuration)
        {
            _db = db;
            _he = he;
              _configuration = configuration;
        }


        public string CheckOrCreateDirectory(string path)
        {
            string rootpath = _he.WebRootPath;
            string folderPath = Path.Combine(rootpath, path);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return folderPath;
        }
        public void CopyStream(Stream stream, string destPath)
        {
            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }
        public string Save(string dirPath, IFormFile file)
        {
            string physicallPath = CheckOrCreateDirectory(dirPath);
            IFormFile myFile = file;

            string physicalfilepath = physicallPath + "\\" + myFile.FileName;
            var relativePath = Path.Combine(dirPath, myFile.FileName);
            relativePath = relativePath.Replace("\\", "/").Replace(@"\", "/");
            if (!relativePath.StartsWith("/"))
            {
                relativePath = "/" + relativePath;
            }
            if (myFile != null && myFile.Length != 0)
            {

                try
                {
                    CopyStream(myFile.OpenReadStream(), physicalfilepath);
                    return relativePath;


                }
                catch (Exception ex)
                {
                    throw ex;
                }
                //}
            }
            return "";
        }




        public async Task<IActionResult> Index()
        {
            var data = await _configuration.QueryAsync<Products>(@"select p.*,pt.ProductType as 'ProductTypeName',sta.Name as 'SpecialTagName' from Products p
                                                                left join ProductTypes pt on p.ProductTypeId = pt.Id
                                                                left join SpecialTags sta on p.SpecialTagId = sta.Id where p.IsActive=1 and p.IsDeleted=0
                                                                order by p.Id desc");
 
            return View(data); 
        }

        //POST Index action method
       [HttpPost]
        public async Task<IActionResult> Index( decimal? lowAmount, decimal? largeAmount)
        {
            var products = await _configuration.QueryAsync<Products>(@"select p.*,pt.ProductType as 'ProductTypeName',sta.Name as 'SpecialTagName' from Products p
                                                                        left join ProductTypes pt on p.ProductTypeId=pt.Id
                                                                        left join SpecialTags sta on p.SpecialTagId=sta.Id
                                                                       where p.IsActive=1 and p.IsDeleted=0 and (p.Price>=@lowAmount or @lowAmount is null) and (p.Price<=@largeAmount or @largeAmount is null)
                                                                        order by p.Id desc ", new {lowAmount, largeAmount});
            ViewBag.lowAmount=lowAmount;
            ViewBag.largeAmount = largeAmount;

            return View(products);
        }

        //Get Create method
        public async Task<IActionResult> Create()
        {
            ViewData["productTypeId"] = await _configuration.QueryAsync<ProductTypes>("select * from ProductTypes where IsActive=1 and IsDeleted=0");/*new SelectList(_db.ProductTypes.ToList(), "Id", "ProductType");*/
            ViewData["TagId"] = await _configuration.QueryAsync<SpecialTag>("select * from SpecialTags where IsActive=1 and IsDeleted=0");
            return View();
        }
        

        //Post Create method
        [HttpPost]
        public async Task<IActionResult> Create(Products product)
        {
            if(ModelState.IsValid)
            {
                var searchProduct =await  _configuration.QueryAsync<Products>("select 1 from Products where Name=@Name", new {Name=product.Name});
                //var searchProduct = _db.Products.FirstOrDefault(c => c.Name == product.Name);
                if(searchProduct.Any())
                {
                    ViewBag.message = "This product is already exist";

                    ViewData["productTypeId"] = await _configuration.GetListAsync<ProductTypes>(); 
                    ViewData["TagId"] = await _configuration.GetListAsync<SpecialTag>();
                    return View(product);
                }

                if (product.primaryimagefile != null)
                {
                    product.Image = Save("Primary/Image1", product.primaryimagefile);
                }
                if (product.secondaryimage1file != null)
                {
                    product.SecondaryImage1 = Save("Secondary/Image1", product.secondaryimage1file);
                }
                if (product.secondaryimage2file != null)
                {
                    product.SecondaryImage2 = Save("Secondary/Image2", product.secondaryimage2file);
                }
                if (product.secondaryimage3file != null)
                {
                    product.SecondaryImage3 = Save("Secondary/Image3", product.secondaryimage3file);
                }
                product.IsActive = true;
                product.IsDeleted = false;
                //if (image != null)
                //{
                //    var name = Path.Combine(_he.WebRootPath + "/Images", Path.GetFileName( image.FileName));
                //    await  image.CopyToAsync(new FileStream(name, FileMode.Create));
                //    product.Image = "Images/" +  image.FileName;
                //}

                //if( image == null)
                //{
                //    product.Image = "Images/noimage.PNG";
                //}
                await _configuration.InsertAsync<Products>(product); 

                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        //GET Edit Action Method

        public async Task<IActionResult> Edit(int? id)
        {
            ViewData["productTypeId"] = await _configuration.GetListAsync<ProductTypes>(); 
            ViewData["TagId"] = await _configuration.GetListAsync<SpecialTag>();
            if (id==null)
            {
                return NotFound();
            }
            var product = _configuration.Get<Products>(id);

            if (product==null)
            {
                return NotFound();
            }
            return View(product);
        }

        //POST Edit Action Method
        [HttpPost]
        public async Task<IActionResult> Edit(Products products,IFormFile image)
        {
            if (ModelState.IsValid)
             {
                products.IsActive = true;
                products.IsDeleted = false;
                if ( image != null)
                {
                    var name = Path.Combine(_he.WebRootPath + "/Images", Path.GetFileName( image.FileName));
                    await  image.CopyToAsync(new FileStream(name, FileMode.Create));
                    products.Image = "Images/" + image.FileName;
                }

                if ( image == null)
                {
                    products.Image = "Images/noimage.PNG";
                }
                await _configuration.UpdateAsync<Products>(products);
                return RedirectToAction(nameof(Index));
            }

            return View(products);
        }

        //GET Details Action Method
        public async Task<IActionResult> Details(int? id)
        {

            if(id==null)
            {
                return NotFound();
            }
            var product = await _configuration.QueryFirstOrDefaultAsync<Products>(@"select p.*,pt.ProductType as 'ProductTypeName',sta.Name as 'SpecialTagName' from Products p
                                                                left join ProductTypes pt on p.ProductTypeId = pt.Id
                                                                left join SpecialTags sta on p.SpecialTagId = sta.Id
                                                                    where p.Id=@id                                                                
                                                                    order by p.Id desc", new { id = id });
            //var product = _configuration.Get<Products>(id);
           
            if(product==null)
            {
                return NotFound();
            }
            return View(product);
        }

        //GET Delete Action Method

        public async Task<ActionResult> Delete(int? id)
        {
            if(id==null)
            {
                return NotFound();
            }
            var data = _configuration.Get<Products>(id);
            data.IsActive = false;
            data.IsDeleted = true;
            await _configuration.UpdateAsync<Products>(data);
            //var product =await _configuration.DeleteAsync<Products>(id);
            TempData["delete"] = "Product has been Deleted";
            return RedirectToAction(nameof(Index));
        }    
     
    }
}