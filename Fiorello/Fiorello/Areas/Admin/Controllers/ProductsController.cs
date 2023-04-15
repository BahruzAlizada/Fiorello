﻿using Fiorello.DAL;
using Fiorello.Helper;
using Fiorello.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Fiorello.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductsController(AppDbContext db,IWebHostEnvironment env)
        {
            _db=db;
            _env=env;
        }

        public async Task<IActionResult> Index()
        {
            List<Product> products = await _db.Products.Include(x=>x.Category).ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _db.Categories.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(Product product,int categoryId)
        {
            ViewBag.Categories = await _db.Categories.ToListAsync();

            bool isExist = await _db.Products.AnyAsync(x => x.Name == product.Name);

            if (isExist)
            {
                ModelState.AddModelError("Name", "This Product Name already is exist !");
                return View();
            }

            #region PhotoSave
            if (product.Photo == null)
            {
                ModelState.AddModelError("Photo", "Photo can not be null");
                return View();
            }

            if (!product.Photo.IsImage())
            {
                ModelState.AddModelError("Photo", "Selecet Image Type");
                return View();
            }

            if (product.Photo.IsOlder216Kb())
            {
                ModelState.AddModelError("Photo", "Max 216Kb");
                return View();
            }

            string folder = Path.Combine(_env.WebRootPath, "img");
            product.Image = await product.Photo.SaveFileAsync(folder);
            #endregion

            product.CategoryId = categoryId;
            await _db.Products.AddAsync(product);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        public async Task<IActionResult> Activity(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            Product dbproduct = await _db.Products.FirstOrDefaultAsync(x => x.Id == Id);
            if (dbproduct == null)
            {
                return BadRequest();
            }

            if (dbproduct.IsDeactive)
            {
                dbproduct.IsDeactive = false;
            }
            else
            {
                dbproduct.IsDeactive = true;
            }

            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}