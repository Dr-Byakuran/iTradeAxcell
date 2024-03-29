﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class LoanManagement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "ID")]
        public int LoanManagementID { get; set; }

        [Display(Name = "Quotation#")]
        public int QuoID { get; set; }

        [Display(Name = "Invoice No.")]
        public int InvID { get; set; }

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

        [Required]
        [Display(Name = "Company")]
        [StringLength(60, ErrorMessage = "Company name can not be empty.", MinimumLength = 1)]
        public string CustName { get; set; }

        [StringLength(60)]
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

        [Display(Name = "Postal Code")]
        [StringLength(20)]
        public string PostalCode { get; set; }

        [Display(Name = "Loaning Address")]
        [StringLength(100)]
        public string LoaningAddress { get; set; }

        [Display(Name = "Loaning Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> LoaningDate { get; set; }

        [Display(Name = "Time")]
        [StringLength(20)]
        public string LoaningTime { get; set; }

        [Display(Name = "Terms")]
        [StringLength(100)]
        public string PaymentTerms { get; set; }

        [StringLength(50)]
        public string Status { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }

        public int PersonID { get; set; }

        [Display(Name = "Sales Person")]
        [StringLength(30)]
        public string PersonName { get; set; }

        [Display(Name = "Created By")]
        [StringLength(30)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<DateTime> CreatedOn { get; set; }

        [Display(Name = "Last Modified")]
        [StringLength(30)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<DateTime> ModifiedOn { get; set; }
    }
}