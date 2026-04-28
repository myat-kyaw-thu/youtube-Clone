using System;

namespace GreenLifeOrganicStore.Models
{
    /// <summary>
    /// Supplier entity - represents a product supplier/vendor
    /// </summary>
    public class Supplier
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateAdded { get; set; }
        public string Notes { get; set; }

        public Supplier()
        {
            Id = Guid.NewGuid().ToString();
            DateAdded = DateTime.Now;
            IsActive = true;
            Notes = "";
            Website = "";
        }

        public Supplier(string name, string contactPerson, string email, string phone, string address)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            ContactPerson = contactPerson;
            Email = email;
            Phone = phone;
            Address = address;
            Website = "";
            IsActive = true;
            DateAdded = DateTime.Now;
            Notes = "";
        }

        /// <summary>
        /// Validates required supplier fields
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Name)
                && !string.IsNullOrWhiteSpace(ContactPerson)
                && !string.IsNullOrWhiteSpace(Email)
                && !string.IsNullOrWhiteSpace(Phone);
        }

        public override string ToString()
        {
            return $"{Name} - Contact: {ContactPerson} ({Phone})";
        }
    }
}
