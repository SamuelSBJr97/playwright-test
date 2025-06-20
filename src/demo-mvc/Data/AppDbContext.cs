using Microsoft.EntityFrameworkCore;
using DemoTestProject.Models;

namespace DemoTestProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<BigTableItem> BigTableItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var items = new List<BigTableItem>();
            for (int i = 1; i <= 1_000_000; i++)
            {
                items.Add(new BigTableItem
                {
                    Id = i,
                    Name = $"Item {i}",
                    Value = $"Valor {i}"
                });
            }
            modelBuilder.Entity<BigTableItem>().HasData(items);
        }
    }
}