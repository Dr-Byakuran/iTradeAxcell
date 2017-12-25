using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class StockTakeDet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Detail ID")]
        public int DetID { get; set; }

        [Display(Name = "Stock Take#")]
        public int SttID { get; set; }

        [Display(Name = "Location ID")]
        public int LocationID { get; set; }

        [Display(Name = "Product ID")]
        public int ProductID { get; set; }

        [Display(Name = "SKU")]
        [StringLength(60)]
        public string SKU { get; set; }

        [Display(Name = "Barcode")]
        [StringLength(48)]
        public string Barcode { get; set; }

        [Display(Name = "Product Name")]
        [StringLength(200)]
        public string ProductName { get; set; }

        [Display(Name = "In-Stock")]
        public double InStock { get; set; }

        [Display(Name = "Allocated")]
        public double Allocated { get; set; }

        [Display(Name = "On Hand")]
        public double OnHand { get; set; }

        [Display(Name = "Incoming")]
        public double OnOrder { get; set; }

        [Display(Name = "KIV")]
        public double OnKiv { get; set; }

        [Display(Name = "Stocktake Qty")]
        public double StockTakeQty { get; set; }

        [Display(Name = "Difference")]
        public double DifferentQty { get; set; }




    }
}