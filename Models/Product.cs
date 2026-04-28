using System;

namespace GreenLifeOrganicStore.Models
{
    /// <summary>
    /// Product entity - represents an item in the organic store catalog
    /// </summary>
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Description { get; set; }
        public string SupplierId { get; set; }
        public decimal Rating { get; set; }         // 0.0 to 5.0
        public string ImageUrl { get; set; }        // Path to product image
        public DateTime ExpiryDate { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsActive { get; set; }
        public int LowStockThreshold { get; set; }  // Alert when stock below this

        public Product()
        {
            Id = Guid.NewGuid().ToString();
            DateAdded = DateTime.Now;
            IsActive = true;
            Rating = 0.0m;
            LowStockThreshold = 10;
            ImageUrl = "";
        }

        public Product(string name, string category, decimal price, int stock, string description, string supplierId)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Category = category;
            Price = price;
            Stock = stock;
            Description = description;
            SupplierId = supplierId;
            Rating = 0.0m;
            ImageUrl = "";
            DateAdded = DateTime.Now;
            IsActive = true;
            LowStockThreshold = 10;
        }

        /// <summary>
        /// Returns true if stock is at or below the low stock threshold
        /// </summary>
        public bool IsLowStock()
        {
            return Stock <= LowStockThreshold;
        }

        /// <summary>
        /// Returns true if product expires within the given number of days
        /// </summary>
        public bool IsNearExpiry(int daysThreshold = 30)
        {
            return ExpiryDate != DateTime.MinValue
                && ExpiryDate <= DateTime.Now.AddDays(daysThreshold);
        }

        /// <summary>
        /// Returns true if product is already expired
        /// </summary>
        public bool IsExpired()
        {
            return ExpiryDate != DateTime.MinValue
                && ExpiryDate < DateTime.Now;
        }

        /// <summary>
        /// Validates required product fields
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(Category)
                && Price >= 0
                && Stock >= 0;
        }

        public override string ToString()
        {
            return $"{Name} - {Category} - ${Price:F2} (Stock: {Stock})";
        }
    }
}
