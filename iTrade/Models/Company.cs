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
}