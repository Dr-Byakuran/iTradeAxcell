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
    public class ScheduleController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();

        // GET: Schedule
        public ActionResult Index()
        {
            var p = new List<Schedule>();

            var tday = DateTime.Now;
            DateTime d1 = new DateTime(tday.Year, tday.Month, 1);
            DateTime d2 = d1.AddMonths(1).AddDays(-1);

            p = db.Schedules.Where(x => x.ScheduleDate >= d1 && x.ScheduleDate <= d2).ToList();

            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d2.ToShortDateString();
            ViewBag.CourseID = 0;
            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();

            return View(p);

        }

        [HttpGet]
        public ActionResult Index(string courseID, string fromDate, string toDate)
        {
            int cid = Convert.ToInt32(courseID);
            var tday = DateTime.Now;
            DateTime d1 = new DateTime(tday.Year, tday.Month, 1);
            DateTime d2 = d1.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrEmpty(fromDate))
            {
                d1 = Convert.ToDateTime(fromDate);
            };
            if (!string.IsNullOrEmpty(toDate))
            {
                d2 = Convert.ToDateTime(toDate);
            };

            var p = new List<Schedule>();

            if (cid != 0)
            {
                p = db.Schedules.Where(x => (x.CourseID == cid) && (x.ScheduleDate >= d1 && x.ScheduleDate <= d2)).ToList();

            }
            else
            {
                p = db.Schedules.Where(x => (x.ScheduleDate >= d1 && x.ScheduleDate <= d2)).ToList();

            }


            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d2.ToShortDateString();
            ViewBag.CourseID = cid;
            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();

            return View(p);
        }

        public ActionResult Holiday()
        {
            var p = new List<Holiday>();

            var tday = DateTime.Now;
            DateTime d1 = new DateTime(tday.Year, tday.Month, 1);
            DateTime d2 = d1.AddMonths(1).AddDays(-1);

            p = db.Holidays.ToList();

            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d2.ToShortDateString();
            ViewBag.CourseID = 0;
            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ToList();

            return View(p);

        }

        public ActionResult ClassSchedule()
        {
            var p = new List<ClassSchedule>();

            var tday = DateTime.Now;
            DateTime d1 = new DateTime(tday.Year, tday.Month, 1);
            DateTime d2 = d1.AddMonths(1).AddDays(-1);

            p = db.ClassSchedules.ToList();

            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d2.ToShortDateString();
            ViewBag.CourseID = 0;
            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ToList();

            return View(p);

        }

        [HttpGet]
        public ActionResult ClassSchedule(string courseID, string fromDate, string toDate)
        {
            int cid = Convert.ToInt32(courseID);
            var tday = DateTime.Now;
            DateTime d1 = new DateTime(tday.Year, tday.Month, 1);
            DateTime d2 = d1.AddMonths(1).AddDays(-1);

            if (!string.IsNullOrEmpty(fromDate))
            {
                d1 = Convert.ToDateTime(fromDate);
            };
            if (!string.IsNullOrEmpty(toDate))
            {
                d2 = Convert.ToDateTime(toDate);
            };

            var p = new List<ClassSchedule>();

            if (cid != 0)
            {
                p = db.ClassSchedules.Where(x => (x.PriceID == cid)).ToList();

            }
            else
            {
                p = db.ClassSchedules.ToList();

            }


            ViewBag.StartDate = d1.ToShortDateString();
            ViewBag.EndDate = d2.ToShortDateString();
            ViewBag.CourseID = cid;
            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();
 
            return View(p);
        }

        public ActionResult _AddClassSchedule()
        {
            var p = new ClassSchedule();

            p.FromDate = DateTime.Now;
            p.ToDate = new DateTime(DateTime.Now.Year, 12, 31);
            p.CreatedOn = DateTime.Now;

            ViewData["CoursesAll"] = db.Pricebooks.Where(x => x.IsValid == true).OrderBy(x => x.CourseName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ToList();

            return PartialView(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddClassSchedule(ClassSchedule data)
        {
            var p = db.Pricebooks.Find(data.PriceID);
            if (p != null)
            {
                data.CourseID = p.CourseID;
                data.CourseName = p.CourseName;
                data.CourseLevel = p.CourseLevel;
                data.CourseDuration = p.CourseDuration;
                data.TeacherLevel = p.TeacherLevel;
                data.OptionName = p.OptionName;

            }
            data.CreatedBy = User.Identity.Name;
            data.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                if (data.ScheduleType == "Private") {
                    data.ToDate = data.FromDate;
                    data.Weekday = Convert.ToInt32(data.FromDate.DayOfWeek);
                }
                db.ClassSchedules.Add(data);
                db.SaveChanges();
            };

            CreateClassAttendances(data.ScheduleID);

            ViewData["CoursesAll"] = db.Pricebooks.Where(x => x.IsValid == true).OrderBy(x => x.CourseName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ToList();

            return Json(new { success = true, redirectUrl = Url.Action("ClassSchedule", "Schedule") });

        }

        private void CreateClassAttendances(int id)
        {
            var sch = db.ClassSchedules.Find(id);
            bool flag = db.ClassAttendances.Any(x => x.ScheduleID == sch.ScheduleID);
            if (!flag)
            {
                var dates = new List<DateTime>();
                int weekday = sch.Weekday;              

                DateTime startDate = sch.FromDate;
                DateTime endDate = startDate;
                if (sch.ToDate != null)
                {
                    endDate = Convert.ToDateTime(sch.ToDate);
                }

                for (DateTime dt = startDate; dt < endDate; dt = dt.AddDays(1.0))
                {
                    if ((int)dt.DayOfWeek == weekday)
                    {
                        dates.Add(dt);
                    }
                }

                foreach (var dt in dates)
                {
                    var newatt = new ClassAttendance();
                    newatt.AttendDate = dt;
                    newatt.ScheduleID = sch.ScheduleID;
                    newatt.PriceID = sch.PriceID;
                    newatt.CourseName = sch.CourseName;
                    newatt.CourseLevel = sch.CourseLevel;
                    newatt.CourseDuration = sch.CourseDuration;
                    newatt.TeacherLevel = sch.TeacherLevel;
                    newatt.OptionName = sch.OptionName;
                    newatt.CourseType = sch.ScheduleType;
                    newatt.TutorID = sch.TutorID;
                    newatt.TutorName = sch.TutorName;
                    newatt.Weekday = sch.Weekday;
                    newatt.StartTimeValue = sch.StartTimeValue;
                    newatt.EndTimeValue = sch.EndTimeValue;
                    newatt.StartTime = sch.StartTime;
                    newatt.EndTime = sch.EndTime;

                    newatt.Status = "Open";
                    newatt.CreatedBy = User.Identity.Name;
                    newatt.CreatedOn = DateTime.Now;

                    if (ModelState.IsValid)
                    {
                        db.ClassAttendances.Add(newatt);
                        db.SaveChanges();
                    };

                }

            }
            
        }

        public ActionResult _AddSchedule()
        {
            var p = new Schedule();
            p.CreatedOn = DateTime.Now;

            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();

            return PartialView(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddSchedule(Schedule data)
        {
            data.CreatedBy = User.Identity.Name;
            data.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Schedules.Add(data);
                db.SaveChanges();
            };

            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();

            return Json(new { success = true,  redirectUrl = Url.Action("Index", "Schedule") });

        }

        public ActionResult _EditClassSchedule(int ID)
        {
            var p = db.ClassSchedules.Find(ID);
            //p.CreatedOn = DateTime.Now;

            ViewData["CoursesAll"] = db.Pricebooks.Where(x => x.IsValid == true).OrderBy(x => x.CourseName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ToList();

            return PartialView(p);
            //var p = db.ClassSchedules.Find(ID);
            ////p.CreatedOn = DateTime.Now;

            //ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();

            //return PartialView(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditClassSchedule(ClassSchedule data)
        {
            var p = db.Pricebooks.Find(data.PriceID);
            if (p != null)
            {
                data.CourseID = p.CourseID;
                data.CourseName = p.CourseName;
                data.CourseLevel = p.CourseLevel;
                data.CourseDuration = p.CourseDuration;
                data.TeacherLevel = p.TeacherLevel;
                data.OptionName = p.OptionName;

            }
            data.CreatedBy = User.Identity.Name;
            data.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            };
            return RedirectToAction("/ClassSchedule/Schedule");
            //return Json(new { success = true, redirectUrl = Url.Action("ClassSchedule", "Schedule") });

        }

        public ActionResult _AddHoliday()
        {
            var p = new Holiday();
            p.CreatedOn = DateTime.Now;

            ViewData["CoursesAll"] = db.Pricebooks.Where(x => x.IsValid == true).OrderBy(x => x.CourseName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ToList();

            return PartialView(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddHoliday(Holiday data, DateTime d1, DateTime d2)
        {
            data.CreatedBy = User.Identity.Name;
            data.CreatedOn = DateTime.Now;

            for (DateTime date = d1; date.Date <= d2.Date; date = date.AddDays(1))
            {
                data.Weekday = (int)date.DayOfWeek;
                data.FromDate = date.Date;
                data.ToDate = date.Date;

                if (ModelState.IsValid)
                {
                    db.Holidays.Add(data);
                    db.SaveChanges();
                };
            }

            ViewData["CoursesAll"] = db.Pricebooks.Where(x => x.IsValid == true).OrderBy(x => x.CourseName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ToList();

            return Json(new { success = true, redirectUrl = Url.Action("Holiday", "Schedule") });

        }



        public ActionResult Calendar()
        {
 
            return View();
        }

        public ActionResult View(int id)
        {
            var p = db.ClassSchedules.Find(id);

            return View(p);
        }

        public ActionResult _DisplayStudents(int id)
        {
            var result = db.Enrolments.Where(x => x.ScheduleID == id).ToList();

            return PartialView(result);
        }

        public JsonResult AutoType(int priceid)
        {
            if (priceid != 0)
            {
                var c = db.Pricebooks.Where(x => x.PriceID == priceid).FirstOrDefault();

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

    }
}