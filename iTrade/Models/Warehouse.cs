using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using iTrade.CustomAttributes;

namespace iTrade.Models
{
    public class Warehouse
    {
        [Key]
        [Display(Name = "Location ID")]
        public int LocationID { get; set; }

        [Required]
        [Display(Name = "Location Name")]
        [StringLength(60)]
        public string LocationName { get; set; }

        [Display(Name = "Type")]
        [StringLength(30)]
        public string LocationType { get; set; }

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

        [StringLength(45)]
        public string Country { get; set; }

        [Display(Name = "Phone")]
        [StringLength(60)]
        public string PhoneNo { get; set; }

        [Display(Name = "Fax")]
        [StringLength(20)]
        public string FaxNo { get; set; }

        [Display(Name = "Contact Person")]
        [StringLength(60)]
        public string ContactPerson { get; set; }

        [Display(Name = "Is Active?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsActive { get; set; }


    }
}