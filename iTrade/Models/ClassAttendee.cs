using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using iTrade.CustomAttributes;

namespace iTrade.Models
{
    public class ClassAttendee
    {
        [Key]
        public int DetID { get; set; }

        public int RefDetID { get; set; }

        public int AttendID { get; set; }

        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime AttendDate { get; set; }

        public int ScheduleID { get; set; }

        [Display(Name = "Student#")]
        public int CustNo { get; set; }

        [Display(Name = "Full Name")]
        [StringLength(60)]
        public string CustName { get; set; }

        [Display(Name = "NRIC/Passport No")]
        [StringLength(20)]
        public string NRIC { get; set; }

        [Display(Name = "Enrol ID")]
        public int EnrID { get; set; }

        [Display(Name = "Enrol#")]
        [StringLength(25)]
        public string EnrNo { get; set; }

        [Display(Name = "Type")]
        [StringLength(30)]
        public string AttendType { get; set; }

        [Display(Name = "Is Present?")]
        public Boolean IsPresent { get; set; }

        [StringLength(100)]
        public string Notes { get; set; }

        [StringLength(60)]
        public string AbsentType { get; set; }

        [Display(Name = "Valid for Makeup?")]
        public Boolean IsMakeup { get; set; }

        public int ToAttendID { get; set; }

        [Display(Name = "Makeup Class Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ToAttendDate { get; set; }

        [Display(Name = "Refund?")]
        public Boolean IsRefund { get; set; }

        [Display(Name = "Refund Bill DetID")]
        public int ToBillDetID { get; set; }

        [Display(Name = "Action Status")]
        [StringLength(30)]
        public string ActionStatus { get; set; }

        [StringLength(30)]
        public string Status { get; set; }

        [Display(Name = "IN")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ClockTimeIN { get; set; }

        [Display(Name = "OUT")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ClockTimeOUT { get; set; }

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