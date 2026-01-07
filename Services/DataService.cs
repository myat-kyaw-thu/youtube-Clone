using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using GreenLifeOrganicStore.Models;

namespace GreenLifeOrganicStore.Services
{
    /// <summary>
    /// Handles all JSON file operations - CRUD for all data types
    /// </summary>
    public class DataService
    {
        private readonly string _dataFolder;
        
        // File paths
        private string UsersFile => Path.Combine(_dataFolder, "users.json");
        private string ProductsFile => Path.Combine(_dataFolder, "products.json");
        private string OrdersFile => Path.Combine(_dataFolder, "orders.json");
        private string SuppliersFile => Path.Combine(_dataFolder, "suppliers.json");
        private string AnalyticsFile => Path.Combine(_dataFolder, "analytics.json");

        public DataService()
        {
            // Resolve the project-level Data folder so reads and writes
            // always use the same single source — no bin\Debug\Data split.
            // Walk up from the executable (bin\Debug\) two levels to reach the project root.
            string exeDir    = AppDomain.CurrentDomain.BaseDirectory; // …\bin\Debug\
            string projectDir = Path.GetFullPath(Path.Combine(exeDir, @"..\..\"));
            string candidate  = Path.Combine(projectDir, "Data");

            // If the project-level Data folder exists, use it.
            // Otherwise fall back to the exe-relative Data folder (e.g. when published).
            _dataFolder = Directory.Exists(candidate)
                ? candidate
                : Path.Combine(exeDir, "Data");

            EnsureDataFolderExists();
        }

        /// <summary>
        /// Creates Data folder if it doesn't exist
        /// </summary>
        private void EnsureDataFolderExists()
        {
            if (!Directory.Exists(_dataFolder))
            {
                Directory.CreateDirectory(_dataFolder);
            }
        }

        #region Generic JSON Operations

        /// <summary>
        /// Reads and deserializes a JSON file to a list of objects
        /// </summary>
        private List<T> LoadData<T>(string filePath) where T : class
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new List<T>();
                }

                string json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<T>();
                }

                var data = JsonConvert.DeserializeObject<dynamic>(json);
                if (data == null) return new List<T>();

                // Handle different root property names
                string propertyName = typeof(T).Name + "s";
                if (propertyName == "Orders") propertyName = "orders";
                else if (propertyName == "Products") propertyName = "products";
                else if (propertyName == "Users") propertyName = "users";
                else if (propertyName == "Suppliers") propertyName = "suppliers";
                else if (propertyName == "PurchaseAnalytics") propertyName = "customerAnalytics";

