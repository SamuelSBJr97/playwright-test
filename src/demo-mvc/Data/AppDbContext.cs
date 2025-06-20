using Microsoft.EntityFrameworkCore;
using DemoTestProject.Models;

namespace DemoTestProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<BigTableItem> BigTableItems { get; set; }
    }
}