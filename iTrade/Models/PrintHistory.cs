using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class PrintHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Print ID")]
        public int PrintId { get; set; }

        [Display(Name = "Document Type")]
        [StringLength(20)]
        public string DocumentType { get; set; }

        [Display(Name = "Document No")]
        [StringLength(20)]
        public string DocumentNo { get; set; }

        [Display(Name = "Date Printed")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PrintDate { get; set; }

        [Display(Name = "Printed By")]
        [StringLength(60)]
        public string PrintedBy { get; set; }
    }
}