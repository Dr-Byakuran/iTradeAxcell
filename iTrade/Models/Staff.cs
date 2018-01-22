using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using iTrade.CustomAttributes;

namespace iTrade.Models
{
    public class Staff
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Staff ID")]
        public int StaffID { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(30, ErrorMessage = "First name can not be empty.", MinimumLength = 1)]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        [StringLength(30)]
        public string LastName { get; set; }

        [Display(Name = "Job Title")]
        [StringLength(45)]
        public string Position { get; set; }

        [Display(Name = "Assign to Outlet")]
        public string BranchID { get; set; }

        public string BranchName { get; set; }

        [Display(Name = "Department")]
        [StringLength(45)]
        public string DepartmentName { get; set; }

        [Display(Name = "Email")]
        [StringLength(60)]
        public string Email { get; set; }

        [Display(Name = "Mobile Number")]
        [StringLength(30)]
        public string MobileNo { get; set; }

        [Display(Name = "Is Active?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsActive { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }

        [Display(Name = "Created By")]
        [StringLength(60)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "User ID")]
        public string UserID { get; set; }


    }
}