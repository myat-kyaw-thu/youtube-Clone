using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using GreenLifeOrganicStore.Models;

namespace GreenLifeOrganicStore.Services
{
    /// <summary>
    /// Handles authentication, password hashing, and user validation
    /// </summary>
    public class AuthService
    {
        private readonly DataService _dataService;

        public AuthService()
        {
            _dataService = new DataService();
        }

        #region Password Hashing (SHA-256 with Salt)

        /// <summary>
        /// Generates a random salt for password hashing
        /// </summary>
        public string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Hashes a password with salt using SHA-256
        /// </summary>
        public string HashPassword(string password, string salt)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty", nameof(password));
            if (string.IsNullOrEmpty(salt))
                throw new ArgumentException("Salt cannot be empty", nameof(salt));

            // Combine password and salt
            string combined = password + salt;
            
            // Hash using SHA-256
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// Creates a hashed password with automatic salt generation
        /// </summary>
        public string CreatePasswordHash(string password)
        {
            string salt = GenerateSalt();
            string hash = HashPassword(password, salt);
            return salt + ":" + hash; // Store as "salt:hash"
        }

        /// <summary>
        /// Verifies a password against a stored hash
        /// </summary>
        public bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                // Extract salt and hash from stored value
                string[] parts = storedHash.Split(':');
                if (parts.Length != 2)
                    return false;

                string salt = parts[0];
                string hash = parts[1];

                string computedHash = HashPassword(password, salt);
                return computedHash == hash;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region User Authentication

        /// <summary>
        /// Authenticates a user by username and password
        /// </summary>
        public User Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var user = _dataService.GetUserByUsername(username);
            if (user == null)
                return null;

            if (!user.IsActive)
                return null;

            if (!VerifyPassword(password, user.PasswordHash))
                return null;

            // Update last login time
            user.LastLogin = DateTime.Now;
            _dataService.UpdateUser(user);

            return user;
        }

        /// <summary>
        /// Registers a new customer
        /// </summary>
        public Customer RegisterCustomer(string username, string password, string email, 
            string fullName, string phone, string address)
        {
            // Check if username already exists
            if (_dataService.GetUserByUsername(username) != null)
                throw new Exception("Username already exists");

            // Create new customer
            var customer = new Customer
            {
                Username = username,
                PasswordHash = CreatePasswordHash(password),
                Email = email,
                FullName = fullName,
                Phone = phone,
                Address = address,
                UserType = "Customer",
                RegistrationDate = DateTime.Now,
                LastLogin = DateTime.Now,
                IsActive = true
            };

            _dataService.AddUser(customer);
            return customer;
        }

        /// <summary>
        /// Registers a new admin (for initial setup)
        /// </summary>
        public Admin RegisterAdmin(string username, string password, string email,
            string fullName, string phone, string address)
        {
            // Check if username already exists
            if (_dataService.GetUserByUsername(username) != null)
                throw new Exception("Username already exists");

            // Create new admin
            var admin = new Admin
            {
                Username = username,
                PasswordHash = CreatePasswordHash(password),
                Email = email,
                FullName = fullName,
                Phone = phone,
                Address = address,
                UserType = "Admin",
                RegistrationDate = DateTime.Now,
                LastLogin = DateTime.Now,
                IsActive = true
            };

            _dataService.AddUser(admin);
            return admin;
        }

        /// <summary>
        /// Changes user password
        /// </summary>
        public bool ChangePassword(string userId, string oldPassword, string newPassword)
        {
            var user = _dataService.GetUserById(userId);
            if (user == null)
                return false;

            if (!VerifyPassword(oldPassword, user.PasswordHash))
                return false;

            user.PasswordHash = CreatePasswordHash(newPassword);
            _dataService.UpdateUser(user);
            return true;
        }

        /// <summary>
        /// Resets password (admin function)
        /// </summary>
        public bool ResetPassword(string userId, string newPassword)
        {
            var user = _dataService.GetUserById(userId);
            if (user == null)
                return false;

            user.PasswordHash = CreatePasswordHash(newPassword);
            _dataService.UpdateUser(user);
            return true;
        }

        #endregion

        #region Input Validation

        /// <summary>
        /// Validates username format
        /// </summary>
        public bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;
            if (username.Length < 3 || username.Length > 50)
                return false;
            // Username can only contain letters, numbers, and underscores
            foreach (char c in username)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Validates password strength
        /// </summary>
        public bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;
            if (password.Length < 6)
                return false;
            return true;
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates phone number format
        /// </summary>
        public bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;
            // Remove common formatting characters
            string cleaned = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            return cleaned.Length >= 10 && cleaned.All(char.IsDigit);
        }

        #endregion

        #region Session Management

        private static User _currentUser = null;

        /// <summary>
        /// Gets the currently logged in user
        /// </summary>
        public User GetCurrentUser()
        {
            return _currentUser;
        }

        /// <summary>
        /// Sets the current user (after successful login)
        /// </summary>
        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        public void Logout()
        {
            _currentUser = null;
        }

        /// <summary>
        /// Checks if a user is currently logged in
        /// </summary>
        public bool IsLoggedIn()
        {
            return _currentUser != null;
        }

        /// <summary>
        /// Checks if current user is an admin
        /// </summary>
        public bool IsAdmin()
        {
            return _currentUser != null && _currentUser.UserType == "Admin";
        }

        /// <summary>
        /// Checks if current user is a customer
        /// </summary>
        public bool IsCustomer()
        {
            return _currentUser != null && _currentUser.UserType == "Customer";
        }

        #endregion
    }
}