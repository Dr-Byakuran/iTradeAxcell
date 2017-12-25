using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using iTrade.CustomAttributes;

namespace iTrade.Models
{

    public class VendorViewModel
    {
        public virtual Vendor Vendor { get; set; }
        public virtual VendorCreditSetting VendorCreditSetting { get; set; }
    }

    public class Vendor
    {
        public Vendor()
        {
            this.VendorCreditSettings = new HashSet<VendorCreditSetting>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Customer#")]
        public int CustNo { get; set; }

        [Display(Name = "ACC NO")]
        [StringLength(20)]
        public string AccNo { get; set; }

        [Display(Name = "Account Type")]
        [StringLength(20)]
        public string AccType { get; set; }

        [Required]
        [Display(Name = "Company")]
        [StringLength(100, ErrorMessage = "Company name can not be empty.", MinimumLength = 1)]
        public string CustName { get; set; }

        [Display(Name = "Address")]
        [StringLength(100, MinimumLength = 1)]
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

        [StringLength(45)]
        public string Country { get; set; }

        [Display(Name = "Phone (Main)")]
        [StringLength(60)]
        public string PhoneNo { get; set; }

        [Display(Name = "Phone")]
        [StringLength(30)]
        public string PhoneNo2 { get; set; }

        [Display(Name = "Mobile")]
        [StringLength(30)]
        public string MobileNo { get; set; }

        [Display(Name = "Fax")]
        [StringLength(20)]
        public string FaxNo { get; set; }

        [Display(Name = "Contact")]
        [StringLength(60)]
        public string ContactPerson { get; set; }

        [Display(Name = "Primary Email")]
        [StringLength(60)]
        public string PrimaryEmail { get; set; }

        [StringLength(60)]
        public string Website { get; set; }

        [StringLength(30)]
        public string Group { get; set; }

        [Display(Name = "Is Active?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsActive { get; set; }

        [StringLength(60)]
        public string Terms { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }

        [Display(Name = "Assign To")]
        [StringLength(60)]
        public string AssignedTo { get; set; }

        public int SalesPersonID { get; set; }

        [Display(Name = "Sales Person")]
        [StringLength(60)]
        public string SalesPersonName { get; set; }

        [Display(Name = "Created By")]
        [StringLength(60)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Last Modified")]
        [StringLength(60)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modibied On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ModifiedOn { get; set; }

        public virtual ICollection<VendorCreditSetting> VendorCreditSettings { get; set; }
        internal void CreateCreditSetting(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                VendorCreditSettings.Add(new VendorCreditSetting());
            }
        }

    }

    public class VendorCreditSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Credit No")]
        public int CreditCustNo { get; set; }

        [Display(Name = "Credit Limit")]
        public decimal? CreditLimit { get; set; }

        [Display(Name = "Overdue Limit")]
        public decimal? OverdueLimit { get; set; }

        [Display(Name = "Account Balance")]
        public decimal? AccountBalance { get; set; }

        [Display(Name = "Payment Term (days)")]
        public int? PaymentTerms { get; set; }

        [Display(Name = "Manage credit for this Vendor?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsCreditAllowed { get; set; }

        public int CustNo { get; set; }

        [Display(Name = "Created By")]
        [StringLength(60)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Last Modified")]
        [StringLength(60)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ModifiedOn { get; set; }

    }

}