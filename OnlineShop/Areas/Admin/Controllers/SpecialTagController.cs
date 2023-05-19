using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.Data;
using OnlineShop.Models;

namespace OnlineShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Super User")]
    public class SpecialTagController : Controller
    {
        
        private ApplicationDbContext _db;
        private readonly IDbConnection _dapper;

        public SpecialTagController(ApplicationDbContext db, IDbConnection dapper)
        {
            _db = db;
            _dapper = dapper;
        }

        //GET Index Action Method
        public IActionResult Index()
        {
            return View(_db.SpecialTags.ToList().Where(x=>x.IsActive==true && x.IsDeleted==false));
        }

        //GET Create Action Method

        public ActionResult Create()
        {
            return View();
        }

        //POST Create Action Method

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SpecialTag specialTag)
        {
            if (ModelState.IsValid)
            {
                specialTag.IsActive = true;
                specialTag.IsDeleted = false;
                _db.SpecialTags.Add(specialTag);
                await _db.SaveChangesAsync();
                TempData["save"] = "Brand has been saved";
                return RedirectToAction(nameof(Index));
            }

            return View(specialTag);
        }

        //GET Edit Action Method

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialTag = _db.SpecialTags.Find(id);
            if (specialTag == null)
            {
                return NotFound();
            }
            return View(specialTag);
        }

        //POST Edit Action Method

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SpecialTag specialTag)
        {
            if (ModelState.IsValid)
            {
                specialTag.IsActive = true;
                specialTag.IsDeleted = false;
                _db.Update(specialTag);
                await _db.SaveChangesAsync();
                TempData["edit"] = "Brand has been Updated";
                return RedirectToAction(nameof(Index));
            }

            return View(specialTag);
        }

        //GET Details Action Method

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialTag = _db.SpecialTags.Find(id);
            if (specialTag == null)
            {
                return NotFound();
            }
            return View(specialTag);
        }

        //POST Edit Action Method

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Details(SpecialTag specialTag)
        {
            return RedirectToAction(nameof(Index));

        }

        //GET Delete Action Method

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var specialTag = _db.SpecialTags.Find(id);
            if (specialTag == null)
            {
                return NotFound();
            }
            return View(specialTag);
        }

        //POST Delete Action Method

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id, SpecialTag specialTag)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (id != specialTag.Id)
            {
                return NotFound();
            }

            var specialTags = _db.SpecialTags.Find(id);
            if (specialTags == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {

                specialTag.IsActive = false;
                specialTag.IsDeleted = true;
                await _dapper.UpdateAsync<SpecialTag>(specialTag);
                TempData["delete"] = "Brand has been deleted";
                return RedirectToAction(nameof(Index));
            }

            return View(specialTag);
        }
    }
}