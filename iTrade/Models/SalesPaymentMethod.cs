using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class SalesPaymentMethod
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID")]
        public int SalesPaymentMethodID { get; set; }

        [Display(Name = "Sales Order#")]
        public int SorID { get; set; }

        [Display(Name = "Invoice#")]
        public int InvID { get; set; }

        [Display(Name = "Payment Receipt#")]
        public int PrID { get; set; }

        [Display(Name = "Type")]
        [StringLength(15)]
        public string InvType { get; set; }

        [Display(Name = "Payment On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Recorded From")]
        public string RecordedFrom { get; set; }

        [Display(Name = "PaymentMethod")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Cheque Number")]
        public string ChequeNumber { get; set; }

        [Display(Name = "FullPayment?")]
        public Boolean IsFullPayment { get; set; }

        [Display(Name = "Created By")]
        [StringLength(30)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }
    }
}