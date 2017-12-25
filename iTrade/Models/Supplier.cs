using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using iTrade.CustomAttributes;

namespace iTrade.Models
{
    public class Supplier
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Supplier NO")]
        public int SupplierID { get; set; }

        [Required]
        [Display(Name = "Supplier")]
        [StringLength(60, ErrorMessage = "Supplier name can not be empty.", MinimumLength = 1)]
        public string SupplierName { get; set; }

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

        [Display(Name = "Phone")]
        [StringLength(60)]
        public string PhoneNo { get; set; }

        [Display(Name = "Fax")]
        [StringLength(20)]
        public string FaxNo { get; set; }

        [Display(Name = "Contact")]
        [StringLength(60)]
        public string ContactPerson { get; set; }

        [Display(Name = "Primary Email")]
        [StringLength(50)]
        public string PrimaryEmail { get; set; }

        [StringLength(100)]
        public string Website { get; set; }

        [Display(Name = "Is Active?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsActive { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }

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


    }
}