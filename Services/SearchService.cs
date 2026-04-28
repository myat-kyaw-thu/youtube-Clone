using System;
using System.Collections.Generic;
using System.Linq;
using GreenLifeOrganicStore.Models;

namespace GreenLifeOrganicStore.Services
{
    /// <summary>
    /// Handles search operations - Linear and Binary search algorithms
    /// </summary>
    public class SearchService
    {
        private readonly DataService _dataService;

        public SearchService()
        {
            _dataService = new DataService();
        }

        #region Linear Search (O(n) - Required)

        /// <summary>
        /// Linear search by product name - O(n)
        /// </summary>
        public List<Product> LinearSearchByName(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _dataService.GetAllProducts();

            var allProducts = _dataService.GetAllProducts();
            var results = new List<Product>();
            searchTerm = searchTerm.ToLower();

            foreach (var product in allProducts)
            {
                if (product.Name.ToLower().Contains(searchTerm))
                {
                    results.Add(product);
                }
            }

            return results;
        }

        /// <summary>
        /// Linear search by category - O(n)
        /// </summary>
        public List<Product> LinearSearchByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return _dataService.GetAllProducts();

            var allProducts = _dataService.GetAllProducts();
            var results = new List<Product>();

            foreach (var product in allProducts)
            {
                if (product.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(product);
                }
            }

            return results;
        }

        /// <summary>
        /// Linear search by price range - O(n)
        /// </summary>
        public List<Product> LinearSearchByPriceRange(decimal minPrice, decimal maxPrice)
        {
            var allProducts = _dataService.GetAllProducts();
            var results = new List<Product>();

            foreach (var product in allProducts)
            {
                if (product.Price >= minPrice && product.Price <= maxPrice)
                {
                    results.Add(product);
                }
            }

            return results;
        }

        /// <summary>
        /// Linear search combining multiple criteria - O(n)
        /// </summary>
        public List<Product> AdvancedLinearSearch(string name = null, string category = null, 
            decimal? minPrice = null, decimal? maxPrice = null, decimal? minRating = null)
        {
            var allProducts = _dataService.GetAllProducts();
            var results = new List<Product>();

            foreach (var product in allProducts)
            {
                bool matches = true;

                // Name filter
                if (!string.IsNullOrWhiteSpace(name))
                {
                    matches = product.Name.ToLower().Contains(name.ToLower());
                }

                // Category filter
                if (matches && !string.IsNullOrWhiteSpace(category))
                {
                    matches = product.Category.Equals(category, StringComparison.OrdinalIgnoreCase);
                }

                // Price range filter
                if (matches && minPrice.HasValue)
                {
                    matches = product.Price >= minPrice.Value;
                }

                if (matches && maxPrice.HasValue)
                {
                    matches = product.Price <= maxPrice.Value;
                }

                // Rating filter
                if (matches && minRating.HasValue)
                {
                    matches = product.Rating >= minRating.Value;
                }

                if (matches)
                {
                    results.Add(product);
                }
            }

            return results;
        }

        #endregion

        #region Binary Search (O(log n) - Extra Feature)

        /// <summary>
        /// Sorts products by name for binary search
        /// </summary>
        private List<Product> GetProductsSortedByName()
        {
            var products = _dataService.GetAllProducts();
            return products.OrderBy(p => p.Name).ToList();
        }

        /// <summary>
        /// Sorts products by price for binary search
        /// </summary>
        private List<Product> GetProductsSortedByPrice()
        {
            var products = _dataService.GetAllProducts();
            return products.OrderBy(p => p.Price).ToList();
        }

        /// <summary>
        /// Sorts products by rating for binary search
        /// </summary>
        private List<Product> GetProductsSortedByRating()
        {
            var products = _dataService.GetAllProducts();
            return products.OrderByDescending(p => p.Rating).ToList();
        }

        /// <summary>
        /// Binary search by exact name match - O(log n)
        /// </summary>
        public Product BinarySearchByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var sortedProducts = GetProductsSortedByName();
            int left = 0;
            int right = sortedProducts.Count - 1;
            name = name.ToLower();

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                string midName = sortedProducts[mid].Name.ToLower();
                int comparison = midName.CompareTo(name);

                if (comparison == 0)
                {
                    return sortedProducts[mid];
                }
                else if (comparison < 0)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            return null; // Not found
        }

        /// <summary>
        /// Binary search for products in a price range - O(log n) + O(k)
        /// </summary>
        public List<Product> BinarySearchByPriceRange(decimal minPrice, decimal maxPrice)
        {
            var sortedProducts = GetProductsSortedByPrice();
            var results = new List<Product>();

            // Find first product >= minPrice
            int left = 0;
            int right = sortedProducts.Count - 1;
            int startIndex = -1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (sortedProducts[mid].Price >= minPrice)
                {
                    startIndex = mid;
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }

            if (startIndex == -1)
                return results;

            // Collect all products within price range
            for (int i = startIndex; i < sortedProducts.Count; i++)
            {
                if (sortedProducts[i].Price <= maxPrice)
                {
                    results.Add(sortedProducts[i]);
                }
                else
                {
                    break;
                }
            }

            return results;
        }

        /// <summary>
        /// Binary search for products by minimum rating - O(log n) + O(k)
        /// </summary>
        public List<Product> BinarySearchByMinRating(decimal minRating)
        {
            var sortedProducts = GetProductsSortedByRating();
            var results = new List<Product>();

            foreach (var product in sortedProducts)
            {
                if (product.Rating >= minRating)
                {
                    results.Add(product);
                }
                else
                {
                    break; // Since sorted by rating descending
                }
            }

            return results;
        }

        /// <summary>
        /// Binary search with custom comparison - O(log n)
        /// </summary>
        public Product BinarySearchCustom(List<Product> products, string searchTerm, 
            Func<Product, string, int> comparisonFunc)
        {
            if (products == null || products.Count == 0)
                return null;

            int left = 0;
            int right = products.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                int comparison = comparisonFunc(products[mid], searchTerm);

                if (comparison == 0)
                {
                    return products[mid];
                }
                else if (comparison < 0)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            return null;
        }

        #endregion

        #region Search Algorithm Comparison

        /// <summary>
        /// Demonstrates the difference between linear and binary search
        /// Returns results from both methods for comparison
        /// </summary>
        public (List<Product> linearResults, Product binaryResult) CompareSearchMethods(string name)
        {
            // Linear search - returns all matches
            var linearResults = LinearSearchByName(name);

            // Binary search - returns exact match only
            var binaryResult = BinarySearchByName(name);

            return (linearResults, binaryResult);
        }

        /// <summary>
        /// Gets search statistics for a term
        /// </summary>
        public string GetSearchStats(string searchTerm, string searchType)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            
            switch (searchType.ToLower())
            {
                case "name":
                    LinearSearchByName(searchTerm);
                    break;
                case "category":
                    LinearSearchByCategory(searchTerm);
                    break;
                case "price":
                    LinearSearchByPriceRange(0, 1000);
                    break;
            }

            sw.Stop();
            return $"Search completed in {sw.ElapsedTicks} ticks ({sw.ElapsedMilliseconds}ms)";
        }

        #endregion
    }
}