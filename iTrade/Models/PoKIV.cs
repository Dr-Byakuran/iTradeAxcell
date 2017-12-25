using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;


namespace iTrade.Models
{
    public class PoKIV
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "KIV No")]
        public int KivID { get; set; }

        [Display(Name = "Sales Order#")]
        public int SorID { get; set; }

        [Display(Name = "Invoice No")]
        public int InvID { get; set; }

        [Display(Name = "Invoice#")]
        [StringLength(25)]
        public string InvNo { get; set; }

        [Display(Name = "InvDet ID")]
        public int InvDetID { get; set; }

        [Display(Name = "Invoice Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime InvDate { get; set; }

        [Required]
        public int CustNo { get; set; }

        [Required]
        [Display(Name = "Company")]
        [StringLength(60)]
        public string CustName { get; set; }

        [Display(Name = "Product ID")]
        public int ProductID { get; set; }

        [Display(Name = "SKU")]
        [StringLength(60)]
        public string SKU { get; set; }

        [Display(Name = "Product Name")]
        [StringLength(100)]
        public string ProductName { get; set; }

        [Display(Name = "Order Qty")]
        public double OrderQty { get; set; }

        [Display(Name = "Balance Qty")]
        public double BalanceQty { get; set; }

        [StringLength(15)]
        public string Unit { get; set; }

        //[Display(Name = "Deliver Qty")]
        //public double DeliverQty { get; set; }

        //[Display(Name = "KIV Balance")]
        //public double KivBalanceQty { get; set; }

        [Display(Name = "Sales Type")]
        [StringLength(30)]
        public string SalesType { get; set; }

        public double Position { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }

        [Display(Name = "Created By")]
        [StringLength(60)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Last Modified")]
        [StringLength(60)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<DateTime> ModifiedOn { get; set; }

    }
}