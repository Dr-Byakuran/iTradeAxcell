using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class ReceivedItemDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID")]
        public int ReceivedItemDetailID { get; set; }

        [Display(Name = "Received Item ID")]
        public int ReceivedItemID { get; set; }

        [Display(Name = "Product No")]
        public int ItemID { get; set; }

        [Display(Name = "Product Code")]
        [StringLength(60)]
        public string ItemCode { get; set; }

        [Display(Name = "Product Type")]
        [StringLength(30)]
        public string ItemType { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        [StringLength(100)]
        public string ItemName { get; set; }

        [Display(Name = "Sell Type")]
        [StringLength(20)]
        public string SellType { get; set; }

        public double Qty { get; set; }

        [StringLength(15)]
        public string Unit { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Sell Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal DiscountedPrice { get; set; }

        public decimal Amount { get; set; }

        public decimal Gst { get; set; }

        public decimal Nett { get; set; }

        public Boolean IsBundle { get; set; }

        [Display(Name = "Sales Type")]
        [StringLength(30)]
        public string SalesType { get; set; }

        [Display(Name = "Ref No")]
        public int RefItemID { get; set; }

        public Boolean IsControlItem { get; set; }

        public double Position { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }
    }
}