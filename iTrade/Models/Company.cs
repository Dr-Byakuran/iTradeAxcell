using iTrade.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "No")]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Chinese Name")]
        [StringLength(100)]
        public string ChineseName { get; set; }

        [Required]
        [Display(Name = "Address")]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Telephone Number")]
        [StringLength(40)]
        public string TelephoneNumber { get; set; }

        [Display(Name = "Fax Number")]
        [StringLength(40)]
        public string FaxNumber { get; set; }

        [Required]
        [Display(Name = "Email Address")]
        [StringLength(100)]
        public string EmailAddress { get; set; }

        [Required]
        [Display(Name = "Business Reg. No.")]
        [StringLength(100)]
        public string BusinessRegNo { get; set; }

        [Required]
        [Display(Name = "GST Reg. No.")]
        [StringLength(100)]
        public string GSTRegNo { get; set; }

        [Display(Name = "Logo Image")]
        public byte[] LogoImage { get; set; }

        [Display(Name = "Created By")]
        [StringLength(40)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Last Modified")]
        [StringLength(40)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ModifiedOn { get; set; }
    }

    public class CompanyBranch
    {
        [Key]
        public int BranchID { get; set; }

        [Display(Name = "Default Branch?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsDefault { get; set; }

        [Display(Name = "CompanyID")]
        public int CompID { get; set; }

        [Display(Name = "Branch Name")]
        [StringLength(100)]
        public string BranchName { get; set; }

        [Display(Name = " ")]
        [StringLength(100)]
        public string BranchName2 { get; set; }

        [Display(Name = "Branch Code")]
        [StringLength(20)]
        public string BranchCode { get; set; }

        [Display(Name = "Type")]
        [StringLength(20)]
        public string BranchType { get; set; }

        [Display(Name = "Address")]
        [StringLength(100, MinimumLength = 1)]
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

        [Display(Name = "Postal Code")]
        [StringLength(20)]
        public string PostalCode { get; set; }

        [StringLength(60)]
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
        [StringLength(30)]
        public string FaxNo { get; set; }

        [Display(Name = "Contact/Attn")]
        [StringLength(60)]
        public string ContactPerson { get; set; }

        [Display(Name = "Primary Email")]
        [StringLength(60)]
        public string PrimaryEmail { get; set; }

        [StringLength(60)]
        public string Website { get; set; }

        [StringLength(45)]
        public string Group { get; set; }

        [Display(Name = "Is Active?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsActive { get; set; }

        [StringLength(150)]
        [DataType(DataType.MultilineText)]
        public string Remark { get; set; }

        public string ImageUrl { get; set; }

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