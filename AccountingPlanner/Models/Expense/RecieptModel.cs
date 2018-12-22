using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AccountingPlanner.Models.Expense
{
    public class RecieptModel
    {
        [Required(ErrorMessage = "Date is required")]
        [Display(Name = "Date")]
        public string dated { get; set; }

        [Display(Name = "Notes")]
        public string notes { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public string category { get; set; }

        [Required(ErrorMessage = "Payment account is required")]
        [Display(Name = "Payment Account")]
        public string payment_account { get; set; }

        [Required(ErrorMessage = "Sub Total is required")]
        [Display(Name = "Sub Total")]
        public string subtotal { get; set; }

        [Required(ErrorMessage = "Currency is required")]
        [Display(Name = "Currency")]
        public string currency { get; set; }

        [Required(ErrorMessage = "Total is required")]
        [Display(Name = "Total")]
        public string total { get; set; }

        [Required(ErrorMessage = "Image is required")]
        public IFormFile image { get; set; }
    }
}
