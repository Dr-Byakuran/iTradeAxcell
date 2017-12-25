using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using iTrade.CustomAttributes;

namespace iTrade.Models
{
    public class Pricebook
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "No")]
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

        [Display(Name = "Cost Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal CostPrice { get; set; }

        [Display(Name = "Cost Code")]
        [StringLength(20)]
        public string CostCode { get; set; }

        [Display(Name = "Register Fee")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal RegisterFee { get; set; }

        [Display(Name = "Student Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal StudentPrice { get; set; }

        [Display(Name = "Public Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal PublicPrice { get; set; }

        [Display(Name = "Default Price?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsDefault { get; set; }

        [Display(Name = "Is Valid?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsValid { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }

        public double Position { get; set; }

        [Display(Name = "Modified By")]
        [StringLength(30)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Last Modified")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ModifiedOn { get; set; }

    }
}