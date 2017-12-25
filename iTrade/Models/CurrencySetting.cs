using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using iTrade.CustomAttributes;

namespace iTrade.Models
{
    public class CurrencySetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "No")]
        public int ID { get; set; }

        [Required]
        [Display(Name = "Currency")]
        [StringLength(20, ErrorMessage = "Currency can not be empty.", MinimumLength = 1)]
        public string CurrencyName { get; set; }

        [Display(Name = "Country/Region")]
        [StringLength(45)]
        public string CountryName { get; set; }

        [Display(Name = "Exchange Rate")]
        [DisplayFormat(DataFormatString = "{0:#,##0.0000#}", ApplyFormatInEditMode = true)]
        public decimal ExRate { get; set; }

        [Display(Name = "Is Active?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsActive { get; set; }

        [Display(Name = "Updated By")]
        [StringLength(30)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Last Updated")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime ModifiedOn { get; set; }

    }
}