using System;

namespace GreenLifeOrganicStore.Models
{
    /// <summary>
    /// Represents a single line item inside an Order
    /// Like a row in a shopping cart
    /// </summary>
    public class OrderItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }     // Stored for historical record
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }      // Price at time of order
        public decimal Subtotal { get; set; }       // Quantity * UnitPrice

        public OrderItem()
        {
            Quantity = 1;
            UnitPrice = 0;
            Subtotal = 0;
        }

        public OrderItem(string productId, string productName, int quantity, decimal unitPrice)
        {
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
            CalculateSubtotal();
        }

        /// <summary>
        /// Recalculates subtotal from quantity and unit price
        /// </summary>
        public void CalculateSubtotal()
        {
            Subtotal = Quantity * UnitPrice;
        }

        /// <summary>
        /// Updates quantity and recalculates subtotal
        /// </summary>
        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity < 1)
                throw new ArgumentException("Quantity must be at least 1.");

            Quantity = newQuantity;
            CalculateSubtotal();
        }

        public override string ToString()
        {
            return $"{ProductName} x{Quantity} @ ${UnitPrice:F2} = ${Subtotal:F2}";
        }
    }
}
