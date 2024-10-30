using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceParser.UAI.Server
{
    [ApiController]
    [Route("api/[controller]")]
    public class RuntimeServer : ControllerBase
    {

        private static readonly List<Item> items = new List<Item>
        {
            new Item { Id = 1, Name = "Item1", Description = "First item description" },
            new Item { Id = 2, Name = "Item2", Description = "Second item description" }
        };

        // GET: api/items
        [HttpGet]
        public ActionResult<IEnumerable<Item>> GetItems()
        {
            return Ok(items);
        }

        // GET: api/items/{id}
        [HttpGet("{id}")]
        public ActionResult<Item> GetItem(int id)
        {
            var item = items.Find(i => i.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        // POST: api/items
        // Accepts a JSON object and parses it into an Item object
        [HttpPost]
        public ActionResult<Item> CreateItem([FromBody] Item newItem)
        {
            if (newItem == null || string.IsNullOrEmpty(newItem.Name))
            {
                return BadRequest("Invalid item data.");
            }

            // Simulate assigning a new ID
            newItem.Id = items.Count + 1;
            items.Add(newItem);

            // Return the created item and the URL to access it
            return CreatedAtAction(nameof(GetItem), new { id = newItem.Id }, newItem);
        }

        // POST: api/items/bulk
        // Accepts a JSON array of items and parses it into a List<Item>
        [HttpPost("bulk")]
        public ActionResult<IEnumerable<Item>> CreateItems([FromBody] List<Item> newItems)
        {
            if (newItems == null || newItems.Count == 0)
            {
                return BadRequest("No items provided.");
            }

            foreach (var item in newItems)
            {
                item.Id = items.Count + 1;
                items.Add(item);
            }

            return Ok(newItems);
        }
        public class StartupServer
        {
            public void ConfigureServices(IServiceCollection services)
            {
                services.AddControllers().AddNewtonsoftJson(); // Add NewtonsoftJson if needed for JSON serialization
            }

            public void Configure(IApplicationBuilder app, IHostEnvironment env)
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            }
        }

    }





    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
