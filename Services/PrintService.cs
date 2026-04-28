using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using GreenLifeOrganicStore.Models;

namespace GreenLifeOrganicStore.Services
{
    /// <summary>
    /// Handles printing receipts, invoices, and reports
    /// </summary>
    public class PrintService
    {
        private readonly DataService _dataService;

        public PrintService()
        {
            _dataService = new DataService();
        }

        #region Receipt Printing

        /// <summary>
        /// Prints a receipt for an order
        /// </summary>
        public void PrintReceipt(Order order)
        {
            var printDoc = new PrintDocument();
            printDoc.PrintPage += (s, e) => PrintReceiptPage(e, order);
            
            try
            {
                printDoc.Print();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error printing receipt: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates receipt content as string (for preview or file export)
        /// </summary>
        public string GenerateReceiptText(Order order)
        {
            var sb = new StringBuilder();
            
            // Header
            sb.AppendLine("================================");
            sb.AppendLine("     GREENLIFE ORGANIC STORE    ");
            sb.AppendLine("================================");
            sb.AppendLine();
            sb.AppendLine($"Receipt #: {order.Id.Substring(0, 8)}");
            sb.AppendLine($"Date: {order.OrderDate:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Customer: {order.CustomerName}");
            sb.AppendLine($"Status: {order.Status}");
            sb.AppendLine();
            sb.AppendLine("--------------------------------");
            sb.AppendLine("ITEMS:");
            sb.AppendLine("--------------------------------");

            foreach (var item in order.Items)
            {
                sb.AppendLine($"{item.ProductName}");
                sb.AppendLine($"  {item.Quantity} x ${item.UnitPrice:F2} = ${item.Subtotal:F2}");
            }

            sb.AppendLine("--------------------------------");
            sb.AppendLine($"TOTAL: ${order.TotalAmount:F2}");
            sb.AppendLine();
            sb.AppendLine("================================");
            sb.AppendLine("   Thank you for shopping!     ");
            sb.AppendLine("================================");

            return sb.ToString();
        }

        /// <summary>
        /// Prints receipt page event handler
        /// </summary>
        private void PrintReceiptPage(PrintPageEventArgs e, Order order)
        {
            var content = GenerateReceiptText(order);
            using (var font = new Font("Courier New", 10))
            {
                e.Graphics.DrawString(content, font, Brushes.Black, 10, 10);
            }
        }

        #endregion

        #region Invoice Printing

        /// <summary>
        /// Prints an invoice for an order
        /// </summary>
        public void PrintInvoice(Order order)
        {
            var printDoc = new PrintDocument();
            printDoc.PrintPage += (s, e) => PrintInvoicePage(e, order);
            
            try
            {
                printDoc.Print();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error printing invoice: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates invoice content as string
        /// </summary>
        public string GenerateInvoiceText(Order order)
        {
            var sb = new StringBuilder();
            
            // Invoice Header
            sb.AppendLine("================================");
            sb.AppendLine("        INVOICE                ");
            sb.AppendLine("================================");
            sb.AppendLine();
            sb.AppendLine("GreenLife Organic Store");
            sb.AppendLine("123 Organic Lane");
            sb.AppendLine("Green City, GC 12345");
            sb.AppendLine();
            sb.AppendLine($"Invoice #: INV-{order.Id.Substring(0, 8)}");
            sb.AppendLine($"Date: {order.OrderDate:yyyy-MM-dd}");
            sb.AppendLine($"Due Date: {order.OrderDate.AddDays(30):yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine("Bill To:");
            sb.AppendLine(order.CustomerName);
            sb.AppendLine(order.ShippingAddress);
            sb.AppendLine();
            sb.AppendLine("--------------------------------");
            sb.AppendLine("ITEMS:");
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"{"Item",-25} {"Qty",4} {"Price",10} {"Total",10}");
            sb.AppendLine("--------------------------------");

            foreach (var item in order.Items)
            {
                string name = item.ProductName.Length > 25 
                    ? item.ProductName.Substring(0, 22) + "..." 
                    : item.ProductName;
                sb.AppendLine($"{name,-25} {item.Quantity,4} ${item.UnitPrice,8:F2} ${item.Subtotal,8:F2}");
            }

            sb.AppendLine("--------------------------------");
            sb.AppendLine($"{"Subtotal:",-20} ${order.TotalAmount:F2}");
            sb.AppendLine($"{"Tax (0%):",-20} $0.00");
            sb.AppendLine($"{"TOTAL:",-20} ${order.TotalAmount:F2}");
            sb.AppendLine();
            sb.AppendLine("================================");
            sb.AppendLine("Payment Terms: Due in 30 days");
            sb.AppendLine("================================");

            return sb.ToString();
        }

        /// <summary>
        /// Prints invoice page
        /// </summary>
        private void PrintInvoicePage(PrintPageEventArgs e, Order order)
        {
            var content = GenerateInvoiceText(order);
            using (var font = new Font("Courier New", 10))
            {
                e.Graphics.DrawString(content, font, Brushes.Black, 10, 10);
            }
        }

        #endregion

        #region Report Printing

        /// <summary>
        /// Prints a sales report
        /// </summary>
        public void PrintSalesReport(SalesReport report)
        {
            var printDoc = new PrintDocument();
            printDoc.PrintPage += (s, e) => PrintSalesReportPage(e, report);
            
            try
            {
                printDoc.Print();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error printing report: {ex.Message}");
            }
        }

        /// <summary>
        /// Generates sales report text
        /// </summary>
        public string GenerateSalesReportText(SalesReport report)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("================================");
            sb.AppendLine("       SALES REPORT            ");
            sb.AppendLine("================================");
            sb.AppendLine();
            sb.AppendLine($"Period: {report.FromDate:yyyy-MM-dd} to {report.ToDate:yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine($"Total Orders: {report.TotalOrders}");
            sb.AppendLine($"Total Revenue: ${report.TotalRevenue:F2}");
            sb.AppendLine($"Average Order: ${report.AverageOrderValue:F2}");
            sb.AppendLine();
            sb.AppendLine("Orders by Status:");
            foreach (var kvp in report.OrdersByStatus)
            {
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
            }

            return sb.ToString();
        }

        private void PrintSalesReportPage(PrintPageEventArgs e, SalesReport report)
        {
            var content = GenerateSalesReportText(report);
            using (var font = new Font("Courier New", 10))
            {
                e.Graphics.DrawString(content, font, Brushes.Black, 10, 10);
            }
        }

        #endregion

        #region Print Preview

        /// <summary>
        /// Gets available printers
        /// </summary>
        public string[] GetAvailablePrinters()
        {
            return PrinterSettings.InstalledPrinters.Cast<string>().ToArray();
        }

        /// <summary>
        /// Shows print preview dialog (for Windows Forms)
        /// </summary>
        public void ShowPrintPreview(string content, string title = "Print Preview")
        {
            // This would be called from a Form to show preview
            // Implementation depends on the calling context
            throw new NotImplementedException("Call this from a Windows Forms context");
        }

        #endregion
    }
}