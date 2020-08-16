using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace InventoryManagement.Models
{
    /// <summary>
    /// An individual instance of an item in inventory
    /// </summary>
    public class Item : ItemIn
    {
        /// <summary>
        /// The unique identifier for this item
        /// </summary>
        [Key]
        public long Id { get; set; }

        #region Constructors

        public Item() : base()
        {
        }

        /// <summary>
        /// Copy constructor from base
        /// </summary>
        /// <param name="orig">Item to be copied</param>
        public Item(ItemIn orig) : base(orig)
        {
            this.TimeStampOrdered = DateTime.UtcNow;
        }

        public override void Copy(ItemIn orig)
        {
            var oldStatus = this.ItemStatus;

            base.Copy(orig);

            // updated dates if itemStatus changing
            if (orig.ItemStatus != oldStatus)
            {
                if (orig.ItemStatus == ItemStatus.InStock)
                {
                    this.TimeStampReceived = DateTime.UtcNow;
                }
                else if (orig.ItemStatus == ItemStatus.Sold)
                {
                    this.TimeStampSold = DateTime.UtcNow;
                }
            }
        }

        /// <summary>
        /// The time this item was ordered/created
        /// </summary>
        [Column(TypeName = "timestamp")]
        public DateTime TimeStampOrdered { get; set; } = new DateTime();

        /// <summary>
        /// The time this item was marked in-stock
        /// </summary>
        [Column(TypeName = "timestamp")]
        public DateTime TimeStampReceived { get; set; } = new DateTime();

        /// <summary>
        /// The time this item was marked sold
        /// </summary>
        [Column(TypeName = "timestamp")]
        public DateTime TimeStampSold { get; set; } = new DateTime();
        #endregion
    }

    /// <summary>
    /// All important information for the Item class but the Id
    /// </summary>
    public class ItemIn
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public ItemIn()
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="orig">Item to be copied</param>
        public ItemIn(ItemIn orig)
        {
            this.Copy(orig);
        }

        #endregion

        public virtual void Copy(ItemIn orig)
        {
            this.ItemTypeUPC = orig.ItemTypeUPC;
            this.PricePaidByBuyer = orig.PricePaidByBuyer;
            this.PricePaidBySeller = orig.PricePaidBySeller;
            this.ShippingCostToBuyer = orig.ShippingCostToBuyer;
            this.ShippingCostToSeller = orig.ShippingCostToSeller;
            this.Fees = orig.Fees;
            this.OtherExpenses = orig.OtherExpenses;
            this.ShippingPaidByBuyer = orig.ShippingPaidByBuyer;
            this.QRCode = orig.QRCode;
            this.ItemStatus = orig.ItemStatus;
            this.TaxPaidBySeller = orig.TaxPaidBySeller;
            this.OrderNumberToSeller = orig.OrderNumberToSeller;
            this.OrderNumberToBuyer = orig.OrderNumberToBuyer;
        }

        /// <summary>
        /// The UPC of the type of doll, toy, etc of this item
        /// </summary>
        [Required]
        public long ItemTypeUPC { get; set; }

        /// <summary>
        /// The order number for the order containing this item
        /// </summary>
        public string OrderNumberToSeller { get; set; }

        /// <summary>
        /// The order number for the order containing this item
        /// </summary>
        public string OrderNumberToBuyer { get; set; }

        /// <summary>
        /// The random QRCode string for this item
        /// </summary>
        public string QRCode { get; set; }

        /// <summary>
        /// The status of the item
        /// </summary>
        [Required]
        public ItemStatus ItemStatus { get; set; }

        /// <summary>
        /// Getter for the profit made on this item
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public decimal Profit => this.Revenue - this.Expenses;

        #region Expenses

        /// <summary>
        /// Getter for the expenses of this item
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public decimal Expenses =>
            this.PricePaidByBuyer + this.TaxPaidBySeller + this.ShippingCostToSeller +
            this.ShippingCostToBuyer + this.Fees + this.OtherExpenses;

        /// <summary>
        /// Price we paid for this item
        /// </summary>
        public decimal PricePaidBySeller { get; set; }

        /// <summary>
        /// Tax we paid for this item
        /// </summary>
        public decimal TaxPaidBySeller { get; set; }

        /// <summary>
        /// Shipping we paid to get this item
        /// </summary>
        public decimal ShippingCostToSeller { get; set; }

        /// <summary>
        /// Shipping we paid to send this item to the seller
        /// </summary>
        public decimal ShippingCostToBuyer { get; set; }

        /// <summary>
        /// Fees collected by Ebay, Amazon, etc.
        /// </summary>
        public decimal Fees { get; set; }

        /// <summary>
        /// Any other expenses that may have ensued
        /// </summary>
        public decimal OtherExpenses { get; set; }

        #endregion

        #region Revenues

        /// <summary>
        /// Getter for the revenue of this item
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public decimal Revenue =>
            this.ShippingPaidByBuyer + this.PricePaidByBuyer;

        /// <summary>
        /// Shipping price the buyer paid to us to ship to them
        /// </summary>
        public decimal ShippingPaidByBuyer { get; set; }

        /// <summary>
        /// Price the buyer paid to us
        /// </summary>
        public decimal PricePaidByBuyer { get; set; }

        #endregion
    }
}
