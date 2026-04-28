using System;
using System.Collections.Generic;
using System.Linq;

namespace GreenLifeOrganicStore.Models
{
    /// <summary>
    /// Tracks purchase history and analytics for a single customer
    /// Used for reports and customer insights
    /// </summary>
    public class PurchaseAnalytics
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal AverageOrderValue { get; set; }
        public string FavoriteCategory { get; set; }
        public DateTime LastPurchaseDate { get; set; }
        public List<PurchaseRecord> PurchaseHistory { get; set; }

        public PurchaseAnalytics()
        {
            PurchaseHistory = new List<PurchaseRecord>();
            TotalOrders = 0;
            TotalSpent = 0;
            AverageOrderValue = 0;
            FavoriteCategory = "";
        }

        public PurchaseAnalytics(string customerId, string customerName)
        {
            CustomerId = customerId;
            CustomerName = customerName;
            PurchaseHistory = new List<PurchaseRecord>();
            TotalOrders = 0;
            TotalSpent = 0;
            AverageOrderValue = 0;
            FavoriteCategory = "";
        }

        /// <summary>
        /// Adds a new purchase record and recalculates all analytics
        /// </summary>
        public void AddPurchase(DateTime date, decimal amount, int productCount, string category = "")
        {
            PurchaseHistory.Add(new PurchaseRecord
            {
                Date = date,
                Amount = amount,
                ProductCount = productCount,
                Category = category
            });

            RecalculateAnalytics();
        }

        /// <summary>
        /// Recalculates all analytics from purchase history
        /// </summary>
        public void RecalculateAnalytics()
        {
            if (PurchaseHistory == null || PurchaseHistory.Count == 0)
                return;

            TotalOrders = PurchaseHistory.Count;
            TotalSpent = PurchaseHistory.Sum(p => p.Amount);
            AverageOrderValue = TotalOrders > 0 ? TotalSpent / TotalOrders : 0;
            LastPurchaseDate = PurchaseHistory.Max(p => p.Date);

            // Find favorite category (most purchased)
            var categoryGroups = PurchaseHistory
                .Where(p => !string.IsNullOrEmpty(p.Category))
                .GroupBy(p => p.Category)
                .OrderByDescending(g => g.Count());

            FavoriteCategory = categoryGroups.FirstOrDefault()?.Key ?? "";
        }

        /// <summary>
        /// Returns purchase history filtered by date range
        /// </summary>
        public List<PurchaseRecord> GetHistoryByDateRange(DateTime from, DateTime to)
        {
            return PurchaseHistory
                .Where(p => p.Date >= from && p.Date <= to)
                .OrderByDescending(p => p.Date)
                .ToList();
        }
    }

    /// <summary>
    /// A single purchase entry in the analytics history
    /// </summary>
    public class PurchaseRecord
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public int ProductCount { get; set; }
        public string Category { get; set; }

        public PurchaseRecord()
        {
            Date = DateTime.Now;
            Category = "";
        }

        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd} - ${Amount:F2} ({ProductCount} items)";
        }
    }
}
