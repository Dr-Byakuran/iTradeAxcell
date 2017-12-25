using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using iTrade.Models;
using System.Data.SqlClient;

namespace iTrade.Controllers
{
    public class StudentsController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();

        // GET: Student
        public ActionResult Index(string txtFilter)
        {
            var result = db.Students.Take(200).ToList();

            if (!string.IsNullOrEmpty(txtFilter))
            {
                result = db.Students.Where(x => x.CustName.Contains(txtFilter) || x.NRIC.StartsWith(txtFilter)).Take(200).ToList();
            }

            return View(result);
        }


        public ActionResult Create()
        {
            StudentViewModel sv = new StudentViewModel();
            var c = new Student();
            c.IsActive = true;
            c.CreatedBy = User.Identity.Name;
            c.CreatedOn = DateTime.Now;

            var cs = new StudentGuardian();
            cs.CreatedBy = User.Identity.Name;
            cs.CreatedOn = DateTime.Now;

            sv.Student = c;
            sv.StudentGuardian = cs;

            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(sv);
        }

        [HttpPost]
     //   [ValidateAntiForgeryToken]
        public ActionResult Create(StudentViewModel svm)
        {
            Student stu = svm.Student;

            var newnum = GetMaxStudentNumber();
            stu.AccNo = newnum;
            stu.CreatedBy = User.Identity.Name;
            stu.CreatedOn = DateTime.Now;
            stu.EnabledAccount = true;
            stu.Deposit = 0;
            stu.TotalFees = 0;
            stu.PaidAmount = 0;
            stu.UnpaidAmount = 0;
       //     stu.Gender = Request.Form["Gender"].ToString();
            

            StudentGuardian sg = svm.StudentGuardian;
            sg.CreatedBy = User.Identity.Name;
            sg.CreatedOn = DateTime.Now;
            sg.ModifiedBy = User.Identity.Name;
            sg.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Students.Add(stu);
                db.SaveChanges();

                sg.CustNo = db.Students.Local[0].CustNo;
                db.StudentGuardians.Add(sg);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(svm);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StudentViewModel svm = new StudentViewModel();
            Student stu = db.Students.Find(id);
            if (stu == null)
            {
                return HttpNotFound();
            }

            StudentGuardian sg = new StudentGuardian(); 
            sg = db.StudentGuardians.Where(m => m.CustNo == id).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            if (sg == null)
            {
                return HttpNotFound();
            }

            svm.Student = stu;
            svm.StudentGuardian = sg;

            ViewBag.Status = "GeneralInfo";
            ViewBag.CustNo = id;

            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(svm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentViewModel svm)
        {
            Student stu = new Student();
            StudentGuardian sg = new StudentGuardian();
  
            int studentID = 0;
            if (ModelState.IsValid)
            {
                stu = svm.Student;
                stu.ModifiedBy = User.Identity.Name;
                stu.ModifiedOn = DateTime.Now;
         //       stu.Gender = Request.Form["Gender"].ToString();

                db.Entry(stu).State = EntityState.Modified;
                db.SaveChanges();

                studentID = stu.CustNo;
                sg = svm.StudentGuardian;
                sg.CustNo = studentID;
                sg.ModifiedBy = User.Identity.Name;
                sg.ModifiedOn = DateTime.Now;
                db.Entry(sg).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            ViewBag.CustNo = studentID;

            ViewBag.Status = Request.Form["selectedTab"];

            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(svm);
        }

        public void Excel()
        {
            //ToExcel Excel = new ToExcel();
            //DataTable dt = new DataTable();
            //string str = System.Configuration.ConfigurationManager.ConnectionStrings["StarConnection"].ConnectionString;
            //SqlConnection sqlcon = new SqlConnection(str);
            //sqlcon.Open();
            //string strsql = "select ROW_NUMBER() over(order by StudentID) as No,RegistrationNo,StudentName,NRIC,CurrentSchool,CurrentLevel,RegistrationDate,PhoneNo,IsActive  from [dbo].[Students] where IsActive='True'";
            //SqlDataAdapter da = new SqlDataAdapter(strsql, sqlcon);
            //DataSet ds = new DataSet();
            //da.Fill(ds);
            //dt = ds.Tables[0];
            //Excel.Excel(dt);
            //sqlcon.Close();
            //Response.End();
        }

    }
}