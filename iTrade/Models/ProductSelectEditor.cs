using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class ProductSelectEditor
    {
        public bool Selected { get; set; }
        [Key]
        public int ProductID { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public string ModelNo { get; set; }
        public string Unit { get; set; }
        public double Qty { get; set; }
        public decimal CostPrice { get; set; }
        public string CostCode { get; set; }
        public decimal RetailPrice { get; set; }
 
    }
}