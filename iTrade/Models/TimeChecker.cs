using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class TimeChecker
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime DateSelected { get; set; }

        public int PersonID { get; set; }

        public string PersonName { get; set; }


        [Display(Name = "Clock In")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ClockInTime { get; set; }


        [Display(Name = "Clock Out")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ClockOutTime { get; set; }

        public double TotalHours { get; set; }

        [StringLength(30)]
        public string ClockLocation { get; set; }

        [StringLength(20)]
        public string Notes { get; set; }

    }
}