using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InventoryManagement.Models;

namespace InventoryManagement.Controllers
{
    /// <summary>
    /// Class to handle all requests on the /itemtypes/ route
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ItemTypesController : ControllerBase
    {
        /// <summary>
        /// Internal logger provided by ASP.Net
        /// </summary>
        private readonly ILogger<ItemTypesController> _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger">Logger provided by ASP.Net</param>
        public ItemTypesController(ILogger<ItemTypesController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Respond to a GET request on all ItemTypes
        /// </summary>
        /// <returns>A list of all itemtypes</returns>
        [HttpGet]
        public ActionResult<IEnumerable<ItemType>> Get()
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            using (var db = new DatabaseAccess())
            {
                return db.ItemTypes.OrderBy(it => it.Name).ToArray();
            }
        }

        /// <summary>
        /// Respond to a GET request on a specific ItemType
        /// </summary>
        /// <param name="upc">The UPC of this ItemType</param>
        /// <returns>Details for that ItemType</returns>
        [HttpGet("{upc}")]
        public ActionResult<ItemType> Get(long upc)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            using (var db = new DatabaseAccess())
            {
                var itemType = db.ItemTypes.Find(upc);

                if (itemType != null)
                {
                    return itemType;
                }
                else
                {
                    return NotFound();
                }
            }
        }

        /// <summary>
        /// Delete a specific ItemType
        /// </summary>
        /// <param name="upc">The UPC of this ItemType</param>
        /// <returns>HTTP response code</returns>
        [HttpDelete("{upc}")]
        public IActionResult Delete(long upc)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            using (var db = new DatabaseAccess())
            {
                var itemType = db.ItemTypes.Find(upc);

                if (itemType == null)
                {
                    return NotFound();
                }

                bool hasChildren = db.Items.Where(
                    item => item.ItemTypeUPC == upc).Any();

                if (hasChildren)
                {
                    return Unauthorized("Can't delete an ItemType while some Items reference it.");
                }

                db.ItemTypes.Remove(itemType);
                db.SaveChanges();
                return Accepted();
            }
        }

        /// <summary>
        /// Update a specific ItemType
        /// </summary>
        /// <param name="upc">The upc to replace at</param>
        /// <param name="itemTypeNew">The new contents to replace with</param>
        /// <returns>HTTP response code</returns>
        [HttpPut("{upc}")]
        public IActionResult Update(long upc, ItemType itemTypeNew)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            using (var db = new DatabaseAccess())
            {
                if (itemTypeNew.UPC != upc)
                {
                    return Conflict("Can't change the upc.");
                }

                var itemTypeOld = db.ItemTypes.Find(upc);

                if (itemTypeOld == null)
                {
                    return NotFound();
                }

                itemTypeOld.Copy(itemTypeNew);
                db.SaveChanges();
                return Accepted();
            }
        }

        /// <summary>
        /// Add a new ItemType to the server
        /// </summary>
        /// <param name="itemType">ItemType to add</param>
        /// <returns>HTTP response information</returns>
        [HttpPost]
        public IActionResult Create(ItemType itemType)
        {
            if (!AuthenticationController.CheckToken(HttpContext, out string _))
            {
                return Unauthorized();
            }

            using (var db = new DatabaseAccess())
            {
                var exists = db.ItemTypes.Where(
                    i => i.Name == itemType.Name ||
                    i.UPC == itemType.UPC).Any();

                if (exists)
                {
                    return Conflict("ItemType with that name/upc already exists.");
                }
                else
                {

                    db.ItemTypes.Add(itemType);
                    db.SaveChanges();
                    return Accepted();
                }
            }
        }
    }
}
