using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class CourseSelectEditor
    {
        public bool Selected { get; set; }

        [Key]
        public int CourseID { get; set; }
        public string CourseCode { get; set; }
        public string CourseType { get; set; }
        public string CourseName { get; set; }
        public string CourseDesc { get; set; }
        public string CourseCats { get; set; }
        public string Duration { get; set; }
        public int PreCourseID { get; set; }
        public string PreCourseName { get; set; }
        public Boolean IsFocRevision { get; set; }
        public double Qty { get; set; }
        public string Unit { get; set; }
        public string BaseCurrency { get; set; }
        public decimal StudentPrice { get; set; }
        public decimal PublicPrice { get; set; }
        public Boolean IsBundle { get; set; }
        public string Remark { get; set; }

    }
}