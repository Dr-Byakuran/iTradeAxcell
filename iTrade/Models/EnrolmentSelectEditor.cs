using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace iTrade.Models
{
    public class EnrolmentSelectEditor
    {
        public bool Selected { get; set; }

        [Key]
        public int EnrID { get; set; }
        public string EnrNo { get; set; }
        public string EnrType { get; set; }
        public DateTime EnrDate { get; set; }
        public int CustNo { get; set; }
        public string CustName { get; set; }
        public string CustName2 { get; set; }
        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public string NRIC { get; set; }
        public int CourseID { get; set; }
        public string CourseName { get; set; }
        public string CourseCode { get; set; }
        public string CourseType { get; set; }
        public string CourseLevel { get; set; }
        public string CourseDuration { get; set; }
        public string TeacherLevel { get; set; }
        public string OptionName { get; set; }
        public int TutorID { get; set; }
        public string TutorName { get; set; }
        public int TermID { get; set; }
        public string TermName { get; set; }
        public int Weekday { get; set; }
        public DateTime StartDate { get; set; }
        public Nullable<DateTime> EndDate { get; set; }
        public decimal RegisterFee { get; set; }
        public decimal CourseFee { get; set; }
        public decimal Deposit { get; set; }
        public string SalesType { get; set; }
        public Boolean IsBillable { get; set; }
        public string BillRemark { get; set; }
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
        [StringLength(30)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }
    }
}