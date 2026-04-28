using System;
using System.Collections.Generic;
using System.Linq;

namespace GreenLifeOrganicStore.Models
{
    /// <summary>
    /// Order entity - represents a customer purchase
    /// Status flow: Pending → Shipped → Delivered
    /// </summary>
    public class Order
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }    // Stored for display/reports
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }          // Pending, Shipped, Delivered, Cancelled
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public List<OrderItem> Items { get; set; }
        public string Notes { get; set; }
        public DateTime LastUpdated { get; set; }

        // Valid order statuses
        public static readonly string[] ValidStatuses = { "Pending", "Shipped", "Delivered", "Cancelled" };

        public Order()
        {
            Id = Guid.NewGuid().ToString();
            OrderDate = DateTime.Now;
            LastUpdated = DateTime.Now;
            Status = "Pending";
            Items = new List<OrderItem>();
            TotalAmount = 0;
            Notes = "";
        }

        public Order(string customerId, string customerName, string shippingAddress)
        {
            Id = Guid.NewGuid().ToString();
            CustomerId = customerId;
            CustomerName = customerName;
            ShippingAddress = shippingAddress;
            OrderDate = DateTime.Now;
            LastUpdated = DateTime.Now;
            Status = "Pending";
            Items = new List<OrderItem>();
            TotalAmount = 0;
            Notes = "";
        }

        /// <summary>
        /// Adds an item to the order and recalculates total
        /// </summary>
        public void AddItem(OrderItem item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            // If product already in order, increase quantity
            var existing = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existing != null)
            {
                existing.UpdateQuantity(existing.Quantity + item.Quantity);
            }
            else
            {
                Items.Add(item);
            }

            CalculateTotal();
        }

        /// <summary>
        /// Removes an item from the order by product ID
        /// </summary>
        public void RemoveItem(string productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
            CalculateTotal();
        }

        /// <summary>
        /// Recalculates total from all items
        /// </summary>
        public void CalculateTotal()
        {
            TotalAmount = Items.Sum(i => i.Subtotal);
            LastUpdated = DateTime.Now;
        }

        /// <summary>
        /// Updates order status with validation
        /// </summary>
        public bool UpdateStatus(string newStatus)
        {
            if (!Array.Exists(ValidStatuses, s => s == newStatus))
                return false;

            Status = newStatus;
            LastUpdated = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Returns true if order can still be cancelled (only Pending orders)
        /// </summary>
        public bool CanBeCancelled()
        {
            return Status == "Pending";
        }

        /// <summary>
        /// Returns total number of items in the order
        /// </summary>
        public int GetTotalItemCount()
        {
            return Items.Sum(i => i.Quantity);
        }

        public override string ToString()
        {
            return $"Order #{Id.Substring(0, 8)} - {CustomerName} - ${TotalAmount:F2} - {Status}";
        }
    }
}
