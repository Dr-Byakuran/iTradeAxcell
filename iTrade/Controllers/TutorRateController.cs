using iTrade.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web;
using System.Web.Mvc;

namespace iTrade.Controllers
{
    public class TutorRateController : Controller
    {
        private StarDbContext db = new StarDbContext();
        // GET: TutorRate
        public ActionResult Index(string txtFilter)
        {
            var result = db.Tutors.Take(200).ToList();

            if (!string.IsNullOrEmpty(txtFilter))
            {
                result = db.Tutors.Where(x => x.TutorName.Contains(txtFilter) || x.TutorCode.StartsWith(txtFilter)).Take(200).ToList();
            }

            return View(result);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tutor stu = db.Tutors.Find(id);
            ViewBag.No = stu.AttId;
            if (stu == null)
            {
                return HttpNotFound();
            }
            ViewBag.Status = "GeneralInfo";
            ViewBag.CustNo = id;
            ViewData["CourseAll"] = db.Courses.Where(x => x.IsActive == true).ToList();
            return View(stu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Tutor tu)
        {
            int tutorID = 0;
            if (ModelState.IsValid)
            {
                tutorID = tu.TutorID;
                tu.TutorType = (Request["TutorType"] == null) ? "" : Request["TutorType"].ToString();
                tu.JobType = (Request["JobType"] == null) ? "" : Request["JobType"].ToString();
                tu.Gender = (Request["Gender"] == null) ? "" : Request["Gender"].ToString();

                tu.Subjects = (Request["CourseSubjects"] != null) ? Request["CourseSubjects"].ToString() : "";
                string zhi = "";
                string categoryid = "";
                List<Course> cse = db.Courses.Where(x => x.IsActive == true).ToList();
                string[] val = tu.Subjects.Split(',');
                for (int i = 0; i < val.Length; i++)
                {
                    for (int k = 0; k < cse.Count; k++)
                    {
                        if (val[i] == cse[k].CourseID.ToString())
                        {
                            if (zhi == "")
                            {
                                zhi = cse[k].CourseName;
                            }
                            else
                            {
                                zhi += "," + cse[k].CourseName;
                            }

                            //if (categoryid == "")
                            //{
                            //    categoryid = cse[k].CourseCategory;
                            //}
                            //else
                            //{
                            //    categoryid += "," + cse[k].CourseCategory;
                            //}
                            break;
                        }
                    }
                }
                tu.AttId = (Request["AttachmenNo"] == null) ? 0 : Convert.ToInt32(Request["AttachmenNo"].ToString());
                tu.SubjectsName = zhi;
                tu.CategoryID = categoryid;

                tu.ModifiedBy = User.Identity.Name;
                tu.ModifiedOn = DateTime.Now;

                db.Entry(tu).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CustNo = tutorID;

            //ViewBag.Status = Request.Form["selectedTab"];
            ViewData["CourseAll"] = db.Courses.Where(x => x.IsActive == true).ToList();
            return View(tu);
        }

        public ActionResult Detail(string id)
        {
            var p = new List<TutorRate>();
            p = db.TutorRates.Where(x => x.TutorCode == id).ToList();

            return PartialView(p);
        }

        public ActionResult Item(string id)
        {
            Tutor tut = db.Tutors.Where(x =>x.TutorCode ==id).FirstOrDefault();
            TutorRate p = new TutorRate();
            p.TutorID = tut.TutorID;
            p.TutorCode = tut.TutorCode;
            p.TutorName = tut.TutorName;
            p.TutorType = tut.JobType;

            ViewData["CourseAll"] = db.Courses.Where(x => x.IsActive == true).ToList();

            return PartialView(p);
        }

        public void AddItem(TutorRate det)
        {
            var tutorRate = db.TutorRates.Where(m => m.TutorCode == det.TutorCode).ToList();
            double tuCount = tutorRate.Count;
            det.Position = tuCount + 1;

            db.TutorRates.Add(det);
            int x = db.SaveChanges();
            int p = det.TutorID;

            if (x != 0)
            {
                Response.Redirect("Edit/" + p);
            }

        }
    }
}