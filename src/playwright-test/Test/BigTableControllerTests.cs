using Microsoft.VisualStudio.TestTools.UnitTesting;
using DemoTestProject.Controllers;
using DemoTestProject.Data;
using DemoTestProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace Test
{
    [TestClass]
    public class BigTableControllerTests
    {
        private AppDbContext _context;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            _context = new AppDbContext(options);

            for (int i = 1; i <= 1_000_000; i++)
            {
                _context.BigTableItems.Add(new BigTableItem { Id = i, Name = $"Item {i}", Value = $"Valor {i}" });
            }
            _context.SaveChanges();
        }

        [TestMethod]
        public async Task Index_Deve_Retornar_Itens_Paginados()
        {
            var controller = new BigTableController(_context);
            var result = await controller.Index(2) as ViewResult;
            var model = result.Model as System.Collections.Generic.IEnumerable<BigTableItem>;

            Assert.IsNotNull(model);
            Assert.AreEqual(100, model.Count()); // PageSize = 100
            Assert.AreEqual(101, model.First().Id);
        }
    }
}