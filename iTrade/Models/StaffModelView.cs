using iTrade.CustomAttributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace iTrade.Models
{
    public class StaffModelView
    {
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
        [StringLength(30)]
        public string Position { get; set; }

        [Display(Name = "Department")]
        [StringLength(30)]
        public string DepartmentName { get; set; }

        [Display(Name = "Email")]
        [StringLength(50)]
        public string Email { get; set; }

        [Display(Name = "Mobile Number")]
        [StringLength(20)]
        public string MobileNo { get; set; }

        [Display(Name = "Is Active?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsActive { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }

        [Display(Name = "Created By")]
        [StringLength(30)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "User ID")]
        public string UserID { get; set; }
        public string DisplayName { get; set; }
        public Boolean IsCreateNewUser { get; set; }
    }
}