using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using iTrade.Models;

namespace iTrade.Controllers
{
    public class TimeClockController : Controller
    {
        private StarDbContext db = new StarDbContext();

        // GET: TimeClock
        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            };

            Schedule p = db.Schedules.Find(id);

            if (p == null)
            {
                return HttpNotFound();
            };

            ViewBag.CourseID = 0;
            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();

            return View(p);
        }

      //  [HttpGet]
        public ActionResult _ClockIn(int scheduleId, string studentId)
        {
            if (scheduleId == null || studentId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            };

            Schedule sch = db.Schedules.Find(scheduleId);

            if (sch == null)
            {
                return Json(new { success = false, responseText = "The class does not exist. Please verify." }, JsonRequestBehavior.AllowGet);

                //   return HttpNotFound();
            };

            Student stu = db.Students.Where(x => x.NRIC == studentId).FirstOrDefault();

            if (stu == null)
            {
                return Json(new { success = false, responseText = "The student does not exist. Please verify." }, JsonRequestBehavior.AllowGet);

             //   return HttpNotFound();
            };

            var tc = new TimeClock();
          //  tc.ID = 0;
            tc.ScheduleID = sch.ScheduleID;
            tc.CourseID = sch.CourseID;
            tc.CourseName = sch.CourseName;
            tc.PersonID = stu.CustNo;
            tc.PersonName = stu.CustName;
            tc.NRIC = stu.NRIC;
            tc.ClockTime = System.DateTime.Now;
            tc.InOut = "IN";
            tc.ClockLocation = "Default";
            tc.Notes = "";

            return PartialView("_ClockIn", tc);
        }

        [HttpPost]
        public ActionResult _ClockIn(TimeClock timeclock)
        {
            if (ModelState.IsValid)
            {
                db.TimeClocks.Add(timeclock);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(timeclock, JsonRequestBehavior.AllowGet);
        }

        // GET: Shifts/Create
        [HttpGet]
        public ActionResult _ClockOut(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            };


            Student stu = db.Students.Where(x => x.NRIC == id).FirstOrDefault();

            if (stu == null)
            {
                return HttpNotFound();
            };
            var tc = new TimeClock();
            tc.PersonID = stu.CustNo;
            tc.PersonName = stu.CustName;
            tc.NRIC = stu.NRIC;
            tc.ClockTime = System.DateTime.Now;
            tc.InOut = "OUT";
            tc.ClockLocation = "Default";
            tc.Notes = "";

            return PartialView("_ClockOut", tc);
        }


        // POST: /Phone/Create
        [HttpPost]
        public JsonResult _ClockOut(TimeClock timeclock)
        {
            if (ModelState.IsValid)
            {
                db.TimeClocks.Add(timeclock);
                db.SaveChanges();
                return Json(new { success = true });
            }
            return Json(timeclock, JsonRequestBehavior.AllowGet);
        }


    }
}