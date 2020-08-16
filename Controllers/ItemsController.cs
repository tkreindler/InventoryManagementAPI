using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    /// <summary>
    /// Class to handle all requests on the /items/ route
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {
        /// <summary>
        /// Internal logger provided by ASP.Net
        /// </summary>
        private readonly ILogger<ItemsController> _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger provided by ASP.Net</param>
        public ItemsController(ILogger<ItemsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Respond to a GET request on all Items
        /// </summary>
        /// <returns>A list of all itemtypes</returns>
        [HttpGet]
        public ActionResult<IEnumerable<Item>> Get()
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            using (var db = new DatabaseAccess())
            {
                return db.Items.OrderBy(item => item.Id).ToArray();
            }
        }

        /// <summary>
        /// Respond to a GET request on a specific Item
        /// </summary>
        /// <returns>Details for that Item</returns>
        [HttpGet("id/{id}")]
        public ActionResult<Item> Get(long id)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            using (var db = new DatabaseAccess())
            {
                var item = db.Items.Find(id);

                if (item != null)
                {
                    return item;
                }
                else
                {
                    return NotFound();
                }
            }
        }

        /// <summary>
        /// Respond to a GET request on a specific Item with given qr code
        /// </summary>
        /// <returns>Details for that Item</returns>
        [HttpGet("qr/{qrcode}")]
        public ActionResult<Item> GetByQr(string qrcode)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            using (var db = new DatabaseAccess())
            {
                var item = qrcode == null ? null : db.Items.Where(i => i.QRCode == qrcode).FirstOrDefault();

                if (item != null)
                {
                    return item;
                }
                else
                {
                    return NotFound();
                }
            }
        }

        /// <summary>
        /// Respond to a GET request on a specific Item with given order number
        /// </summary>
        /// <returns>Details for that Item</returns>
        [HttpGet("ordertoseller/{orderNumber}")]
        public ActionResult<IEnumerable<Item>> GetByOrderSeller(string orderNumber)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            if (orderNumber == null)
            {
                return NotFound();
            }

            using (var db = new DatabaseAccess())
            {
                var items = db.Items.Where(i => i.OrderNumberToSeller == orderNumber).ToArray();

                return Ok(items);
            }
        }

        /// <summary>
        /// Respond to a GET request on a specific Item with given order number
        /// </summary>
        /// <returns>Details for that Item</returns>
        [HttpGet("ordertobuyer/{orderNumber}")]
        public ActionResult<IEnumerable<Item>> GetByOrderBuyer(string orderNumber)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            if (orderNumber == null)
            {
                return NotFound();
            }

            using (var db = new DatabaseAccess())
            {
                var items = db.Items.Where(i => i.OrderNumberToBuyer == orderNumber).ToArray();

                return Ok(items);
            }
        }

        /// <summary>
        /// Respond to a GET request on all Items with a given type
        /// </summary>
        /// <returns>A list of all itemtypes</returns>
        [HttpGet("type/{upc}")]
        public ActionResult<IEnumerable<Item>> GetByType(long upc)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            using (var db = new DatabaseAccess())
            {
                if (db.ItemTypes.Find(upc) == null)
                {
                    return NotFound("There isn't an ItemType with that UPC.");
                }

                return Ok(db.Items.Where(item => item.ItemTypeUPC == upc).OrderBy(item => item.Id).ToArray());
            }
        }

        /// <summary>
        /// Delete a specific Item
        /// </summary>
        /// <param name="id">The Id of this item</param>
        /// <returns>HTTP response code</returns>
        [HttpDelete("id/{id}")]
        public IActionResult Delete(long id)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            using (var db = new DatabaseAccess())
            {
                var item = db.Items.Find(id);

                if (item == null)
                {
                    return NotFound();
                }

                db.Items.Remove(item);
                db.SaveChanges();
                return Accepted();
            }
        }

        /// <summary>
        /// Update a specific Item
        /// </summary>
        /// <param name="id">The id to replace at</param>
        /// <param name="itemNoId">The new contents to replace with</param>
        /// <returns>HTTP response code</returns>
        [HttpPut("id/{id}")]
        public IActionResult Update(long id, ItemIn itemNoId)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            using (var db = new DatabaseAccess())
            {
                var itemOld = db.Items.Find(id);

                if (itemOld == null)
                {
                    return NotFound();
                }

                if (itemOld.QRCode != itemNoId.QRCode && itemNoId.QRCode != null && db.Items.Where(i => i.QRCode == itemNoId.QRCode).Any())
                {
                    return Conflict("An item with that qr code already exists");
                }

                itemOld.Copy(itemNoId);
                db.SaveChanges();
                return Accepted();
            }
        }

        /// <summary>
        /// Add a new Item to the server
        /// </summary>
        /// <param name="itemNoId">Item to add</param>
        /// <returns>HTTP response information and the new id of this item</returns>
        [HttpPost]
        public ActionResult<long[]> Create(ItemIn[] itemNoIds)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            // id gets set by database
            var items = itemNoIds.Select(i => new Item(i)).ToArray();

            using (var db = new DatabaseAccess())
            {
                foreach (Item item in items)
                {
                    if (db.ItemTypes.Find(item.ItemTypeUPC) == null)
                    {
                        return NotFound("There isn't an ItemType with that UPC.");
                    }

                    if (item.QRCode != null && db.Items.Where(i => i.QRCode == item.QRCode).Any())
                    {
                        return Conflict("An item with that qr code already exists");
                    }

                    db.Items.Add(item);
                }
                db.SaveChanges();
                return Accepted(items.Select(i => i.Id).ToArray());
            }
        }
    }
}
