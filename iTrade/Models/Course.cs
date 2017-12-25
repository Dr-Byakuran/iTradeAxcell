using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using iTrade.CustomAttributes;

namespace iTrade.Models
{
    public class Course
    {
        public Course()
        {
            this.CoursePrices = new HashSet<CoursePrice>();
            this.CourseBundles = new HashSet<CourseBundle>();

        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "No")]
        public int CourseID { get; set; }

        [Display(Name = "Course Code")]
        [StringLength(40)]
        public string CourseCode { get; set; }

        [Display(Name = "Course Type")]
        [StringLength(60)]
        public string CourseType { get; set; }

        [Required]
        [Display(Name = "Course Name")]
        [StringLength(100, ErrorMessage = "Course name can not be empty.", MinimumLength = 1)]
        public string CourseName { get; set; }

        [Display(Name = "Course Description")]
        [StringLength(100)]
        public string CourseDesc { get; set; }

        [Display(Name = "Category(s)")]
        [StringLength(100)]
        public string CourseCats { get; set; }

        [Display(Name = "Course Level")]
        [StringLength(45)]
        public string CourseLevel { get; set; }

        [Display(Name = "Duration")]
        [StringLength(60)]
        public string Duration { get; set; }

        public int TutorID { get; set; }

        [Display(Name = "Tutor Name")]
        [StringLength(30)]
        public string TutorName { get; set; }

        public int PreCourseID { get; set; }

        [Display(Name = "Prerequisite Course")]
        [StringLength(100)]
        public string PreCourseName { get; set; }

        [Display(Name = "Make Up")]
        public int MaxMakeUp { get; set; }

        [Display(Name = "FOC Revision?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsFocRevision { get; set; }

        [Display(Name = "Maximum Revision")]
        public double MaxRevision { get; set; }

        [Display(Name = "Maximum Years")]
        public double MaxYears { get; set; }

        [Display(Name = "Qty")]
        public double Qty { get; set; }

        [Display(Name = "UOM")]
        [StringLength(60)]
        public string Unit { get; set; }

        [Display(Name = "Base Currency")]
        [StringLength(15)]
        public string BaseCurrency { get; set; }

        [Display(Name = "Registration Fee")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal RegisterFee { get; set; }

        [Display(Name = "Student Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal StudentPrice { get; set; }

        [Display(Name = "Public Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal PublicPrice { get; set; }

        [Display(Name = "Is Bundle?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsBundle { get; set; }

        [Display(Name = "Control Item?")]
        public Boolean IsControlItem { get; set; }

        [Display(Name = "Is Active?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsActive { get; set; }

        public int PersonID { get; set; }

        [Display(Name = "Staff Name")]
        [StringLength(30)]
        public string PersonName { get; set; }

        [DataType(DataType.MultilineText)]
        [StringLength(200)]
        public string Remark { get; set; }

        [StringLength(100)]
        public string ImageUrl { get; set; }

        [StringLength(100)]
        public string ImageUrl2 { get; set; }

        [Display(Name = "Created By")]
        [StringLength(60)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Last Modified")]
        [StringLength(60)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modibied On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ModifiedOn { get; set; }

        public virtual ICollection<CoursePrice> CoursePrices { get; set; }
        public virtual ICollection<CourseBundle> CourseBundles { get; set; }
 

        internal void CreateCoursePrices(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                var po = new CoursePrice();
                if (this.CoursePrices.Count == 0)
                {
                    po.IsDefault = true;
                }
                else
                {
                    po.IsDefault = false;
                }

                CoursePrices.Add(po);
            }
        }

        internal void CreateCourseBundles(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                CourseBundles.Add(new CourseBundle());
            }
        }

    }

    public class CoursePrice
    {
        [Key]
        public int OptionID { get; set; }

        [Display(Name = "Default Price?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsDefault { get; set; }

        [Display(Name = "Price Title")]
        [StringLength(20)]
        public string OptionName { get; set; }

        [Display(Name = "Unit Cost Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal CostPrice { get; set; }

        [Display(Name = "Cost Code")]
        [StringLength(20)]
        public string CostCode { get; set; }

        [Display(Name = "Unit Factor")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public double? UnitFactor { get; set; }

        [Display(Name = "Student Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal StudentPrice { get; set; }

        [Display(Name = "Public Price")]
        [DisplayFormat(DataFormatString = "{0:#,##0.00#}", ApplyFormatInEditMode = true)]
        public decimal PublicPrice { get; set; }

        public Boolean DeleteItem { get; set; }

    }

    public class CourseBundle
    {
        [Key]
        public int BunleID { get; set; }

        [Display(Name = "Course ID")]
        public int IncCourseID { get; set; }

        [Display(Name = "Course Type")]
        [StringLength(30)]
        public string IncCourseType { get; set; }

        [Display(Name = "Course Name")]
        [StringLength(100)]
        public string IncCourseName { get; set; }

        [Display(Name = "Course Code")]
        [StringLength(60)]
        public string IncCourseCode { get; set; }

        [Display(Name = "Duration")]
        [StringLength(30)]
        public string IncDuration { get; set; }

        [Display(Name = "Qty")]
        public double IncQty { get; set; }

        [Display(Name = "UOM")]
        [StringLength(60)]
        public string IncUnit { get; set; }

        public Boolean IsControlItem { get; set; }

        public double Position { get; set; }

        [Display(Name = "Remark")]
        [StringLength(30)]
        public string IncRemark { get; set; }

        public Boolean DeleteBundle { get; set; }

    }
    
}