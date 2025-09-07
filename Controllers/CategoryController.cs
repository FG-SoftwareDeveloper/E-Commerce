using System.Globalization;
using System.Text.RegularExpressions;
using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace E_Commerce.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ApplicationDbContext db, ILogger<CategoryController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: /Category
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            // AsNoTracking for read-only list (faster, less memory)
            var categories = await _db.Category
                .AsNoTracking()
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(ct);

            return View(categories);
        }

        // GET: /Category/Create
        [HttpGet]
        public IActionResult Create() => View();

        // POST: /Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(nameof(Category.Name), nameof(Category.DisplayOrder))] Category input,
            CancellationToken ct)
        {
            if (input is null) return BadRequest();

            Normalize(input);

            // Validation
            ValidateCategory(input);

            // Duplicate-name check (case-insensitive), only if current model state is valid so far
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(input.Name))
            {
                var exists = await _db.Category.AsNoTracking()
                    .AnyAsync(c => c.Name.ToLower() == input.Name.ToLower(), ct);

                if (exists)
                    ModelState.AddModelError(nameof(Category.Name), "A category with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please correct the errors in the form.";
                return View(input);
            }

            try
            {
                await _db.Category.AddAsync(input, ct);
                await _db.SaveChangesAsync(ct);

                TempData["success"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error creating Category {@Category}", input);
                ModelState.AddModelError(string.Empty, "An error occurred while saving the category. Please try again later.");
                TempData["error"] = "An error occurred while saving the category. Please try again later.";
                return View(input);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating Category {@Category}", input);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                TempData["error"] = "An unexpected error occurred. Please try again later.";
                return View(input);
            }
        }

        // GET: /Category/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id, CancellationToken ct)
        {
            if (id is null or <= 0) return NotFound();

            var category = await _db.Category
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (category is null) return NotFound();

            return View(category);
        }

        // POST: /Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind(nameof(Category.Id), nameof(Category.Name), nameof(Category.DisplayOrder))] Category input,
            CancellationToken ct)
        {
            if (input is null || id != input.Id) return BadRequest();

            Normalize(input);
            ValidateCategory(input);

            // Duplicate (exclude current record)
            if (ModelState.IsValid && !string.IsNullOrWhiteSpace(input.Name))
            {
                var exists = await _db.Category.AsNoTracking()
                    .AnyAsync(c => c.Id != input.Id && c.Name.ToLower() == input.Name.ToLower(), ct);

                if (exists)
                    ModelState.AddModelError(nameof(Category.Name), "A category with this name already exists.");
            }

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Please correct the errors in the form.";
                return View(input);
            }

            try
            {
                var categoryFromDb = await _db.Category.FirstOrDefaultAsync(c => c.Id == id, ct);
                if (categoryFromDb is null) return NotFound();

                // Whitelist fields to avoid over-posting
                categoryFromDb.Name = input.Name;
                categoryFromDb.DisplayOrder = input.DisplayOrder;

                await _db.SaveChangesAsync(ct);

                TempData["success"] = "Category updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency error updating Category Id={CategoryId}", id);
                ModelState.AddModelError(string.Empty, "The record was modified by another user. Please reload and try again.");
                TempData["error"] = "The record was modified by another user. Please reload and try again.";
                return View(input);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error updating Category {@Category}", input);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the category. Please try again later.");
                TempData["error"] = "An error occurred while updating the category. Please try again later.";
                return View(input);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating Category {@Category}", input);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                TempData["error"] = "An unexpected error occurred. Please try again later.";
                return View(input);
            }
        }

        // GET: /Category/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id, CancellationToken ct)
        {
            if (id is null or <= 0) return NotFound();

            var category = await _db.Category
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (category is null) return NotFound();

            return View(category);
        }

        // POST: /Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, CancellationToken ct)
        {
            if (id is null or <= 0) return NotFound();

            var entity = await _db.Category.FirstOrDefaultAsync(c => c.Id == id, ct);
            if (entity is null) return NotFound();

            try
            {
                _db.Category.Remove(entity);
                await _db.SaveChangesAsync(ct);

                TempData["success"] = "Category deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "DB error deleting Category Id={CategoryId}", id);
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the category. Please try again later.");
                TempData["error"] = "An error occurred while deleting the category. Please try again later.";
                return View(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting Category Id={CategoryId}", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again later.");
                TempData["error"] = "An unexpected error occurred. Please try again later.";
                return View(entity);
            }
        }

        #region Helpers

        private static void Normalize(Category c)
        {
            // Safe-guard nulls
            c.Name = (c.Name ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(c.Name))
            {
                // Collapse internal whitespace to single spaces
                c.Name = Regex.Replace(c.Name, @"\s+", " ");
            }
        }

        private void ValidateCategory(Category c)
        {
            if (string.IsNullOrWhiteSpace(c.Name))
                ModelState.AddModelError(nameof(Category.Name), "Name is required.");

            if (c.DisplayOrder <= 0)
                ModelState.AddModelError(nameof(Category.DisplayOrder), "Display Order must be greater than zero.");

            if (!string.IsNullOrEmpty(c.Name) &&
                c.Name.Equals(c.DisplayOrder.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(Category.Name), "The Display Order cannot exactly match the Name.");
            }
        }

        #endregion
    }
}
