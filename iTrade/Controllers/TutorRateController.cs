using iTrade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        public ActionResult Detail(int id)
        {
            var p = new List<TutorRate>();
            p = db.TutorRates.Where(x => x.TutorID == id).ToList();

            return PartialView(p);
        }

        public ActionResult Item(int id)
        {
            Tutor tut = db.Tutors.Find(id);
            TutorRate p = new TutorRate();
            p.TutorID = tut.TutorID;
            p.TutorName = tut.TutorName;
            p.TutorType = tut.TutorType;

            return PartialView(p);
        }

        public void AddItem(TutorRate det)
        {
            var tutorRate = db.TutorRates.Where(m => m.TutorID == det.TutorID).ToList();
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