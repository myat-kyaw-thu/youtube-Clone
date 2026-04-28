using System;

namespace GreenLifeOrganicStore.Models
{
    /// <summary>
    /// Admin user - inherits from User, has full system access
    /// </summary>
    public class Admin : User
    {
        public string Department { get; set; }
        public DateTime LastReportGenerated { get; set; }

        public Admin()
        {
            UserType = "Admin";
            Department = "Management";
            LastReportGenerated = DateTime.Now;
        }

        public Admin(string username, string passwordHash, string email, string fullName, string phone, string address)
        {
            Id = Guid.NewGuid().ToString();
            Username = username;
            PasswordHash = passwordHash;
            Email = email;
            FullName = fullName;
            Phone = phone;
            Address = address;
            UserType = "Admin";
            Department = "Management";
            RegistrationDate = DateTime.Now;
            LastLogin = DateTime.Now;
            IsActive = true;
            LastReportGenerated = DateTime.Now;
        }

        /// <summary>
        /// Check if admin has permission to perform an action
        /// Admins always have full access
        /// </summary>
        public bool HasPermission(string action)
        {
            return IsActive;
        }
    }
}
