using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using iTrade.CustomAttributes;

namespace iTrade.Models
{
    public class StudentViewModel
    {
        public virtual Student Student { get; set; }
        public virtual StudentGuardian StudentGuardian { get; set; }
    }

    public class Student
    {        
        public Student()
        {
            this.StudentGuardians = new HashSet<StudentGuardian>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Student#")]
        public int CustNo { get; set; }

        [Display(Name = "ACC NO")]
        [StringLength(20)]
        public string AccNo { get; set; }

        [Display(Name = "Account Type")]
        [StringLength(20)]
        public string AccType { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        [StringLength(60, ErrorMessage = "Student name can not be empty.", MinimumLength = 1)]
        public string CustName { get; set; }

        [Display(Name = " ")]
        [StringLength(60)]
        public string CustName2 { get; set; }

        [Display(Name = "FIN/NRIC/Passport")]
        [StringLength(20)]
        public string NRIC { get; set; }

        [Display(Name = "Nationality")]
        [StringLength(30)]
        public string Nationality { get; set; }

        [Display(Name = "Gender")]
        [StringLength(20)]
        public string Gender { get; set; }

        [Display(Name = "Date Of Birth")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> DateOfBirth { get; set; }

        [Display(Name = "Current School")]
        [StringLength(150)]
        public string CurrentSchool { get; set; }

        [Display(Name = "Current Level ")]
        [StringLength(60)]
        public string CurrentLevel { get; set; }

        [Display(Name = "Subscription for marketing?")]
        [BooleanDisplayValuesAsYesNo]
        public string EnableMarketing { get; set; }

        [Display(Name = "Lead From ")]
        [StringLength(60)]
        public string LeadChannel { get; set; }

        [Display(Name = "Address")]
        [StringLength(100)]
        public string Addr1 { get; set; }

        [Display(Name = " ")]
        [StringLength(100)]
        public string Addr2 { get; set; }

        [Display(Name = " ")]
        [StringLength(100)]
        public string Addr3 { get; set; }

        [Display(Name = "Postal Code")]
        [StringLength(20)]
        public string PostalCode { get; set; }

        [StringLength(20)]
        public string Country { get; set; }

        [Display(Name = "Phone (Main)")]
        [StringLength(20)]
        public string PhoneNo { get; set; }

        [Display(Name = "Phone")]
        [StringLength(20)]
        public string PhoneNo2 { get; set; }

        [Display(Name = "Mobile")]
        [StringLength(20)]
        public string MobileNo { get; set; }

        [Display(Name = "Fax")]
        [StringLength(20)]
        public string FaxNo { get; set; }

        [Display(Name = "Contact")]
        [StringLength(30)]
        public string ContactPerson { get; set; }

        [Display(Name = "Primary Email")]
        [StringLength(50)]
        public string PrimaryEmail { get; set; }

        [StringLength(50)]
        public string Website { get; set; }

        [StringLength(30)]
        public string Group { get; set; }

        [StringLength(30)]
        public string Status { get; set; }

        [Display(Name = "Is Active?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean IsActive { get; set; }

        [StringLength(60)]
        public string Terms { get; set; }

        [Display(Name = "Discount %")]
        public decimal DiscountPercent { get; set; }

        [Display(Name = "Assign To")]
        [StringLength(30)]
        public string AssignedTo { get; set; }

        public int SalesPersonID { get; set; }

        [Display(Name = "Sales Person")]
        [StringLength(30)]
        public string SalesPersonName { get; set; }

        [StringLength(100)]
        public string Remark { get; set; }  
  

      
        [Display(Name = "Name")]
        [StringLength(60)]
        public string FatherName { get; set; }

        [Display(Name = "NRIC")]
        [StringLength(20)]
        public string FatherNRIC { get; set; }

        [Display(Name = "Mobile")]
        [StringLength(20)]
        public string FatherMobile { get; set; }

        [Display(Name = "Occupation")]
        [StringLength(60)]
        public string FatherOccupation { get; set; }

        [Display(Name = "Name")]
        [StringLength(60)]
        public string MontherName { get; set; }

        [Display(Name = "NRIC")]
        [StringLength(20)]
        public string MontherNRIC { get; set; }

        [Display(Name = "Mobile")]
        [StringLength(20)]
        public string MontherMobile { get; set; }

        [Display(Name = "Occupation")]
        [StringLength(60)]
        public string MontherOccupation { get; set; }


        [Display(Name = "Medical Condition / Allergies")]
        [StringLength(200)]
        [DataType(DataType.MultilineText)]
        public string MedicalCondition { get; set; }

        [Display(Name = "CCA in School & Frequency")]
        [StringLength(200)]
        [DataType(DataType.MultilineText)]
        public string CCANote { get; set; }

        [Display(Name = "Attending other Tuition / Encrichment Classes?")]
        [BooleanDisplayValuesAsYesNo]
        public string IsAttendOthers { get; set; }

        [Display(Name = "If 'Yes', please state venue and frequency")]
        [StringLength(200)]
        [DataType(DataType.MultilineText)]
        public string VenueNote { get; set; }


        //-----
        [Display(Name = "Manage account for this client?")]
        [BooleanDisplayValuesAsYesNo]
        public Boolean EnabledAccount { get; set; }
        //----
        [Display(Name = "Deposit")]
        public decimal Deposit { get; set; }
        //--
        [Display(Name = "Total Fees")]
        public decimal TotalFees { get; set; }
        //---
        [Display(Name = "Paid")]
        public decimal PaidAmount { get; set; }
        //----
        [Display(Name = "UnPaid")]
        public decimal UnpaidAmount { get; set; }


        [Display(Name = "Created By")]
        [StringLength(30)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedOn { get; set; }
        
        [Display(Name = "Last Modified")]
        [StringLength(30)]
        public string ModifiedBy { get; set; }
        
        [Display(Name = "Modibied On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ModifiedOn { get; set; }

        public virtual ICollection<StudentGuardian> StudentGuardians { get; set; }
        internal void StudentGuardian(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                StudentGuardians.Add(new StudentGuardian());
            }
        }

    }


    public class StudentGuardian
    {
        [Key] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Guardian No")]
        public int GuardianNo { get; set; }

        [Display(Name = "Student#")]
        public int CustNo { get; set; }

        [Display(Name = "Name")]
        [StringLength(60)]
        public string GuardianName { get; set; }

        [Display(Name = "NRIC")]
        [StringLength(20)]
        public string NRIC { get; set; }

        [Display(Name = "Relationship to student")]
        [StringLength(100)]
        public string Relationship { get; set; }

        [Display(Name = "Occupation")]
        [StringLength(60)]
        public string Occupation { get; set; }

        [Display(Name = "Address")]
        public string Addr1 { get; set; }

        [Display(Name = "Phone (Main)")]
        [StringLength(20)]
        public string PhoneNo { get; set; }

        [Display(Name = "Mobile")]
        [StringLength(20)]
        public string MobileNo { get; set; }

        [Display(Name = "Primary Email")]
        [StringLength(50)]
        public string PrimaryEmail { get; set; }
        
        [Display(Name = "Created By")]
        [StringLength(30)]
        public string CreatedBy { get; set; }
        
        [Display(Name = "Created On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime CreatedOn { get; set; }
        
        [Display(Name = "Last Modified")]
        [StringLength(30)]
        public string ModifiedBy { get; set; }
        
        [Display(Name = "Modified On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public Nullable<DateTime> ModifiedOn { get; set; }

    }

}