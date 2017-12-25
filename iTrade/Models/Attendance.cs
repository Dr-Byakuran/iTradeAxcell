using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using iTrade.CustomAttributes;

namespace iTrade.Models
{
    public class Attendance
    {
        [Key]
        public int AttendanceID { get; set; }

        public int ScheduleID { get; set; }

        [Display(Name = "CourseID")]
        public int CourseID { get; set; }

        [Display(Name = "Course Name")]
        [StringLength(100)]
        public string CourseName { get; set; }

        [Display(Name = "Student#")]
        public int CustNo { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        [StringLength(60, ErrorMessage = "Student name can not be empty.", MinimumLength = 1)]
        public string CustName { get; set; }

        [Display(Name = "NRIC/Passport No")]
        [StringLength(20)]
        public string NRIC { get; set; }

        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime AttendanceDate { get; set; }

        [Display(Name = "Is Present?")]
        public Boolean IsPresent { get; set; }

        [StringLength(100)]
        public string Notes { get; set; }

        [StringLength(60)]
        public string AbsentType { get; set; }

        [Display(Name = "Valid for Makeup?")]
        public Boolean IsMakeup { get; set; }

        [StringLength(30)]
        public string Status { get; set; }

        [Display(Name = "IN")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ClockTimeIN { get; set; }

        [Display(Name = "OUT")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss}", ApplyFormatInEditMode = true)]
        public  Nullable<DateTime> ClockTimeOUT { get; set; }

        [Display(Name = "Location")]
        [StringLength(60)]
        public string ClockLocation { get; set; }

        [StringLength(60)]
        public string Remark { get; set; }

        [Display(Name = "Modified By")]
        [StringLength(60)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<DateTime> ModifiedOn { get; set; }


    }
}