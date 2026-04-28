using System;

namespace GreenLifeOrganicStore.Models
{
    /// <summary>
    /// Base user class - shared properties for Admin and Customer
    /// </summary>
    public class User
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string UserType { get; set; } // "Admin" or "Customer"
        public DateTime RegistrationDate { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsActive { get; set; }

        public User()
        {
            Id = Guid.NewGuid().ToString();
            RegistrationDate = DateTime.Now;
            LastLogin = DateTime.Now;
            IsActive = true;
        }

        /// <summary>
        /// Returns display-friendly string for the user
        /// </summary>
        public override string ToString()
        {
            return $"{FullName} ({Username}) - {UserType}";
        }

        /// <summary>
        /// Validates required fields are not empty
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Username)
                && !string.IsNullOrWhiteSpace(Email)
                && !string.IsNullOrWhiteSpace(FullName);
        }
    }
}
