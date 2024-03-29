﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class TutorRate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "TutorRateID")]
        public int TutorRateID { get; set; }

        [Display(Name = "Tutor ID")]
        public int TutorID { get; set; }

        [Display(Name = "TutorCode")]
        public string TutorCode { get; set; }

        [Display(Name = "Name")]
        [StringLength(60)]
        public string TutorName { get; set; }

        [Display(Name = "Class Type")]
        [StringLength(20)]
        public string ClassType { get; set; }

        [Display(Name = "Course Code")]
        [StringLength(20)]
        public string CourseCode { get; set; }

        [Display(Name = "Course ID")]
        public int CourseID { get; set; }

        [Display(Name = "Price ID")]
        public int PriceID { get; set; }

        [Display(Name = "Course Name")]
        [StringLength(20)]
        public string CourseName { get; set; }

        [Display(Name = "Course Level")]
        [StringLength(20)]
        public string CourseLevel { get; set; }

        [Display(Name = "Min Attend")]
        public int MinAttend { get; set; }

        [Display(Name = "Max Attend")]
        public int MaxAttend { get; set; }

        [Display(Name = "Rate")]
        public double Rate { get; set; }

        [Display(Name = "Tutor Type")]
        [StringLength(25)]
        public string TutorType { get; set; }

        [Display(Name = "Created By")]
        [StringLength(25)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [StringLength(25)]
        public string CreatedOn { get; set; }

        public double Position { get; set; }
    }
}