                return JsonConvert.DeserializeObject<List<T>>(data[propertyName].ToString()) 
                    ?? new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading {filePath}: {ex.Message}");
                return new List<T>();
            }
        }

        /// <summary>
        /// Serializes and saves a list of objects to a JSON file
        /// </summary>
        private void SaveData<T>(string filePath, List<T> data) where T : class
        {
            try
            {
                string propertyName = typeof(T).Name + "s";
                if (propertyName == "Orders") propertyName = "orders";
                else if (propertyName == "Products") propertyName = "products";
                else if (propertyName == "Users") propertyName = "users";
                else if (propertyName == "Suppliers") propertyName = "suppliers";
                else if (propertyName == "PurchaseAnalytics") propertyName = "customerAnalytics";

                var wrapper = new Dictionary<string, object>
                {
                    { propertyName, data }
                };

                string json = JsonConvert.SerializeObject(wrapper, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving {filePath}: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region User Operations

        public List<User> GetAllUsers()
        {
            return LoadData<User>(UsersFile);
        }

        public User GetUserById(string id)
        {
            var users = GetAllUsers();
            return users.Find(u => u.Id == id);
        }

        public User GetUserByUsername(string username)
        {
            var users = GetAllUsers();
            return users.Find(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public void AddUser(User user)
        {
            var users = GetAllUsers();
            users.Add(user);
            SaveData(UsersFile, users);
        }

        public void UpdateUser(User user)
        {
            var users = GetAllUsers();
            int index = users.FindIndex(u => u.Id == user.Id);
            if (index >= 0)
            {
                users[index] = user;
                SaveData(UsersFile, users);
            }
        }

        public void DeleteUser(string id)
        {
            var users = GetAllUsers();
            users.RemoveAll(u => u.Id == id);
            SaveData(UsersFile, users);
        }

        public List<Customer> GetAllCustomers()
        {
            var users = GetAllUsers();
            var analytics = GetAllAnalytics();
            var customers = new List<Customer>();
            
            foreach (var u in users)
            {
                Customer customer = null;
                
                if (u is Customer c)
                {
                    customer = c;
                }
                else if (u.UserType == "Customer")
                {
                    // Convert User to Customer
                    customer = new Customer
                    {
                        Id = u.Id,
                        Username = u.Username,
                        PasswordHash = u.PasswordHash,
                        Email = u.Email,
                        FullName = u.FullName,
                        Phone = u.Phone,
                        Address = u.Address,
                        UserType = u.UserType,
                        RegistrationDate = u.RegistrationDate,
                        LastLogin = u.LastLogin,
                        IsActive = u.IsActive
                    };
                }
                
                if (customer != null)
                {
                    // ALWAYS merge analytics data to get live TotalOrders and TotalSpent
                    var analytic = analytics.FirstOrDefault(a => a.CustomerId == customer.Id);
                    if (analytic != null)
                    {
                        customer.TotalOrders = analytic.TotalOrders;
                        customer.TotalSpent = analytic.TotalSpent;
                        customer.PreferredCategory = analytic.FavoriteCategory;
                        // Update loyalty tier based on actual spending
                        customer.UpdateLoyaltyTier();
                    }
                    else
                    {
                        // No analytics data - set defaults
                        customer.TotalOrders = 0;
                        customer.TotalSpent = 0;
                        customer.PreferredCategory = "";
                        customer.LoyaltyTier = "Bronze";
                    }
                    
                    customers.Add(customer);
                }
            }
            return customers;
        }

        public List<Admin> GetAllAdmins()
        {
            var users = GetAllUsers();
            var admins = new List<Admin>();
            foreach (var u in users)
            {
                if (u is Admin a)
                    admins.Add(a);
                else if (u.UserType == "Admin")
                {
                    admins.Add(new Admin
                    {
                        Id = u.Id,
                        Username = u.Username,
                        PasswordHash = u.PasswordHash,
                        Email = u.Email,
                        FullName = u.FullName,
                        Phone = u.Phone,
                        Address = u.Address,
                        UserType = u.UserType,
                        RegistrationDate = u.RegistrationDate,
                        LastLogin = u.LastLogin,
                        IsActive = u.IsActive
                    });
                }
            }
            return admins;
        }

        #endregion

        #region Product Operations

        public List<Product> GetAllProducts()
        {
            return LoadData<Product>(ProductsFile);
        }

        public Product GetProductById(string id)
        {
            var products = GetAllProducts();
            return products.Find(p => p.Id == id);
        }

        public void AddProduct(Product product)
        {
            var products = GetAllProducts();
            products.Add(product);
            SaveData(ProductsFile, products);
        }

        public void UpdateProduct(Product product)
        {
            var products = GetAllProducts();
            int index = products.FindIndex(p => p.Id == product.Id);
            if (index >= 0)
            {
                products[index] = product;
                SaveData(ProductsFile, products);
            }
        }

        public void DeleteProduct(string id)
        {
            var products = GetAllProducts();
            products.RemoveAll(p => p.Id == id);
            SaveData(ProductsFile, products);
        }

        public List<Product> GetActiveProducts()
        {
            return GetAllProducts().FindAll(p => p.IsActive);
        }

        public List<Product> GetProductsByCategory(string category)
        {
            return GetAllProducts().FindAll(p => 
                p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        public List<Product> GetLowStockProducts()
        {
            return GetAllProducts().FindAll(p => p.IsLowStock());
        }

        public List<Product> GetNearExpiryProducts(int daysThreshold = 30)
        {
            return GetAllProducts().FindAll(p => p.IsNearExpiry(daysThreshold));
        }

        #endregion

        #region Order Operations

        public List<Order> GetAllOrders()
        {
            return LoadData<Order>(OrdersFile);
        }

        public Order GetOrderById(string id)
        {
            var orders = GetAllOrders();
            return orders.Find(o => o.Id == id);
        }

        public void AddOrder(Order order)
        {
            var orders = GetAllOrders();
            orders.Add(order);
            SaveData(OrdersFile, orders);
        }

        public void UpdateOrder(Order order)
        {
            var orders = GetAllOrders();
            int index = orders.FindIndex(o => o.Id == order.Id);
            if (index >= 0)
            {
                orders[index] = order;
                SaveData(OrdersFile, orders);
            }
        }

        public void DeleteOrder(string id)
        {
            var orders = GetAllOrders();
            orders.RemoveAll(o => o.Id == id);
            SaveData(OrdersFile, orders);
        }

        public List<Order> GetOrdersByCustomerId(string customerId)
        {
            return GetAllOrders().FindAll(o => o.CustomerId == customerId);
        }

        public List<Order> GetOrdersByStatus(string status)
        {
            return GetAllOrders().FindAll(o => o.Status == status);
        }

        public List<Order> GetOrdersByDateRange(DateTime from, DateTime to)
        {
            return GetAllOrders().FindAll(o => o.OrderDate >= from && o.OrderDate <= to);
        }

        #endregion

        #region Supplier Operations

        public List<Supplier> GetAllSuppliers()
        {
            return LoadData<Supplier>(SuppliersFile);
        }

        public Supplier GetSupplierById(string id)
        {
            var suppliers = GetAllSuppliers();
            return suppliers.Find(s => s.Id == id);
        }

        public void AddSupplier(Supplier supplier)
        {
            var suppliers = GetAllSuppliers();
            suppliers.Add(supplier);
            SaveData(SuppliersFile, suppliers);
        }

        public void UpdateSupplier(Supplier supplier)
        {
            var suppliers = GetAllSuppliers();
            int index = suppliers.FindIndex(s => s.Id == supplier.Id);
            if (index >= 0)
            {
                suppliers[index] = supplier;
                SaveData(SuppliersFile, suppliers);
            }
        }

        public void DeleteSupplier(string id)
        {
            var suppliers = GetAllSuppliers();
            suppliers.RemoveAll(s => s.Id == id);
            SaveData(SuppliersFile, suppliers);
        }

        #endregion

        #region Analytics Operations

        public List<PurchaseAnalytics> GetAllAnalytics()
        {
            return LoadData<PurchaseAnalytics>(AnalyticsFile);
        }

        public PurchaseAnalytics GetAnalyticsByCustomerId(string customerId)
        {
            var analytics = GetAllAnalytics();
            return analytics.Find(a => a.CustomerId == customerId);
        }

        public void SaveAnalytics(PurchaseAnalytics analytics)
        {
            var allAnalytics = GetAllAnalytics();
            int index = allAnalytics.FindIndex(a => a.CustomerId == analytics.CustomerId);
            
            if (index >= 0)
                allAnalytics[index] = analytics;
            else
                allAnalytics.Add(analytics);

            SaveData(AnalyticsFile, allAnalytics);
        }

        public void UpdateCustomerAnalytics(string customerId, string customerName, decimal orderAmount, int productCount, string category)
        {
            var analytics = GetAnalyticsByCustomerId(customerId);
            
            if (analytics == null)
            {
                analytics = new PurchaseAnalytics(customerId, customerName);
            }

            analytics.AddPurchase(DateTime.Now, orderAmount, productCount, category);
            SaveAnalytics(analytics);
            
            // Sync the customer data in users.json with the updated analytics
            SyncCustomerFromAnalytics(customerId);
        }
        
        /// <summary>
        /// Syncs a customer's TotalOrders, TotalSpent, and LoyaltyTier from analytics.json to users.json
        /// </summary>
        public void SyncCustomerFromAnalytics(string customerId)
        {
            var users = GetAllUsers();
            var analytic = GetAnalyticsByCustomerId(customerId);
            
            if (analytic == null) return;
            
            int index = users.FindIndex(u => u.Id == customerId);
            if (index >= 0 && users[index].UserType == "Customer")
            {
                Customer customer;
                if (users[index] is Customer c)
                {
                    customer = c;
                }
                else
                {
                    // Convert to Customer if it's just a User
                    var u = users[index];
                    customer = new Customer
                    {
                        Id = u.Id,
                        Username = u.Username,
                        PasswordHash = u.PasswordHash,
                        Email = u.Email,
                        FullName = u.FullName,
                        Phone = u.Phone,
                        Address = u.Address,
                        UserType = u.UserType,
                        RegistrationDate = u.RegistrationDate,
                        LastLogin = u.LastLogin,
                        IsActive = u.IsActive
                    };
                }
                
                // Update from analytics
                customer.TotalOrders = analytic.TotalOrders;
                customer.TotalSpent = analytic.TotalSpent;
                customer.PreferredCategory = analytic.FavoriteCategory;
                customer.UpdateLoyaltyTier();
                
                users[index] = customer;
                SaveData(UsersFile, users);
            }
        }
        
        /// <summary>
        /// Syncs all customers' data from analytics.json to users.json
        /// </summary>
        public void SyncAllCustomersFromAnalytics()
        {
            var users = GetAllUsers();
            var analytics = GetAllAnalytics();
            bool changed = false;
            
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].UserType == "Customer")
                {
                    var analytic = analytics.FirstOrDefault(a => a.CustomerId == users[i].Id);
                    
                    Customer customer;
                    if (users[i] is Customer c)
                    {
                        customer = c;
                    }
                    else
                    {
                        // Convert to Customer if it's just a User
                        var u = users[i];
                        customer = new Customer
                        {
                            Id = u.Id,
                            Username = u.Username,
                            PasswordHash = u.PasswordHash,
                            Email = u.Email,
                            FullName = u.FullName,
                            Phone = u.Phone,
                            Address = u.Address,
                            UserType = u.UserType,
                            RegistrationDate = u.RegistrationDate,
                            LastLogin = u.LastLogin,
                            IsActive = u.IsActive
                        };
                    }
                    
                    if (analytic != null)
                    {
                        customer.TotalOrders = analytic.TotalOrders;
                        customer.TotalSpent = analytic.TotalSpent;
                        customer.PreferredCategory = analytic.FavoriteCategory;
                        customer.UpdateLoyaltyTier();
                    }
                    else
                    {
                        customer.TotalOrders = 0;
                        customer.TotalSpent = 0;
                        customer.PreferredCategory = "";
                        customer.LoyaltyTier = "Bronze";
                    }
                    
                    users[i] = customer;
                    changed = true;
                }
            }
            
            if (changed)
            {
                SaveData(UsersFile, users);
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Gets all unique categories from products
        /// </summary>
        public List<string> GetAllCategories()
        {
            var products = GetAllProducts();
            var categories = new List<string>();
            foreach (var p in products)
            {
                if (!categories.Contains(p.Category))
                    categories.Add(p.Category);
            }
            return categories;
        }

        /// <summary>
        /// Gets total sales amount for a date range
        /// </summary>
        public decimal GetTotalSales(DateTime from, DateTime to)
        {
            var orders = GetOrdersByDateRange(from, to);
            return orders
                .Where(o => o.Status != "Cancelled")
                .Sum(o => o.TotalAmount);
        }

        /// <summary>
        /// Gets order count by status
        /// </summary>
        public Dictionary<string, int> GetOrderStatusCounts()
        {
            var orders = GetAllOrders();
            var counts = new Dictionary<string, int>
            {
                { "Pending", 0 },
                { "Shipped", 0 },
                { "Delivered", 0 },
                { "Cancelled", 0 }
            };

            foreach (var order in orders)
            {
                if (counts.ContainsKey(order.Status))
                    counts[order.Status]++;
            }

            return counts;
        }

        #endregion
    }
}