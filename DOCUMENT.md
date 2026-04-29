# GreenLife Organic Store Management System
## Project Documentation & Architecture

---

## 📋 Project Overview
**Course:** CS6004ES Individual Coursework  
**Assignment:** Desktop Management System for GreenLife Organic Store  
**Technology:** C# Windows Forms Application (.NET Framework)  
**Data Storage:** JSON Files  
**Architecture:** Object-Oriented Programming with Classes  

---

## 🎯 Core Requirements Met

### Admin Features
- ✅ Secure Login System
- ✅ Manage Product Details (CRUD Operations)
- ✅ Manage Customer Details (View/Update)
- ✅ Manage Orders (View All, Update Status)
- ✅ Generate Reports (Sales, Stock, Customer History)
- ✅ Dashboard (Overview of Sales, Stock, Orders)

### Customer Features
- ✅ Register/Login System
- ✅ Search Products (Name, Category, Price Range)
- ✅ Place Orders (Cart System)
- ✅ Track Orders (Status Monitoring)
- ✅ Profile Management (Update Details)

### Technical Requirements
- ✅ C# Windows Forms Application
- ✅ JSON File Data Storage
- ✅ Object-Oriented Design with Classes
- ✅ Input Validation & Exception Handling
- ✅ Search Algorithms (Linear + Binary Search)

---

## 🚀 Extra Features Added

### Data Enhancements
- 📸 **Product Images/Photos** - Visual product catalog
- 📊 **Customer Purchase History Analytics** - Detailed buying patterns
- 🏢 **Supplier Contact Information** - Complete supplier management
- 📅 **Product Expiry Dates** - Inventory freshness tracking

### Advanced Functionality
- 🔍 **Advanced Filtering** - Price range, category, rating filters
- 📁 **Bulk Product Import/Export** - CSV file operations
- 🔐 **Data Encryption for Passwords** - Enhanced security
- 🖨️ **Print Receipts/Invoices** - Professional documentation
- ⌨️ **Keyboard Shortcuts** - Improved user experience

---

## 🏗️ Project Structure

```
GreenLifeStore/
├── Program.cs                          # Application entry point
├── Forms/
│   ├── LoginForm.cs                    # User authentication
│   ├── MainForm.cs                     # Main application window
│   ├── AdminDashboard.cs               # Admin overview panel
│   ├── CustomerDashboard.cs            # Customer main interface
│   ├── ProductManagementForm.cs        # Admin product CRUD
│   ├── OrderManagementForm.cs          # Admin order management
│   ├── CustomerRegistrationForm.cs     # New user signup
│   ├── ProductSearchForm.cs            # Customer product search
│   ├── ShoppingCartForm.cs             # Customer cart management
│   ├── OrderTrackingForm.cs            # Customer order status
│   ├── ReportsForm.cs                  # Admin report generation
│   └── ProfileManagementForm.cs        # User profile updates
├── Models/
│   ├── User.cs                         # Base user class
│   ├── Admin.cs                        # Admin user (inherits User)
│   ├── Customer.cs                     # Customer user (inherits User)
│   ├── Product.cs                      # Product entity
│   ├── Order.cs                        # Order entity
│   ├── OrderItem.cs                    # Individual order items
│   ├── Supplier.cs                     # Supplier information
│   └── PurchaseAnalytics.cs            # Customer analytics data
├── Services/
│   ├── DataService.cs                  # JSON file operations
│   ├── AuthService.cs                  # Authentication & encryption
│   ├── SearchService.cs                # Linear & binary search algorithms
│   ├── ReportService.cs                # Report generation logic
│   ├── CSVService.cs                   # Import/export functionality
│   └── PrintService.cs                 # Receipt/invoice printing
├── Data/
│   ├── users.json                      # User accounts (encrypted passwords)
│   ├── products.json                   # Product catalog
│   ├── orders.json                     # Order history
│   ├── suppliers.json                  # Supplier information
│   └── analytics.json                  # Purchase analytics data
└── Resources/
    ├── Images/                         # Product images
    └── Templates/                      # Print templates
```

