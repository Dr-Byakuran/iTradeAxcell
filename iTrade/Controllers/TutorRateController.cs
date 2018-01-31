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
            if (stu == null)
            {
                return HttpNotFound();
            }

            return View(stu);
        }

        public ActionResult Detail(int id)
        {
            var p = new List<TutorRate>();
            p = db.TutorRates.Where(x => x.TutorID == id).ToList();

            return PartialView(p);
        }

        public ActionResult Item(int id)
        {
            Tutor tut = db.Tutors.Where(x =>x.TutorID ==id).FirstOrDefault();
            TutorRate p = new TutorRate();
            p.TutorID = tut.TutorID;
            p.TutorCode = tut.TutorCode;
            p.TutorName = tut.TutorName;
            p.TutorType = tut.JobType;

            ViewData["CourseAll"] = db.Pricebooks.Where(x => x.IsValid == true).ToList();

            return PartialView(p);
        }

        public void AddItem(TutorRate det)
        {
            var tutorRate = db.TutorRates.Where(m => m.TutorCode == det.TutorCode).ToList();
            double tuCount = tutorRate.Count;
            det.Position = tuCount + 1;
            Pricebook pBook = db.Pricebooks.Where(m => m.PriceID == det.PriceID).FirstOrDefault();
            det.CourseName = pBook.CourseName;
            det.CourseCode = pBook.CourseCode;
            det.CourseID = pBook.CourseID;

            db.TutorRates.Add(det);
            int x = db.SaveChanges();
            int p = det.TutorID;

            if (x != 0)
            {
                Response.Redirect("Edit/" + p);
            }

        }

        public JsonResult AutoCourseLevel(int search)
        {
            if (search != 0)
            {
                var c = db.Pricebooks.Where(x => x.PriceID == search).FirstOrDefault();

                if (c != null)
                {

                    return Json(new { result = c }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return null;
                };

            }
            else
            {
                return null;
            }

        }

        public ActionResult DeleteConfirmed(int id)
        {
            var det = db.TutorRates.Find(id);

            if (det != null)
            {
                db.Entry(det).State = EntityState.Deleted;
                db.SaveChanges();
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}