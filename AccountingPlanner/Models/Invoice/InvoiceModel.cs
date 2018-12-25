using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingPlanner.Models.Invoice
{
    public class InvoiceModel
    {
        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public string cutomer { get; set; }

        [Required(ErrorMessage = "Currency is required")]
        [Display(Name = "Currency")]
        public string currency { get; set; }

        [Required(ErrorMessage = "Invoice Number is required")]
        [Display(Name = "Invoice Number")]
        public string invoice_number { get; set; }

        [Required(ErrorMessage = "Due Date is required")]
        [Display(Name = "Due Date")]
        public string due_date { get; set; }

        [Required(ErrorMessage = "Invoice Date is required")]
        [Display(Name = "Invoice Date")]
        public string invoice_date { get; set; }

        [Required(ErrorMessage = "Subhead is required")]
        [Display(Name = "Subhead")]
        public string order_number { get; set; }

        [Required]
        public List<InvoiceDetailModel> invoiceDetailList { get; set; }
    }

    public class InvoiceDetailModel
    {
        [Required]
        public string tax { get; set; }
        [Required]
        public string product { get; set; }
        [Required]
        public string description { get; set; }
        [Required]
        public string quantity { get; set; }
        [Required]
        public string price { get; set; }
        [Required]
        public string amount { get; set; }
    }
}
