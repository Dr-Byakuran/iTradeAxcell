using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using iTrade.Models;
using System.Transactions;
using Microsoft.AspNet.Identity;

namespace iTrade.Controllers
{
    public class AttendanceController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();

        public ActionResult Index()
        {

            return View();

        }

        public ActionResult MakeupList()
        {

            return View();

        }

        public ActionResult Manage(int id, string thedate)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            };

            DateTime mydate = DateTime.Now.Date;
            if (thedate != null)
            {
                mydate = Convert.ToDateTime(thedate);
            }


            var att = db.ClassAttendances.Find(id);
            var sch = db.ClassSchedules.Find(id);

            if (att == null)
            {
                var newatt = new ClassAttendance();
                newatt.AttendDate = mydate;
                newatt.ScheduleID = id;
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

                att = db.ClassAttendances.Find(newatt.AttendID);
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StudentsAll"] = db.Students.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();

            return View(att);

        }

        public ActionResult Edit(int? id)
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

            var list = db.Attendances.Where(x => x.ScheduleID == id).ToList();
            if (list.Count() == 0)
            {
                // create attendancee from existing all student

                var stulist = db.Students.Where(x => x.IsActive == true).ToList();
                foreach (var stu in stulist) {
                    Attendance bb = new Attendance();
                    bb.ScheduleID = Convert.ToInt32(id);
                    bb.CourseID = p.CourseID;
                    bb.CourseName = p.CourseName;
                    bb.CustNo = stu.CustNo;
                    bb.CustName = stu.CustName;
                    bb.NRIC = stu.NRIC;
                    bb.AttendanceDate = DateTime.Today;
                    bb.Status = "";
                    bb.Notes = "";
                    bb.IsMakeup = false;
                    bb.ModifiedBy = User.Identity.Name;
                    bb.ModifiedOn = DateTime.Now;

                    if (ModelState.IsValid)
                    {
                        db.Attendances.Add(bb);
                        db.SaveChanges();
                    };


                }
            }


            return View(p);
        }

        public ActionResult View(int? id)
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

            return View(p);
        }

        public ActionResult _DisplayClassAttendances(string thedate)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            int BranchID = Convert.ToInt32(user.BranchID);

            var list = new List<ClassAttendance>();

            var tday = DateTime.Now;
            DateTime d1 = DateTime.Now.Date;
            DateTime d2 = DateTime.Now.Date;

            int theday = (int)DateTime.Now.DayOfWeek;

            DateTime dateto = DateTime.Today;
            //if (!string.IsNullOrEmpty(thedate))
            //{
            //    dateto = Convert.ToDateTime(thedate);
            //}

            DateTime datefrom = dateto.AddDays(0);

            int weekday = (int)dateto.DayOfWeek;

            // check schedule, if not in attendance then add in.
            var schs = db.ClassSchedules.Where(x => x.Weekday == weekday).ToList();
            foreach(var sch in schs) 
            {
                bool flag = db.ClassAttendances.Any(x => x.ScheduleID == sch.ScheduleID);
                if (!flag)
                {
                    var newatt = new ClassAttendance();
                    newatt.AttendDate = dateto;
                    newatt.ScheduleID = sch.ScheduleID;
                    newatt.PriceID = sch.PriceID;
                    newatt.BranchID = sch.BranchID;
                    newatt.BranchName = sch.BranchName;
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

            if (BranchID == 1)
            {
                list = db.ClassAttendances.Where(x => DbFunctions.TruncateTime(x.AttendDate) >= datefrom && DbFunctions.TruncateTime(x.AttendDate) <= dateto).OrderBy(x => x.StartTime).ToList();
            }
            else
            {
                list = db.ClassAttendances.Where(x => DbFunctions.TruncateTime(x.AttendDate) >= datefrom && DbFunctions.TruncateTime(x.AttendDate) <= dateto && x.BranchID ==BranchID).OrderBy(x => x.StartTime).ToList();
            }

            ViewBag.StartDate = datefrom.ToShortDateString();
            ViewBag.EndDate = dateto.ToShortDateString();
            ViewBag.CourseID = 0;
            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();

            return PartialView(list);
        }

        public ActionResult _DisplayClassAttendancesHist(string thedate, string thetype)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            int BranchID = Convert.ToInt32(user.BranchID);

            var list = new List<ClassAttendance>();

            DateTime dateto = DateTime.Today;
            DateTime datefrom = dateto.AddDays(-365);

            ViewBag.TableNo = 2;

            if (thetype == "BEFORE")
            {
                if (BranchID == 1)
                {
                    list = db.ClassAttendances.Where(x => DbFunctions.TruncateTime(x.AttendDate) >= datefrom && DbFunctions.TruncateTime(x.AttendDate) < dateto).OrderBy(x => x.StartTime).ToList();
                    ViewBag.TableNo = 3;
                }
                else
                {
                    list = db.ClassAttendances.Where(x => DbFunctions.TruncateTime(x.AttendDate) >= datefrom && DbFunctions.TruncateTime(x.AttendDate) < dateto && x.BranchID == BranchID).OrderBy(x => x.StartTime).ToList();
                    ViewBag.TableNo = 3;
                }
            }
            else
            {
                if (BranchID == 1)
                {
                    datefrom = dateto.AddDays(120);
                    list = db.ClassAttendances.Where(x => DbFunctions.TruncateTime(x.AttendDate) <= datefrom && DbFunctions.TruncateTime(x.AttendDate) > dateto).OrderBy(x => x.StartTime).ToList();
                }
                else
                {
                    datefrom = dateto.AddDays(120);
                    list = db.ClassAttendances.Where(x => DbFunctions.TruncateTime(x.AttendDate) <= datefrom && DbFunctions.TruncateTime(x.AttendDate) > dateto && x.BranchID == BranchID).OrderBy(x => x.StartTime).ToList();
                }
            }

            ViewBag.StartDate = datefrom.ToShortDateString();
            ViewBag.EndDate = dateto.ToShortDateString();
            ViewBag.CourseID = 0;
            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();

            return PartialView(list);
        }


        public ActionResult _DisplayClassAttendees(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            };

            var att = db.ClassAttendances.Find(id);

            if (att.Status != "Closed")
            {
                DateTime thedate = att.AttendDate.Date;
                var stus = db.Enrolments.Where(x => x.ScheduleID == att.ScheduleID && DbFunctions.TruncateTime(x.StartDate) <= thedate).ToList();
                foreach (var stu in stus)
                {
                    bool flag = db.ClassAttendees.Any(x => x.ScheduleID == att.ScheduleID && x.AttendID == att.AttendID && x.CustNo == stu.CustNo);
                    if (!flag)
                    {
                        var pp = new ClassAttendee();
                        pp.RefDetID = 0;
                        pp.AttendID = att.AttendID;
                        pp.AttendDate = att.AttendDate;
                        pp.ScheduleID = att.ScheduleID;
                        pp.CustNo = stu.CustNo;
                        pp.CustName = stu.CustName;
                        pp.NRIC = stu.NRIC;
                        pp.AttendType = "REGULAR";
                        pp.IsPresent = true;
                        pp.IsMakeup = false;
                        pp.ToAttendID = 0;
                        pp.IsRefund = false;
                        pp.ToBillDetID = 0;
                        pp.ActionStatus = "Open";
                        pp.Status = "Open";

                        if (ModelState.IsValid)
                        {
                            db.ClassAttendees.Add(pp);
                            db.SaveChanges();
                        };
                    }
                }
            }

            var list = db.ClassAttendees.Where(x => x.AttendID == id).OrderBy(x => x.CustName).ToList();           


            return PartialView(list);
        }

        [HttpPost]
        public ActionResult _DisplayClassAttendees(List<ClassAttendee> list)
        {
            if (list == null)
            {
                return null;
            }

            var sor = list.FirstOrDefault();

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.ClassAttendees.Find(i.DetID);
                    if (det != null)
                    {
                        det.IsPresent = i.IsPresent;
                        det.AbsentType = i.AbsentType;
                        det.IsMakeup = i.IsMakeup;
                        det.Notes = i.Notes;
                        det.Remark = i.Remark;
                        det.ModifiedBy = User.Identity.Name;
                        det.ModifiedOn = DateTime.Now;

                    }
                };
                db.SaveChanges();
            };

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Attendance") });

        }

        public ActionResult _DisplayAttendees(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            };

            var list = db.Attendances.Where(x => x.ScheduleID == id).OrderBy(x => x.CustName).ToList();
 
            return PartialView(list);
        }

        [HttpPost]
        public ActionResult _DisplayAttendees(List<Attendance> list)
        {
            if (list == null)
            {
                return null;
            }

            var sor = list.FirstOrDefault();

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.Attendances.Find(i.AttendanceID);
                    if (det != null)
                    {
                        det.IsPresent = i.IsPresent;
                        det.AbsentType = i.AbsentType;
                        det.IsMakeup = i.IsMakeup;
                        det.Notes = i.Notes; 
                        det.Remark = i.Remark;
                        det.ModifiedBy = User.Identity.Name;
                        det.ModifiedOn = DateTime.Now;

                    }
                };
                db.SaveChanges();
            };

            return Json(new { success = true, redirectUrl = Url.Action("Pricebook", "Courses") });

        }

        public ActionResult _DisplayClassAbsents(string invtype, string thedate)
        {
            var list = new List<ClassAttendeeView>();

            var stus = db.ClassAttendees.Where(x => (x.IsPresent == false || x.AttendType == "ADHOC") && x.ActionStatus != "Completed").OrderBy(x => x.AttendDate).ToList();
            ViewBag.TableNo = 1;

            if (invtype == "Completed")
            {
                stus = db.ClassAttendees.Where(x => x.ActionStatus == "Completed").OrderBy(x => x.AttendDate).ToList();
                ViewBag.TableNo = 2;
            }

            foreach (var stu in stus)
            {
                var att = db.ClassAttendances.Find(stu.AttendID);
                if (att != null)
                {
                    var cav = new ClassAttendeeView();
                    cav.ClassAttendee = stu;
                    cav.ClassAttendance = att;

                    list.Add(cav);
                }
            }

            list = list.Where(x => x.ClassAttendance.Status == "Closed" || x.ClassAttendance.Status == "Open").ToList();

            return PartialView(list);
        }


        [HttpGet]
        public JsonResult _MarkAsComplete(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var det = db.ClassAttendees.Find(id);
                det.ActionStatus = "Completed";
                det.ModifiedBy = User.Identity.Name;
                det.ModifiedOn = DateTime.Now;

                db.Entry(det).State = EntityState.Modified;
                db.SaveChanges();

                trans.Complete();
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult _DelItem(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var det = db.ClassAttendees.Find(id);
                //  double position = det.Position;

                db.Entry(det).State = EntityState.Deleted;
                db.SaveChanges();

                trans.Complete();
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult _ConfirmAttendance(int id)
        {
            var att = db.ClassAttendances.Find(id);

            if (att == null)
            {
                return Json(new { success = false, responseText = "Attendance not found." }, JsonRequestBehavior.AllowGet);
            }

            if (att.AttendDate.Date == DateTime.Today)
            {
                att.Status = "Closed";
            }

            if (ModelState.IsValid)
            {
                db.Entry(att).State = EntityState.Modified;
                db.SaveChanges();
            };

            return Json(new
            {
                success = true,
                printUrl = Url.Action("PrintAttendance", "Attendance", new { id = att.AttendID }),
                redirectUrl = Url.Action("Index", "Attendance"),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult _DisplayRefund(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            };

            var ca = db.ClassAttendees.Find(id);
            var att = db.ClassAttendances.Find(ca.AttendID);
            var price = db.Pricebooks.Find(att.PriceID);
            var enr = db.Enrolments.Where(x => x.CustNo == ca.CustNo &&  x.PriceID == att.PriceID).FirstOrDefault();

            // create refund course fee item

            var givendate = att.AttendDate;
            var days = givendate.Date.Day.ToString() + "/" + givendate.Date.Month.ToString();
            int countdays = 1;

            // unit price base on per 4 unit (dates)
            int baseqty = 4;

            BillItem det = new BillItem();
            det.DetType = "REFUND";
            det.EnrID = enr.EnrID;
            det.EnrNo = enr.EnrNo;
            det.AttDetID = id;
            det.CustNo = enr.CustNo;
            det.CustName = enr.CustName;
            det.CustName2 = enr.CustName2;
            det.NRIC = enr.NRIC;
            det.BillForMonth = DateTime.Now;
            det.ClassDates = days;

            det.ItemID = enr.CourseID;
            det.ItemCode = enr.CourseCode;
            det.ItemType = enr.CourseType;

            string weekdaytxt = Enum.GetName(typeof(DayOfWeek), enr.Weekday);

            det.ItemName = "Refund for " + enr.CourseName + " " + weekdaytxt + " " + enr.StartTimeValue + " - " + enr.EndTimeValue;
            det.Remark = days;
            det.ItemDesc = "Refund for " + enr.CourseName + " " + weekdaytxt + " " + enr.StartTimeValue + " - " + enr.EndTimeValue;
            det.SellType = "CS";
            det.Qty = countdays;
            det.Unit = "";
            det.UnitPrice = enr.CourseFee;
            det.DiscountedPrice = enr.CourseFee;
            det.PreDiscAmount = enr.CourseFee;
            det.Discount = 0;

            decimal unitfee = System.Math.Round((det.DiscountedPrice / baseqty), 2, MidpointRounding.AwayFromZero);
            det.Amount = 0 - System.Math.Round((unitfee * Convert.ToDecimal(det.Qty)), 2, MidpointRounding.AwayFromZero);

            det.Gst = 0;
            det.Nett = det.Amount + det.Gst;
            det.IsBundle = false;
            det.SalesType = enr.SalesType;
            det.RefItemID = 0;
            det.InvRef = "";
            det.IsControlItem = false;
            det.LocationID = 0;
            det.LocationName = "";
            det.Position = 1;
            det.CreatedBy = User.Identity.Name;
            det.CreatedOn = DateTime.Now;


            return PartialView(det);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _DisplayRefund(BillItem data)
        {
            data.ModifiedBy = User.Identity.Name;
            data.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.BillItems.Add(data);
                db.SaveChanges();
            };

            var ca = db.ClassAttendees.Find(data.AttDetID);
            ca.ActionStatus = "Completed";
            ca.ModifiedBy = User.Identity.Name;
            ca.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(ca).State = EntityState.Modified;
                db.SaveChanges();
            }

         //   return Json(new { success = true, totalamount = 0, detcount = 0, redirectUrl = Url.Action("MakeupList", "Attendance") });
            return Json(new
            {
                success = true
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult _DisplayCharge(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            };

            var ca = db.ClassAttendees.Find(id);
            var att = db.ClassAttendances.Find(ca.AttendID);
            var price = db.Pricebooks.Find(att.PriceID);
          //  var enr = db.Enrolments.Where(x => x.CustNo == ca.CustNo && x.PriceID == att.PriceID).FirstOrDefault();

            // create refund course fee item

            var givendate = att.AttendDate;
            var days = givendate.Date.Day.ToString() + "/" + givendate.Date.Month.ToString();
            int countdays = 1;

            // unit price base on per 4 unit (dates)
            int baseqty = 4;

            BillItem det = new BillItem();
            det.DetType = "ADHOC";
            det.EnrID = 0;
            det.EnrNo = null;
            det.AttDetID = id;
            det.CustNo = ca.CustNo;
            det.CustName = ca.CustName;
            det.CustName2 = null;
            det.NRIC = ca.NRIC;
            det.BillForMonth = DateTime.Now;
            det.ClassDates = days;

            det.ItemID = price.CourseID;
            det.ItemCode = price.CourseCode;
            det.ItemType = price.CourseType;

            string weekdaytxt = Enum.GetName(typeof(DayOfWeek), att.Weekday);

            det.ItemName = "Fee for " + price.CourseName + " " + weekdaytxt + " " + att.StartTimeValue + " - " + att.EndTimeValue;
            det.Remark = days;
            det.ItemDesc = "Fee for " + price.CourseName + " " + weekdaytxt + " " + att.StartTimeValue + " - " + att.EndTimeValue;
            det.SellType = "CS";
            det.Qty = countdays;
            det.Unit = "";
            det.UnitPrice = price.StudentPrice;
            det.DiscountedPrice = price.StudentPrice;
            det.PreDiscAmount = price.StudentPrice;
            det.Discount = 0;

            decimal unitfee = System.Math.Round((det.DiscountedPrice / baseqty), 2, MidpointRounding.AwayFromZero);
            det.Amount = System.Math.Round((unitfee * Convert.ToDecimal(det.Qty)), 2, MidpointRounding.AwayFromZero);

            det.Gst = 0;
            det.Nett = det.Amount + det.Gst;
            det.IsBundle = false;
            det.SalesType = "Default";
            det.RefItemID = 0;
            det.InvRef = "";
            det.IsControlItem = false;
            det.LocationID = 0;
            det.LocationName = "";
            det.Position = 1;
            det.CreatedBy = User.Identity.Name;
            det.CreatedOn = DateTime.Now;

            return PartialView(det);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _DisplayCharge(BillItem data)
        {
            data.ModifiedBy = User.Identity.Name;
            data.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.BillItems.Add(data);
                db.SaveChanges();
            };

            var ca = db.ClassAttendees.Find(data.AttDetID);
            ca.ActionStatus = "Completed";
            ca.ModifiedBy = User.Identity.Name;
            ca.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(ca).State = EntityState.Modified;
                db.SaveChanges();
            }
            
            //   return Json(new { success = true, totalamount = 0, detcount = 0, redirectUrl = Url.Action("MakeupList", "Attendance") });
            return Json(new
            {
                success = true
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult _DisplayUpcomingClasses(int id)
        {
            var dt = DateTime.Now.Date;

            var ca = db.ClassAttendees.Find(id);
            var sch = db.ClassSchedules.Find(ca.ScheduleID);

            var atts = db.ClassAttendances.Where(x => x.ScheduleID != ca.ScheduleID && x.PriceID == sch.PriceID && DbFunctions.TruncateTime(x.AttendDate) > dt).ToList();

            ViewBag.AttDetID = id;
            ViewBag.CustNo = ca.CustNo;
            ViewBag.CustName = ca.CustName;

            return PartialView(atts);
        }

        [HttpGet]
        public JsonResult _AddAttendee(int id, int custno, int attdetid, string atype)
        {
            var ca = db.ClassAttendees.Where(x => x.DetID == attdetid).FirstOrDefault();
            if (ca == null && atype != "ADHOC")
            {
                return Json(new { success = false, responseText = "Attendance not found." }, JsonRequestBehavior.AllowGet);
            }

            bool flag = db.ClassAttendees.Any(x => x.AttendID == id && x.CustNo == custno);
            if (flag)
            {
                return Json(new { success = false, responseText = "The student already in the class." }, JsonRequestBehavior.AllowGet);
            }
            var att = db.ClassAttendances.Find(id);
            if (att == null)
            {
                return Json(new { success = false, responseText = "Attendance not found." }, JsonRequestBehavior.AllowGet);
            }
            var stu = db.Clients.Find(custno);

            var pp = new ClassAttendee();
            pp.RefDetID = attdetid;
            pp.AttendID = att.AttendID;
            pp.AttendDate = att.AttendDate;
            pp.ScheduleID = att.ScheduleID;
            pp.CustNo = stu.CustNo;
            pp.CustName = stu.CustName;
            pp.NRIC = stu.NRIC;
            pp.AttendType = atype;
            pp.IsPresent = true;
            pp.IsMakeup = false;
            pp.IsRefund = false;
            pp.ActionStatus = "Open";
            pp.Status = "Open";
            pp.ModifiedBy = User.Identity.Name;
            pp.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.ClassAttendees.Add(pp);
                db.SaveChanges();
            };


            if (ca != null)
            {
                ca.ActionStatus = "Completed";
                ca.ModifiedBy = User.Identity.Name;
                ca.ModifiedOn = DateTime.Now;

                if (ModelState.IsValid)
                {
                    db.Entry(ca).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            return Json(new
            {
                success = true
            }, JsonRequestBehavior.AllowGet);
        }


    }
}