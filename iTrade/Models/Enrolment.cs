using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class Enrolment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Enrol ID")]
        public int EnrID { get; set; }

        [Display(Name = "Enrol#")]
        [StringLength(25)]
        public string EnrNo { get; set; }

        [Display(Name = "Type")]
        [StringLength(25)]
        public string EnrType { get; set; }

        [Display(Name = "Enrol Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EnrDate { get; set; }

        [Required]
        public int CustNo { get; set; }

        [Required]
        [Display(Name = "Student")]
        [StringLength(60, ErrorMessage = "Student name can not be empty.", MinimumLength = 1)]
        public string CustName { get; set; }

        [StringLength(60)]
        public string CustName2 { get; set; }

        [Display(Name = "FIN/NRIC/Passport")]
        [StringLength(20)]
        public string NRIC { get; set; }

        [Display(Name = "PriceID")]
        public int PriceID { get; set; }

        [Display(Name = "Course ID")]
        public int CourseID { get; set; }

        [Display(Name = "Course Name")]
        [StringLength(100)]
        public string CourseName { get; set; }

        [Display(Name = "Course Code")]
        [StringLength(40)]
        public string CourseCode { get; set; }

        [Display(Name = "Course Type")]
        [StringLength(60)]
        public string CourseType { get; set; }

        [Display(Name = "Course Level")]
        [StringLength(30)]
        public string CourseLevel { get; set; }

        [Display(Name = "Duration")]
        [StringLength(30)]
        public string CourseDuration { get; set; }

        [Display(Name = "Teacher Level")]
        [StringLength(30)]
        public string TeacherLevel { get; set; }

        [Display(Name = "Option Name")]
        [StringLength(30)]
        public string OptionName { get; set; }

        [Display(Name = "Tutor ID")]
        public int TutorID { get; set; }

        [Display(Name = "Tutor Name")]
        [StringLength(60)]
        public string TutorName { get; set; }

        public int BranchID { get; set; }

        [Display(Name = "Branch Name")]
        public string BranchName { get; set; }

        [Display(Name = "TermID")]
        public int TermID { get; set; }

        [Display(Name = "TermName")]
        [StringLength(60)]
        public string TermName { get; set; }

        [Display(Name = "ScheduleID")]
        public int ScheduleID { get; set; }

        [Display(Name = "Weekday")]
        public int Weekday { get; set; }

        [Display(Name = "Start Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> EndDate { get; set; }

        [Display(Name = "Start Time")]          
        public DateTime? StartTime { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        [RegularExpression(@"^(0[1-9]|1[0-2]):[0-5][0-9] (am|pm|AM|PM)$", ErrorMessage = "Invalid Time.")]
        public string StartTimeValue
        {
            get
            {
                return StartTime.HasValue ? StartTime.Value.ToString("hh:mm tt") : string.Empty;
            }

            set
            {
                StartTime = DateTime.Parse(value);
            }
        }

        [Display(Name = "End Time")]
        public DateTime? EndTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        [RegularExpression(@"^(0[1-9]|1[0-2]):[0-5][0-9] (am|pm|AM|PM)$", ErrorMessage = "Invalid Time.")]
        public string EndTimeValue
        {
            get
            {
                return EndTime.HasValue ? EndTime.Value.ToString("hh:mm tt") : string.Empty;
            }

            set
            {
                EndTime = DateTime.Parse(value);
            }
        }


        [Display(Name = "Register Fee")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal RegisterFee { get; set; }

        [Display(Name = "Course Fee")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal CourseFee { get; set; }

        [Display(Name = "Deposit")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal Deposit { get; set; }

        [Display(Name = "Sales Type")]
        [StringLength(30)]
        public string SalesType { get; set; }

        [Display(Name = "Is Billable?")]
        public Boolean IsBillable { get; set; }

        [StringLength(100)]
        public string BillRemark { get; set; }

        [Display(Name = "Is Valid?")]
        public Boolean IsValid { get; set; }

        [StringLength(60)]
        public string Status { get; set; }

        [StringLength(200)]
        [DataType(DataType.MultilineText)]
        public string Remark { get; set; }

        public int PersonID { get; set; }

        [Display(Name = "Sales Person")]
        [StringLength(30)]
        public string PersonName { get; set; }

        [Display(Name = "Created By")]
        [StringLength(60)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Last Modified")]
        [StringLength(60)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<DateTime> ModifiedOn { get; set; }

    }
}