using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class StockTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "No")]
        public int TID { get; set; }

        [Display(Name = "Product ID")]
        public int ProductID { get; set; }

        [Display(Name = "SKU")]
        [StringLength(60)]
        public string SKU { get; set; }

        [Display(Name = "Location ID")]
        public int LocationID { get; set; }

        [Display(Name = "Location Name")]
        [StringLength(60)]
        public string LocationName { get; set; }

        [StringLength(15)]
        public string ProcessType { get; set; }

        public double Qty { get; set; }

        [StringLength(25)]
        public string RefNo { get; set; }

        [Display(Name = "From")]
        [StringLength(15)]
        public string SourceFrom { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }

        [Display(Name = "Created By")]
        [StringLength(60)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedOn { get; set; }

    }
}