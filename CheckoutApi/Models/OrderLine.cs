using System.ComponentModel.DataAnnotations;

namespace CheckoutOrderService.Models
{
    /// <summary>
    /// Representation of an order line for the api client
    /// </summary>
    public class OrderLine
    {
        /// <summary>
        /// The identifier for this order line
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// This line's sort order property - to display the order's lines differently if desired
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// The identifier for the product this order line pertains to
        /// </summary>
        [Required]
        public string SkuCode { get; set; }

        /// <summary>
        /// Indicates the quantity of products this order line pertains to
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// A more human friendly description of the product this order line pertains to
        /// </summary>
        public string SkuDisplayName { get; set; }
    }
}
