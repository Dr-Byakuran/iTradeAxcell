using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using iTrade.CustomAttributes;

namespace iTrade.Models
{
    public class Tutor
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "No")]
        public int TutorID { get; set; }

        [Display(Name = "Tutor Code")]
        [StringLength(20)]
        public string TutorCode { get; set; }

        [Display(Name = "Tutor Type")]
        [StringLength(25)]
        public string TutorType { get; set; }

        [Display(Name = "Job Type")]
        [StringLength(25)]
        public string JobType { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(60, ErrorMessage = "Name can not be empty.", MinimumLength = 1)]
        public string TutorName { get; set; }

        [Display(Name = "NRIC")]
        [StringLength(20)]
        public string NRIC { get; set; }

        [Display(Name = "Gender")]
        [StringLength(20)]
        public string Gender { get; set; }

        [Display(Name = "Date Of Birth")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> DateOfBirth { get; set; }

        [Display(Name = "Qualification")]
        [StringLength(200)]
        [DataType(DataType.MultilineText)]
        public string Qualification { get; set; }

        [StringLength(150)]
        [DataType(DataType.MultilineText)]
        public string Subjects { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string SubjectsName { get; set; }

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string CategoryID { get; set; }

        [Display(Name = "Address")]
        public string Addr1 { get; set; }

        [Display(Name = "Phone (Main)")]
        [StringLength(20)]
        public string PhoneNo { get; set; }

        [Display(Name = "Mobile")]
        [StringLength(20)]
        public string MobileNo { get; set; }

        public int? AttId { get; set; }

        [Display(Name = "Primary Email")]
        [StringLength(50)]
        public string PrimaryEmail { get; set; }

        [Display(Name = "Is Active?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsActive { get; set; }

        [StringLength(200)]
        [DataType(DataType.MultilineText)]
        public string Remark { get; set; }

        [StringLength(200)]
        [DataType(DataType.MultilineText)]
        public string Experience { get; set; }

        //public Attachment Attachment { get; set; }
        //[ForeignKey("Attachment")]
        //public int? AttachmentId { get; set; }


        [Display(Name = "Joined On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<DateTime> JoinedDate { get; set; }

        [Display(Name = "Resigned On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<DateTime> ResignedDate { get; set; }

        [Display(Name = "Created By")]
        [StringLength(30)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Last Modified")]
        [StringLength(30)]
        public string ModifiedBy { get; set; }

        [Display(Name = "Modified On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public Nullable<DateTime> ModifiedOn { get; set; }
        

    }
}