---

## 🗄️ Database Schema (JSON Structure)

### users.json
```json
{
  "users": [
    {
      "id": "string",
      "username": "string",
      "passwordHash": "string (encrypted)",
      "email": "string",
      "fullName": "string",
      "phone": "string",
      "address": "string",
      "userType": "Admin|Customer",
      "registrationDate": "datetime",
      "lastLogin": "datetime",
      "isActive": "boolean"
    }
  ]
}
```

### products.json
```json
{
  "products": [
    {
      "id": "string",
      "name": "string",
      "category": "string",
      "price": "decimal",
      "stock": "integer",
      "description": "string",
      "supplierId": "string",
      "rating": "decimal",
      "imageUrl": "string",
      "expiryDate": "datetime",
      "dateAdded": "datetime",
      "isActive": "boolean"
    }
  ]
}
```

### orders.json
```json
{
  "orders": [
    {
      "id": "string",
      "customerId": "string",
      "orderDate": "datetime",
      "status": "Pending|Shipped|Delivered",
      "totalAmount": "decimal",
      "shippingAddress": "string",
      "items": [
        {
          "productId": "string",
          "quantity": "integer",
          "unitPrice": "decimal",
          "subtotal": "decimal"
        }
      ]
    }
  ]
}
```

### suppliers.json
```json
{
  "suppliers": [
    {
      "id": "string",
      "name": "string",
      "contactPerson": "string",
      "email": "string",
      "phone": "string",
      "address": "string",
      "website": "string",
      "isActive": "boolean"
    }
  ]
}
```

### analytics.json
```json
{
  "customerAnalytics": [
    {
      "customerId": "string",
      "totalOrders": "integer",
      "totalSpent": "decimal",
      "averageOrderValue": "decimal",
      "favoriteCategory": "string",
      "lastPurchaseDate": "datetime",
      "purchaseHistory": [
        {
          "date": "datetime",
          "amount": "decimal",
          "productCount": "integer"
        }
      ]
    }
  ]
}
```

---

## 🔍 Search Algorithms Implementation

### Linear Search (Required)
- **Use Case:** General product search by name/category
- **Complexity:** O(n)
- **Implementation:** Manual foreach loops through product list

### Binary Search (Extra Feature)
- **Use Case:** Efficient search on sorted product lists
- **Complexity:** O(log n)
- **Implementation:** Manual binary search algorithm on pre-sorted data
- **Sorting Options:** By name, price, rating, expiry date

---

## 🎨 UI Architecture

### Design Pattern: Flat Sidebar + Center Panel
```
┌─────────────────────────────────────────┐
│ [Logo] GreenLife Store            [User]│
├─────────────┬───────────────────────────┤
│ Dashboard   │                           │
│ Products    │                           │
│ Orders      │     Center Panel          │
│ Reports     │   (Dynamic Content)       │
│ Customers   │                           │
│ Profile     │                           │
│ Logout      │                           │
└─────────────┴───────────────────────────┘
```

### Form Hierarchy
- **LoginForm** → **MainForm** (with UserControls for different sections)
- **Modal Dialogs** for Add/Edit operations
- **Print Preview** for receipts/invoices

---

## ⌨️ Keyboard Shortcuts
- `Ctrl+N` - New Product/Order
- `Ctrl+S` - Save Changes
- `Ctrl+F` - Search Products
- `Ctrl+P` - Print Receipt
- `F5` - Refresh Data
- `Escape` - Close Modal/Cancel
- `Enter` - Confirm Action

---

## 🔐 Security Features
- **Password Encryption** using SHA-256 hashing
- **Input Validation** for all user inputs
- **Exception Handling** for file operations
- **Session Management** for user authentication

---

