using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class CreditNoteDet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "No")]
        public int DetID { get; set; }

        [Display(Name = "CN ID")]
        public int CnID { get; set; }

        [Display(Name = "Credit Note#")]
        [StringLength(25)]
        public string CnNo { get; set; }

        [Display(Name = "Invoice ID")]
        public int InvID { get; set; }

        [Display(Name = "Invoice#")]
        [StringLength(25)]
        public string InvNo { get; set; }

        [Display(Name = "Inv Type")]
        [StringLength(15)]
        public string InvType { get; set; }

        [Display(Name = "Type")]
        [StringLength(25)]
        public string DocType { get; set; }

        [Display(Name = "Invoice Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime InvDate { get; set; }

        [Required]
        public int CustNo { get; set; }

        [Required]
        [Display(Name = "Company")]
        [StringLength(60, ErrorMessage = "Company name can not be empty.", MinimumLength = 1)]
        public string CustName { get; set; }

        [StringLength(60)]
        public string CustName2 { get; set; }

        public decimal Amount { get; set; }

        public decimal Gst { get; set; }

        [Display(Name = "Net Total")]
        public decimal Nett { get; set; }

        [Display(Name = "Paid Amount")]
        public decimal PaidAmount { get; set; }

        [Display(Name = "Knock-Off Amount")]
        public decimal KnockOffAmount { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(100)]
        [DataType(DataType.MultilineText)]
        public string Remark { get; set; }

        [Display(Name = "Ref No")]
        public int RefItemID { get; set; }

        public double Position { get; set; }

    }
}