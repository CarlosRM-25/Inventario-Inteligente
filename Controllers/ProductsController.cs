using Inventario_Inteligente.Data;
using Inventario_Inteligente.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario_Inteligente.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Paso 1: Index con búsqueda
        // GET: /Products?searchString=texto
        public async Task<IActionResult> Index(string searchString)
        {
            IQueryable<Product> productsQuery = _context.Products;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                // Búsqueda case-insensitive (SQL Server collation puede ya ser case-insensitive)
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString));
            }

            var products = await productsQuery
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewData["CurrentFilter"] = searchString;
            return View(products);
        }

        // Paso 2: Edit - GET
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // Paso 2: Edit - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Price,Stock")] Product formModel)
        {
            if (id != formModel.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                // Recargar entidad para mostrar nombre u otros campos que no se editaron
                var original = await _context.Products.AsNoTracking().FirstOrDefaultAsync((Product p) => p.Id == id);
                if (original == null) return NotFound();
                // Mostrar la vista con el original (puedes mapear)
                original.Price = formModel.Price;
                original.Stock = formModel.Stock;
                return View(original);
            }

            try
            {
                // Carga la entidad real
                var productToUpdate = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
                if (productToUpdate == null) return NotFound();

                productToUpdate.Price = formModel.Price;
                productToUpdate.Stock = formModel.Stock;

                _context.Update(productToUpdate);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Producto actualizado correctamente.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(formModel.Id)) return NotFound();
                else throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // Paso 3: Delete - GET (confirmación)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // Paso 3: Delete - POST (confirmado)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Producto eliminado.";
            return RedirectToAction(nameof(Index));
        }

        // Utilitario
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // Paso 4: Estadísticas (LINQ Avanzado)
        public async Task<IActionResult> Statistics()
        {
            var products = _context.Products;

            // 1. Reporte de precios: más caro -> más barato
            var topToBottom = await products
                .OrderByDescending(p => p.Price)
                .ToListAsync();

            // 2. Promedio de precio
            decimal avgPrice = 0m;
            if (await products.AnyAsync())
            {
                avgPrice = await products.AverageAsync(p => p.Price);
            }

            // 3. Valor total del inventario: SUM(Price * Stock)
            decimal totalInventoryValue = 0m;
            totalInventoryValue = await products.SumAsync(p => p.Price * p.Stock);

            // 4. Alerta de stock crítico (<5)
            var criticalStock = await products
                .Where(p => p.Stock < 5)
                .OrderBy(p => p.Stock)
                .ToListAsync();

            // Empaqueta en un ViewModel
            var vm = new InventoryStatisticsViewModel
            {
                ProductsOrderedByPriceDesc = topToBottom,
                AveragePrice = avgPrice,
                TotalInventoryValue = totalInventoryValue,
                CriticalStockProducts = criticalStock
            };

            return View(vm);
        }
    }

    [Serializable]
    internal class DbUpdateConcurrencyException : Exception
    {
        public DbUpdateConcurrencyException()
        {
        }

        public DbUpdateConcurrencyException(string? message) : base(message)
        {
        }

        public DbUpdateConcurrencyException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}