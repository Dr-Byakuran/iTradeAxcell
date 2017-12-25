using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class ProductSelection
    {

        [Key]
        public int ProductID { get; set; }
        public string SKU { get; set; }
        public string Barcode { get; set; }
        public string ProductType { get; set; }
        public string ProductName { get; set; }
        public string ModelNo { get; set; }
        public Boolean IsBundle { get; set; }

        public string Unit { get; set; }
        public decimal CostPrice { get; set; }
        public string CostCode { get; set; }

        public decimal SellPrice { get; set; }

        public Boolean IsControlItem { get; set; }
        public double AvailableQty { get; set; }

    }
}