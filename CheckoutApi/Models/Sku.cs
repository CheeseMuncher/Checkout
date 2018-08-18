namespace CheckoutApi.Models
{
    /// <summary>
    /// Representation of a sku for the api client
    /// </summary>
    public class Sku
    {
        /// <summary>
        /// A system identifier for the Sku
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// A more human friendly description of the product the Sku represents
        /// </summary>
        public string DisplayName { get; set; }
    }
}
