using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GreenLifeOrganicStore.Models;

namespace GreenLifeOrganicStore.Services
{
    /// <summary>
    /// Handles CSV import/export for bulk data operations
    /// </summary>
    public class CSVService
    {
        private readonly DataService _dataService;

        public CSVService()
        {
            _dataService = new DataService();
        }

        #region Product Import/Export

        /// <summary>
        /// Exports all products to a CSV file
        /// </summary>
        public void ExportProductsToCSV(string filePath)
        {
            var products = _dataService.GetAllProducts();
            ExportProductsToCSV(filePath, products);
        }

        /// <summary>
        /// Exports specific products to CSV
        /// </summary>
        public void ExportProductsToCSV(string filePath, List<Product> products)
        {
            try
            {
                var sb = new StringBuilder();
                
                // Header row
                sb.AppendLine("Id,Name,Category,Price,Stock,Description,SupplierId,Rating,ImageUrl,ExpiryDate,DateAdded,IsActive,LowStockThreshold");

                // Data rows
                foreach (var p in products)
                {
                    sb.AppendLine($"\"{p.Id}\",\"{EscapeCSV(p.Name)}\",\"{EscapeCSV(p.Category)}\",{p.Price},{p.Stock},\"{EscapeCSV(p.Description)}\",\"{p.SupplierId}\",{p.Rating},\"{p.ImageUrl}\",\"{p.ExpiryDate:yyyy-MM-dd}\",\"{p.DateAdded:yyyy-MM-dd}\",{p.IsActive},{p.LowStockThreshold}");
                }

                File.WriteAllText(filePath, sb.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting products: {ex.Message}");
            }
        }

        /// <summary>
        /// Imports products from a CSV file
        /// </summary>
        public List<Product> ImportProductsFromCSV(string filePath)
        {
            var products = new List<Product>();

            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("CSV file not found", filePath);

                var lines = File.ReadAllLines(filePath);
                
                // Skip header row
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var product = ParseProductCSVLine(line);
                    if (product != null)
                    {
                        products.Add(product);
                    }
                }

                // Add imported products to database
                foreach (var product in products)
                {
                    _dataService.AddProduct(product);
                }

                return products;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error importing products: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses a single CSV line into a Product
        /// </summary>
        private Product ParseProductCSVLine(string line)
        {
            try
            {
                // Simple CSV parsing - assumes no commas in fields
                var fields = ParseCSVLine(line);
                
                if (fields.Count < 4)
                    return null;

                return new Product
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = fields[1],
                    Category = fields[2],
                    Price = decimal.Parse(fields[3]),
                    Stock = int.Parse(fields[4]),
                    Description = fields.Count > 5 ? fields[5] : "",
                    SupplierId = fields.Count > 6 ? fields[6] : "",
                    Rating = fields.Count > 7 && !string.IsNullOrEmpty(fields[7]) ? decimal.Parse(fields[7]) : 0,
                    ImageUrl = fields.Count > 8 ? fields[8] : "",
                    ExpiryDate = fields.Count > 9 && !string.IsNullOrEmpty(fields[9]) ? DateTime.Parse(fields[9]) : DateTime.MinValue,
                    DateAdded = DateTime.Now,
                    IsActive = true,
                    LowStockThreshold = 10
                };
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Customer Import/Export

        /// <summary>
        /// Exports all customers to CSV
        /// </summary>
        public void ExportCustomersToCSV(string filePath)
        {
            var customers = _dataService.GetAllCustomers();
            
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Id,Username,Email,FullName,Phone,Address,LoyaltyTier,TotalSpent,TotalOrders,RegistrationDate,IsActive");

                foreach (var c in customers)
                {
                    sb.AppendLine($"\"{c.Id}\",\"{EscapeCSV(c.Username)}\",\"{c.Email}\",\"{EscapeCSV(c.FullName)}\",\"{c.Phone}\",\"{EscapeCSV(c.Address)}\",\"{c.LoyaltyTier}\",{c.TotalSpent},{c.TotalOrders},\"{c.RegistrationDate:yyyy-MM-dd}\",{c.IsActive}");
                }

                File.WriteAllText(filePath, sb.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting customers: {ex.Message}");
            }
        }

        #endregion

        #region Order Import/Export

        /// <summary>
        /// Exports orders to CSV
        /// </summary>
        public void ExportOrdersToCSV(string filePath, DateTime? from = null, DateTime? to = null)
        {
            var orders = from.HasValue && to.HasValue 
                ? _dataService.GetOrdersByDateRange(from.Value, to.Value)
                : _dataService.GetAllOrders();

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("OrderId,CustomerId,CustomerName,OrderDate,Status,TotalAmount,ShippingAddress,ItemCount");

                foreach (var o in orders)
                {
                    sb.AppendLine($"\"{o.Id}\",\"{o.CustomerId}\",\"{EscapeCSV(o.CustomerName)}\",\"{o.OrderDate:yyyy-MM-dd}\",\"{o.Status}\",{o.TotalAmount},\"{EscapeCSV(o.ShippingAddress)}\",{o.Items.Count}");
                }

                File.WriteAllText(filePath, sb.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting orders: {ex.Message}");
            }
        }

        #endregion

        #region Supplier Import/Export

        /// <summary>
        /// Exports suppliers to CSV
        /// </summary>
        public void ExportSuppliersToCSV(string filePath)
        {
            var suppliers = _dataService.GetAllSuppliers();

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("Id,Name,ContactPerson,Email,Phone,Address,Website,IsActive");

                foreach (var s in suppliers)
                {
                    sb.AppendLine($"\"{s.Id}\",\"{EscapeCSV(s.Name)}\",\"{EscapeCSV(s.ContactPerson)}\",\"{s.Email}\",\"{s.Phone}\",\"{EscapeCSV(s.Address)}\",\"{s.Website}\",{s.IsActive}");
                }

                File.WriteAllText(filePath, sb.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception($"Error exporting suppliers: {ex.Message}");
            }
        }

        /// <summary>
        /// Imports suppliers from CSV
        /// </summary>
        public List<Supplier> ImportSuppliersFromCSV(string filePath)
        {
            var suppliers = new List<Supplier>();

            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("CSV file not found", filePath);

                var lines = File.ReadAllLines(filePath);

                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var fields = ParseCSVLine(line);
                    if (fields.Count >= 5)
                    {
                        var supplier = new Supplier
                        {
                            Name = fields[1],
                            ContactPerson = fields[2],
                            Email = fields[3],
                            Phone = fields[4],
                            Address = fields.Count > 5 ? fields[5] : "",
                            Website = fields.Count > 6 ? fields[6] : "",
                            IsActive = true
                        };

                        suppliers.Add(supplier);
                        _dataService.AddSupplier(supplier);
                    }
                }

                return suppliers;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error importing suppliers: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Escapes special characters for CSV
        /// </summary>
        private string EscapeCSV(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            return value.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", "");
        }

        /// <summary>
        /// Parses a CSV line handling quoted fields
        /// </summary>
        private List<string> ParseCSVLine(string line)
        {
            var fields = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            fields.Add(current.ToString());
            return fields;
        }

        /// <summary>
        /// Validates CSV file format
        /// </summary>
        public bool ValidateCSV(string filePath, string expectedHeader)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                var firstLine = File.ReadLines(filePath).FirstOrDefault();
                return firstLine != null && firstLine.Contains(expectedHeader);
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}