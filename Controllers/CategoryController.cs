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

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            // Find the category by ID
            Category categoryFromDb = _db.Category.Find(id);
            //Find the category by ID using Linq condition
            //Category categoryFromDb = _db.Category.FirstOrDefault(c => c.Id == id);
            //Find the category by ID using Lambda expression
            //Category categoryFromDb = _db.Category.Where(c => c.Id == id).FirstOrDefault();

            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, Name, DisplayOrder")] Category input)
        {
            if(id != input.Id) return BadRequest();

            //Normalize Inputs
            input.Name = input.Name.Trim();

            //Business Rule: Name must not exactly match DisplayOrder
            if(!string.IsNullOrEmpty(input.Name) && input.Name.Equals(input.DisplayOrder.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("name", "The Display Order cannot exactly match the Name.");
            }

            //Duplicate-name check (case-insensitive, trim-aware), excluding current record
            if (!string.IsNullOrWhiteSpace(input.Name))
            {
                var exists = await _db.Category
                    .AsNoTracking()
                    .AnyAsync(c => c.Id != input.Id && c.Name.Trim().ToLower() == input.Name.ToLower());
                if (exists)
                {
                    ModelState.AddModelError(nameof(Category.Name), "A category with this name already exists.");
                }
            }

            if(!ModelState.IsValid)
            {
                TempData["error"] = "Please correct the errors in the form.";
                return View(input);
            }
            try {
                Category categoryFromDb = await _db.Category.FindAsync(id);
                if(categoryFromDb == null) return NotFound();

                //Update only the fields that are allowed to be changed to avoid overposting
                categoryFromDb.Name = input.Name;
                categoryFromDb.DisplayOrder = input.DisplayOrder;
                _db.Category.Update(categoryFromDb);
                await _db.SaveChangesAsync();
                TempData["success"] = "Category updated successfully.";
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException ex) {
                //_logger.LogError(ex, "Concurrency error updating category with ID {CategoryId}", id);
                ModelState.AddModelError(string.Empty, "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled. Please try again.");
                TempData["error"] = "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled. Please try again.";
                return View(input);

            }
            catch (DbUpdateException dbEx)
            {
                // Log the exception (you can use a logging framework here)
                Console.Error.WriteLine(dbEx);
                // Add a model error to inform the user
                ModelState.AddModelError(string.Empty, "An error occurred while updating the category. Please try again later.");
                TempData["error"] = "An error occurred while updating the category. Please try again later.";
                return View(input);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.Error.WriteLine(ex);
                // Add a model error to inform the user
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                TempData["error"] = "An unexpected error occurred. Please try again later.";
                return View(input);
            }

        }
    }
}
