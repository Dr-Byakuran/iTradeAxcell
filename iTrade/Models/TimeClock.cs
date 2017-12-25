using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class TimeClock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int PersonID { get; set; }

        [Required]
        public string PersonName { get; set; }

        [Display(Name = "NRIC/Passport No")]
        [StringLength(20)]
        public string NRIC { get; set; }

        public int ScheduleID { get; set; }

        [Display(Name = "CourseID")]
        public int CourseID { get; set; }

        [Display(Name = "Course Name")]
        [StringLength(100)]
        public string CourseName { get; set; }

        [Required]
        [Display(Name = "Clock Time")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime ClockTime{ get; set; }

        [Required]
        [StringLength(20)]
        public string InOut { get; set; }

        [StringLength(60)]
        public string ClockLocation { get; set; }

        [StringLength(60)]
        public string Notes { get; set; }
    }
}