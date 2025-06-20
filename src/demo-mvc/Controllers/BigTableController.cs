using DemoTestProject.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoTestProject.Controllers
{
    public class BigTableController : Controller
    {
        private readonly AppDbContext _context;
        private const int PageSize = 100;

        public BigTableController(AppDbContext context) => _context = context;

        public async Task<IActionResult> Index(int page = 1)
        {
            var totalItems = await _context.BigTableItems.CountAsync();
            var items = await _context.BigTableItems
                .OrderBy(x => x.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

            return View(items);
        }
    }
}