using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class SalesOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Order ID")]
        public int SorID { get; set; }

        [Display(Name = "Sales Order#")]
        [StringLength(25)]
        public string SorNo { get; set; }

        [Display(Name = "Quotation ID")]
        public int QuoID { get; set; }

        [Display(Name = "Quotation#")]
        [StringLength(25)]
        public string QuoNo { get; set; }

        [Display(Name = "Delivery#")]
        [StringLength(25)]
        public string DorNo { get; set; }

        [Display(Name = "Invoice ID")]
        public int InvID { get; set; }

        [Display(Name = "Invoice#")]
        [StringLength(25)]
        public string InvNo { get; set; }

        [Display(Name = "Type")]
        [StringLength(15)]
        public string InvType { get; set; }

        [Display(Name = "Invoice Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime InvDate { get; set; }

        [Display(Name = "P/O Reference")]
        [StringLength(25)]
        public string PoNo { get; set; }

        [Display(Name = "INV Reference")]
        [StringLength(25)]
        public string InvRef { get; set; }

        [Required]
        public int CustNo { get; set; }

        [Display(Name = "Account Type")]
        [StringLength(20)]
        public string AccType { get; set; }

        [Required]
        [Display(Name = "Company")]
        [StringLength(100, ErrorMessage = "Company name can not be empty.", MinimumLength = 1)]
        public string CustName { get; set; }

        [StringLength(100)]
        public string CustName2 { get; set; }

        [Display(Name = "Address")]
        [StringLength(100)]
        public string Addr1 { get; set; }

        [Display(Name = " ")]
        [StringLength(100)]
        public string Addr2 { get; set; }

        [Display(Name = " ")]
        [StringLength(100)]
        public string Addr3 { get; set; }

        [Display(Name = " ")]
        [StringLength(100)]
        public string Addr4 { get; set; }

        [Display(Name = "Attn")]
        [StringLength(60)]
        public string Attn { get; set; }

        [Display(Name = "Phone")]
        [StringLength(60)]
        public string PhoneNo { get; set; }

        [Display(Name = "Mobile")]
        [StringLength(30)]
        public string MobileNo { get; set; }

        [Display(Name = "Fax")]
        [StringLength(30)]
        public string FaxNo { get; set; }

        [Display(Name = "Email")]
        [StringLength(60)]
        public string PrimaryEmail { get; set; }

        [Display(Name = "Delivery Address")]
        [StringLength(150)]
        [DataType(DataType.MultilineText)]
        public string DeliveryAddress { get; set; }

        [Display(Name = "Delivery Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> DeliveryDate { get; set; }

        [Display(Name = "Time")]
        [StringLength(20)]
        public string DeliveryTime { get; set; }

        [Display(Name = "Pre Discount Amount")]
        public decimal PreDiscAmount { get; set; }

        public decimal Discount { get; set; }

        public decimal Amount { get; set; }

        public decimal Gst { get; set; }

        [Display(Name = "Net Total")]
        public decimal Nett { get; set; }

        [Display(Name = "Paid Amount")]
        public decimal PaidAmount { get; set; }

        public decimal Deposit { get; set; }
        [Display(Name = "Deposit Percent")]
        public decimal DepositPct { get; set; }

        [Display(Name = "Full Paid?")]
        public Boolean IsPaid { get; set; }

        [Display(Name = "Paid On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> PaidDate { get; set; }

        [Display(Name = "Terms")]
        [StringLength(100)]
        public string PaymentTerms { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(30)]
        public string PaymentStatus { get; set; }

        [Display(Name = "Location ID")]
        public int LocationID { get; set; }

        [Display(Name = "Location")]
        [StringLength(60)]
        public string LocationName { get; set; }

        [StringLength(200)]
        [DataType(DataType.MultilineText)]
        public string Remark { get; set; }

        public int PersonID { get; set; }

        [Display(Name = "Sales Person")]
        [StringLength(60)]
        public string PersonName { get; set; }

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