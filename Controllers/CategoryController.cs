using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db) { 
           
            _db = db;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _db.Category.ToList();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("Name, DisplayOrder")] Category obj)
        {
            //Normalize Inputs
            obj.Name = obj.Name.Trim();

            //Server-side Validation
            if (!string.IsNullOrEmpty(obj.Name) && obj.Name.Equals(obj.DisplayOrder.ToString()))
            {
                ModelState.AddModelError("name", "The Display Order cannot exactly match the Name.");
            }
            //Unique Name check
            // Duplicate-name check (case-insensitive, trim-aware)
            if (!string.IsNullOrWhiteSpace(obj.Name))
            {
                var exists = await _db.Category
                    .AsNoTracking()
                    .AnyAsync(c => c.Name.Trim().ToLower() == obj.Name.ToLower());

                if (exists)
                {
                    ModelState.AddModelError(nameof(Category.Name), "A category with this name already exists.");
                }
            }
            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please correct the errors in the form.";
                return View(obj);
            }

            try
            {
                await _db.Category.AddAsync(obj);
                await _db.SaveChangesAsync();
                TempData["success"] = "Category created successfully.";
                return RedirectToAction("Index");
            }
            catch (DbUpdateException dbEx)
            {
                // Log the exception (you can use a logging framework here)
                Console.Error.WriteLine(dbEx);
                // Add a model error to inform the user
                ModelState.AddModelError(string.Empty, "An error occurred while saving the category. Please try again later.");
                TempData["error"] = "An error occurred while saving the category. Please try again later.";
                return View(obj);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.Error.WriteLine(ex);
                // Add a model error to inform the user
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                TempData["error"] = "An unexpected error occurred. Please try again later.";
                return View(obj);
            }
        }
    }
}