## 📊 Reporting Features
1. **Sales Report** - Revenue by date range
2. **Stock Report** - Current inventory levels
3. **Customer Order History** - Individual customer analytics
4. **Low Stock Alerts** - Products below threshold
5. **Expiry Date Warnings** - Products nearing expiration

---

## 🎯 Marking Scheme Alignment

| Feature | Implementation | Expected Score |
|---------|---------------|----------------|
| UI Design | Flat sidebar + center panel | 4/5 |
| Login System | JSON auth with encryption | 4/5 |
| Product Management | Full CRUD with images | 4/5 |
| Search Algorithms | Linear + Binary search | 5/5 |
| Extra Features | 9 additional features | 5/5 |
| Code Quality | Clean, commented, validated | 4/5 |

**Estimated Total: 88-92% (A- to A)**

---

## 📝 Development Phases

### Phase 1: Core Infrastructure
- Set up project structure
- Create base classes and JSON services
- Implement authentication system

### Phase 2: Basic Functionality
- Product CRUD operations
- Order management system
- Customer registration/login

### Phase 3: Advanced Features
- Search algorithms (linear + binary)
- Advanced filtering system
- CSV import/export

### Phase 4: Extra Features
- Product images integration
- Purchase analytics
- Print functionality
- Keyboard shortcuts

### Phase 5: Polish & Testing
- Exception handling
- Input validation
- UI improvements
- Documentation

---

## 🔧 Technical Implementation Notes (Updated 2026)

### JSON Serialization Library Choice
**Decision: Newtonsoft.Json for .NET Framework 4.7.2**
- System.Text.Json requires .NET Core 3.0+ or .NET Framework 4.6.2+ with NuGet package
- For .NET Framework 4.7.2, Newtonsoft.Json provides better compatibility and feature completeness
- Supports all required features: custom converters, flexible serialization, error handling
- Well-established with extensive documentation and community support

### Windows Forms UI/UX Architecture (Researched 2026)

#### Design Pattern: MVP (Model-View-Presenter)
- **Model**: Data entities (User, Product, Order, etc.)
- **View**: Forms and UserControls (UI only, no business logic)
- **Presenter**: Business logic, mediates between Model and View

**Why MVP for WinForms:**
- Separation of concerns - UI independent from business logic
- Testable - Presenter can be unit tested
- Maintainable - Changes in UI don't affect business logic
- Follows same principles as MVC/WPF MVVM but simpler for WinForms

#### UI Best Practices
1. **Data Binding**: Use BindingSource for connecting data to controls
2. **Async/Await**: Keep UI responsive - use for file I/O, data operations
3. **UserControls**: Group logical UI sections into reusable controls
4. **Layout**: Use TableLayoutPanel for responsive designs
5. **Error Handling**: Try-catch with user-friendly error messages
6. **Validation**: Validate input before processing

#### Layout Pattern: Flat Sidebar + Center Panel
```
┌─────────────────────────────────────────┐
│ [Logo] GreenLife Store            [User]│
├─────────────┬───────────────────────────┤
│ Dashboard   │                           │
│ Products    │     Center Panel          │
│ Orders      │   (Dynamic Content)       │
│ Reports     │                           │
│ Customers   │                           │
│ Profile     │                           │
│ Logout      │                           │
└─────────────┴───────────────────────────┘
```

#### Control Usage
- **Panel**: Main container for sidebar and content areas
- **DataGridView**: Display lists (products, orders, customers)
- **TextBox/ComboBox**: Input fields with validation
- **Button**: Actions with keyboard shortcuts
- **Label**: Information display
- **MessageBox**: Confirmations and alerts

### Security Implementation
- **Password Hashing**: SHA-256 with salt for secure password storage
- **Input Sanitization**: Parameterized queries equivalent for JSON operations
- **File Access**: Controlled file system access with proper exception handling
- **Session Management**: Simple session tracking without sensitive data exposure

---

*This document serves as the complete architectural blueprint for the GreenLife Organic Store Management System, ensuring all requirements and extra features are properly planned and implemented.*