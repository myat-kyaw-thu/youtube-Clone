using System;
using System.Collections.Generic;

namespace GreenLifeOrganicStore.Models
{
    /// <summary>
    /// Customer user - inherits from User, can browse, order, and track
    /// </summary>
    public class Customer : User
    {
        public string LoyaltyTier { get; set; }     // Bronze, Silver, Gold
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        public List<string> OrderIds { get; set; }  // References to Order IDs
        public string PreferredCategory { get; set; }
        public DateTime DateOfBirth { get; set; }

        public Customer()
        {
            UserType = "Customer";
            LoyaltyTier = "Bronze";
            TotalSpent = 0;
            TotalOrders = 0;
            OrderIds = new List<string>();
            PreferredCategory = "";
        }

        public Customer(string username, string passwordHash, string email, string fullName, string phone, string address)
        {
            Id = Guid.NewGuid().ToString();
            Username = username;
            PasswordHash = passwordHash;
            Email = email;
            FullName = fullName;
            Phone = phone;
            Address = address;
            UserType = "Customer";
            LoyaltyTier = "Bronze";
            TotalSpent = 0;
            TotalOrders = 0;
            OrderIds = new List<string>();
            PreferredCategory = "";
            RegistrationDate = DateTime.Now;
            LastLogin = DateTime.Now;
            IsActive = true;
        }

        /// <summary>
        /// Updates loyalty tier based on total amount spent
        /// Bronze < $500, Silver $500-$2000, Gold > $2000
        /// </summary>
        public void UpdateLoyaltyTier()
        {
            if (TotalSpent >= 2000)
                LoyaltyTier = "Gold";
            else if (TotalSpent >= 500)
                LoyaltyTier = "Silver";
            else
                LoyaltyTier = "Bronze";
        }

        /// <summary>
        /// Returns loyalty discount percentage based on tier
        /// </summary>
        public decimal GetLoyaltyDiscount()
        {
            switch (LoyaltyTier)
            {
                case "Gold":   return 0.10m; // 10%
                case "Silver": return 0.05m; // 5%
                default:       return 0.00m; // 0%
            }
        }
    }
}
