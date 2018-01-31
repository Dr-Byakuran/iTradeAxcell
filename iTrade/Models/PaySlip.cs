using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class PaySlip
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "No")]
        public int PaySlipID { get; set; }

        [Display(Name = "Tutor Code")]
        [StringLength(25)]
        public string TutorCode { get; set; }

        [Display(Name = "Tutor ID")]
        public int TutorID { get; set; }

        [Display(Name = "Tutor Name")]
        [StringLength(25)]
        public string TutorName { get; set; }

        [Display(Name = "NRIC/FIN")]
        [StringLength(60)]
        public string Nric { get; set; }

        [Display(Name = "Period of payment")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PaymentDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime PaymentDate2 { get; set; }

        [Display(Name = "Date")]
        public string Date { get; set; }

        [Display(Name = "Total")]
        public double Total { get; set; }

    }
}