using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class PoINVDET
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "No")]
        public int DetID { get; set; }

        [Display(Name = "Quotation#")]
        [StringLength(25)]
        public string QuoNo { get; set; }

        [Display(Name = "Sales Order#")]
        public int SorID { get; set; }

        [Display(Name = "Sales Order#")]
        [StringLength(25)]
        public string SorNo { get; set; }

        [Display(Name = "Invoice No")]
        public int InvID { get; set; }

        [Display(Name = "Invoice#")]
        [StringLength(25)]
        public string InvNo { get; set; }

        [Display(Name = "Exchange ID")]
        public int EorID { get; set; }

        [Display(Name = "Exchange#")]
        [StringLength(25)]
        public string EorNo { get; set; }

        [Display(Name = "CreditNote ID")]
        public int CnID { get; set; }

        [Display(Name = "Credit Note#")]
        [StringLength(25)]
        public string CnNo { get; set; }

        [Display(Name = "Type")]
        [StringLength(15)]
        public string InvType { get; set; }

        [Display(Name = "Product No")]
        public int ItemID { get; set; }

        [Display(Name = "Product Code")]
        [StringLength(60)]
        public string ItemCode { get; set; }

        [Display(Name = "Product Type")]
        [StringLength(45)]
        public string ItemType { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        [StringLength(200)]
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

        [Display(Name = "Pre Discount Amount")]
        public decimal PreDiscAmount { get; set; }

        public decimal Discount { get; set; }

        public decimal Amount { get; set; }

        public decimal Gst { get; set; }

        public decimal Nett { get; set; }

        public Boolean IsBundle { get; set; }

        [Display(Name = "Sales Type")]
        [StringLength(30)]
        public string SalesType { get; set; }

        [Display(Name = "Ref No")]
        public int RefItemID { get; set; }

        [Display(Name = "INV Reference")]
        [StringLength(25)]
        public string InvRef { get; set; }

        public Boolean IsControlItem { get; set; }

        [Display(Name = "Location ID")]
        public int LocationID { get; set; }

        [Display(Name = "Location")]
        [StringLength(60)]
        public string LocationName { get; set; }

        public double Position { get; set; }

        [StringLength(200)]
        public string Remark { get; set; }


    }
}