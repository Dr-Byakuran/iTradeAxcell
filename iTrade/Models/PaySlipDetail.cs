using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class PaySlipDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "No")]
        public int PaySlipDetailID { get; set; }

        [Display(Name = "PaySlip ID")]
        public int PaySlipID { get; set; }

        [Display(Name = "Tutor Code")]
        public string TutorCode { get; set; }

        [Display(Name = "Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Start Time")]
        [StringLength(25)]
        public string StartTime { get; set; }

        [Display(Name = "End Time")]
        [StringLength(25)]
        public string EndTime { get; set; }

        [Display(Name = "Hour")]
        [StringLength(25)]
        public string Hour { get; set; }

        [Display(Name = "Hourly Rate")]
        [StringLength(25)]
        public string HourlyRate { get; set; }

        [Display(Name = "Student Quantity")]
        [StringLength(25)]
        public string Quantity { get; set; }

        [Display(Name = "Amount")]
        public double Amount { get; set; }

        public double Position { get; set; }

    }
}