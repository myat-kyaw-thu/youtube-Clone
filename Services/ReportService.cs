using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GreenLifeOrganicStore.Models;

namespace GreenLifeOrganicStore.Services
{
    /// <summary>
    /// Generates various reports for admin dashboard
    /// </summary>
    public class ReportService
    {
        private readonly DataService _dataService;

        public ReportService()
        {
            _dataService = new DataService();
        }

        #region Sales Reports

        /// <summary>
        /// Generates sales report for a date range
        /// </summary>
        public SalesReport GenerateSalesReport(DateTime from, DateTime to)
        {
            var orders = _dataService.GetOrdersByDateRange(from, to)
                .Where(o => o.Status != "Cancelled")
                .ToList();

            var report = new SalesReport
            {
                FromDate = from,
                ToDate = to,
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                AverageOrderValue = orders.Count > 0 ? orders.Average(o => o.TotalAmount) : 0,
                Orders = orders
            };

            // Group by status
            report.OrdersByStatus = orders
                .GroupBy(o => o.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            // Group by date
            report.OrdersByDate = orders
                .GroupBy(o => o.OrderDate.Date)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

            return report;
        }

        /// <summary>
        /// Gets daily sales summary
        /// </summary>
        public List<SalesSummary> GetDailySalesSummary(int days = 30)
        {
            var from = DateTime.Now.AddDays(-days);
            var orders = _dataService.GetOrdersByDateRange(from, DateTime.Now)
                .Where(o => o.Status != "Cancelled")
                .ToList();

            return orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new SalesSummary
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    TotalSales = g.Sum(o => o.TotalAmount)
                })
                .OrderByDescending(s => s.Date)
                .ToList();
        }

        #endregion

        #region Stock Reports

        /// <summary>
        /// Generates stock report with low stock alerts
        /// </summary>
        public StockReport GenerateStockReport()
        {
            var products = _dataService.GetAllProducts();
            
            var report = new StockReport
            {
                TotalProducts = products.Count,
                TotalStockValue = products.Sum(p => p.Price * p.Stock),
                LowStockProducts = products.Where(p => p.IsLowStock()).ToList(),
                OutOfStockProducts = products.Where(p => p.Stock == 0).ToList(),
                NearExpiryProducts = products.Where(p => p.IsNearExpiry(30)).ToList()
            };

            // Stock by category
            report.StockByCategory = products
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Stock));

            return report;
        }

        /// <summary>
        /// Gets products that need restocking
        /// </summary>
        public List<Product> GetProductsNeedingRestock()
        {
            return _dataService.GetLowStockProducts();
        }

        /// <summary>
        /// Gets products expiring soon
        /// </summary>
        public List<Product> GetExpiringProducts(int daysThreshold = 30)
        {
            return _dataService.GetNearExpiryProducts(daysThreshold);
        }

        #endregion

        #region Customer Reports

        /// <summary>
        /// Generates customer analytics report
        /// </summary>
        public CustomerReport GenerateCustomerReport()
        {
            var customers = _dataService.GetAllCustomers();
            var analytics = _dataService.GetAllAnalytics();

            var report = new CustomerReport
            {
                TotalCustomers = customers.Count,
                ActiveCustomers = customers.Count(c => c.IsActive),
                TotalRevenue = analytics.Sum(a => a.TotalSpent),
                AverageCustomerValue = analytics.Count > 0 ? analytics.Average(a => a.TotalSpent) : 0
            };

            // Top customers by spending
            report.TopCustomers = analytics
                .OrderByDescending(a => a.TotalSpent)
                .Take(10)
                .ToList();

            // Customers by loyalty tier
            report.CustomersByTier = customers
                .GroupBy(c => c.LoyaltyTier)
                .ToDictionary(g => g.Key, g => g.Count());

            return report;
        }

        /// <summary>
        /// Gets purchase history for a specific customer
        /// </summary>
        public PurchaseAnalytics GetCustomerPurchaseHistory(string customerId)
        {
            return _dataService.GetAnalyticsByCustomerId(customerId);
        }

        /// <summary>
        /// Gets customer order history
        /// </summary>
        public List<Order> GetCustomerOrderHistory(string customerId)
        {
            return _dataService.GetOrdersByCustomerId(customerId);
        }

        #endregion

        #region Order Reports

        /// <summary>
        /// Generates order status report
        /// </summary>
        public OrderReport GenerateOrderReport()
        {
            var orders = _dataService.GetAllOrders();

            var report = new OrderReport
            {
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.Status == "Pending"),
                ShippedOrders = orders.Count(o => o.Status == "Shipped"),
                DeliveredOrders = orders.Count(o => o.Status == "Delivered"),
                CancelledOrders = orders.Count(o => o.Status == "Cancelled")
            };

            // Recent orders
            report.RecentOrders = orders
                .OrderByDescending(o => o.OrderDate)
                .Take(20)
                .ToList();

            return report;
        }

        #endregion
    }

    #region Report Data Classes

    public class SalesReport
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<Order> Orders { get; set; }
        public Dictionary<string, int> OrdersByStatus { get; set; }
        public Dictionary<DateTime, decimal> OrdersByDate { get; set; }

        public string ToDisplayString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== SALES REPORT ===");
            sb.AppendLine($"Period: {FromDate:yyyy-MM-dd} to {ToDate:yyyy-MM-dd}");
            sb.AppendLine($"Total Orders: {TotalOrders}");
            sb.AppendLine($"Total Revenue: ${TotalRevenue:F2}");
            sb.AppendLine($"Average Order Value: ${AverageOrderValue:F2}");
            return sb.ToString();
        }
    }

    public class SalesSummary
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSales { get; set; }
    }

    public class StockReport
    {
        public int TotalProducts { get; set; }
        public decimal TotalStockValue { get; set; }
        public List<Product> LowStockProducts { get; set; }
        public List<Product> OutOfStockProducts { get; set; }
        public List<Product> NearExpiryProducts { get; set; }
        public Dictionary<string, int> StockByCategory { get; set; }
    }

    public class CustomerReport
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageCustomerValue { get; set; }
        public List<PurchaseAnalytics> TopCustomers { get; set; }
        public Dictionary<string, int> CustomersByTier { get; set; }
    }

    public class OrderReport
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public List<Order> RecentOrders { get; set; }
    }

    #endregion
}