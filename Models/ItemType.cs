using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    /// <summary>
    /// A unique item type classified by UPC number
    /// </summary>
    public class ItemType
    {
        /// <summary>
        /// The name of this item type
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The unique UPC number of this item
        /// </summary>
        [Key]
        public long UPC { get; set; }

        /// <summary>
        /// The URL the image for this ItemType resides at
        /// </summary>
        public string ImageURL { get; set; }

        /// <summary>
        /// Copy all internal fields with that of another
        /// </summary>
        /// <param name="copy">The object being copied</param>
        public void Copy(ItemType orig)
        {
            this.Name = orig.Name;
            this.UPC = orig.UPC;
            this.ImageURL = orig.ImageURL;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ItemType()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="orig">The object being copied</param>
        public ItemType(ItemType orig)
        {
            this.Copy(orig);
        }
    }
}
