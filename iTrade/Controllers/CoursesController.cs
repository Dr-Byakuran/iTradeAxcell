using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using iTrade.Models;
using System.Transactions;

namespace iTrade.Controllers
{
    public class CoursesController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();

        // GET: Courses
        public ActionResult Index(string txtSearch)
        {
            var result = db.Courses.Take(600).ToList();

            if (!string.IsNullOrEmpty(txtSearch))
            {
                result = db.Courses.Where(x => x.CourseName.Contains(txtSearch.Trim()) || x.CourseCode.StartsWith(txtSearch.Trim()) || x.CourseDesc.StartsWith(txtSearch.Trim())).Take(600).ToList();
            }

            return View(result);
        }

        public ActionResult Create()
        {
            var p = new Course();
            p.IsFocRevision = false;
            p.IsControlItem = true;
            p.IsActive = true;
            p.CreatedBy = User.Identity.Name;
            p.CreatedOn = DateTime.Now;

            p.CreateCoursePrices(1);
            p.CreateCourseBundles(1);
      //      p.CreateCourseSchedule(1);
 
            ViewBag.ProductTypes = db.ProductGroups.ToList();
            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            var cc = db.CustomSettings.Where(x => x.Name == "CostCode").FirstOrDefault();
            if (cc == null)
                ViewBag.CostCodeSetting = "";
            else
                ViewBag.CostCodeSetting = cc.TextValue;

            return View(p);
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Course course)
        {
            if (ModelState.IsValid)
            {
                foreach (CoursePrice bb in course.CoursePrices.ToList())
                {
                    if (bb.DeleteItem == true)
                    {
                        course.CoursePrices.Remove(bb);
                    }

                    if (bb.IsDefault)
                    {
                        course.StudentPrice = bb.StudentPrice;
                        course.PublicPrice = bb.PublicPrice;
                    }
                }

                foreach (CourseBundle bb in course.CourseBundles.ToList())
                {
                    if (bb.DeleteBundle == true || bb.IncCourseName == null)
                    {
                        course.CourseBundles.Remove(bb);
                    }
                }


                //foreach (CourseSchedule bb in course.CourseSchedules.ToList())
                //{
                //    if (bb.DeleteItem == true)
                //    {
                //        course.CourseSchedules.Remove(bb);
                //    }
                //}

                db.Courses.Add(course);
                db.SaveChanges();

                return RedirectToAction("Index");
            };

            ViewBag.Message = "1";

            ViewBag.ProductTypes = db.ProductGroups.ToList();
            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(course);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Course course = db.Courses.Find(id);
            if (course == null)
            {
                return HttpNotFound();
            }

            //foreach (var item in course.CourseSchedules.Where(x => x.ScheduleDate.Value.Date < DateTime.Today ).ToList())
            //{ 
            //        course.CourseSchedules.Remove(item);    
            //}


            ViewBag.ProductTypes = db.ProductGroups.ToList();
            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
 
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Course course)
        {
            if (ModelState.IsValid)
            {
                foreach (var item in course.CoursePrices.ToList())
                {
                    if (item.DeleteItem == true)
                    {
                        CoursePrice cp = db.CoursePrices.Where(x => x.OptionID == item.OptionID).FirstOrDefault();
                        if (cp != null)
                        {
                            db.CoursePrices.Remove(cp);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        db.Entry(item).State = (db.CoursePrices.Any(e => e.OptionID == item.OptionID)) ? EntityState.Modified : EntityState.Added;

                    }

                    if (item.IsDefault)
                    {
                        course.StudentPrice = item.StudentPrice;
                        course.PublicPrice = item.PublicPrice;
                    }

                }

                foreach (var item in course.CourseBundles.ToList())
                {
                    if (item.DeleteBundle == true)
                    {
                        CourseBundle p = db.CourseBundles.Where(x => x.BunleID == item.BunleID && x.IncCourseCode == item.IncCourseCode).FirstOrDefault();
                        if (p != null)
                        {
                            db.CourseBundles.Remove(p);
                            db.SaveChanges();
                        }

                        //     db.Entry(p).State = EntityState.Deleted;
                    }
                    else
                    {
                        db.Entry(item).State = (db.CourseBundles.Any(e => e.IncCourseCode == item.IncCourseCode && e.BunleID == item.BunleID)) ? EntityState.Modified : EntityState.Added;

                    }
                    // db.SaveChanges();
                }

                //foreach (var item in course.CourseSchedules.ToList())
                //{
                //    if (item.DeleteItem == true || string.IsNullOrEmpty(item.ScheduleDate.ToString()) || string.IsNullOrEmpty(item.StartTimeValue) || string.IsNullOrEmpty(item.EndTimeValue))
                //    {
                //        CourseSchedule cs = db.CourseSchedules.Where(x => x.ScheduleID == item.ScheduleID).FirstOrDefault();
                //        if (cs != null)
                //        {
                //            db.CourseSchedules.Remove(cs);
                //            db.SaveChanges();
                //        }
                //    }
                //    else
                //    {
                //        db.Entry(item).State = (db.CourseSchedules.Any(e => e.ScheduleID == item.ScheduleID)) ? EntityState.Modified : EntityState.Added;

                //    }
                //}

                course.ModifiedBy = User.Identity.Name;
                course.ModifiedOn = DateTime.Now;
                db.Entry(course).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Edit", new { id = course.CourseID });
            };

            ViewBag.ProductTypes = db.ProductGroups.ToList();
            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();
            ViewData["TutorsAll"] = db.Tutors.Where(x => x.IsActive == true).OrderBy(x => x.TutorName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(course);
        }

        public ActionResult _DisplayScheduleHistory(int? id, string ch)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //Course course = db.Courses.Find(id);
            //if (course == null)
            //{
            //    return HttpNotFound();
            //}

            var list = db.Schedules.Where(x => x.CourseID == id && x.ScheduleDate >= DateTime.Today).ToList();
            ViewBag.TableNo = 1;
            if (ch == "History")
            {
                list = db.Schedules.Where(x => x.CourseID == id && x.ScheduleDate < DateTime.Today).OrderByDescending(x => x.ScheduleID).ToList();
                ViewBag.TableNo = 2;
            };

            return PartialView(list);
        }

        public ActionResult Pricebook()
        {
            Pricebook pb = new Pricebook();
            pb.CostPrice = 0;
            pb.IsDefault = true;
            pb.IsValid = true;

            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();

            return View(pb);
        }

        [HttpPost]
        //  [ValidateAntiForgeryToken]
        public JsonResult Pricebook([Bind(Include = "PriceID,CourseID,CourseName,CourseCode,CourseType,CourseLevel,CourseDuration,TeacherLevel,OptionName,CostPrice,CostCode,RegisterFee,StudentPrice,PublicPrice,IsDefault,IsValid,Remark,ModifiedBy,ModifiedOn")] Pricebook pb)
        {
            pb.ModifiedBy = User.Identity.Name;
            pb.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                pb.CourseCode = db.Courses.Where(m => m.CourseID == pb.CourseID).FirstOrDefault().CourseCode;
                db.Pricebooks.Add(pb);
                db.SaveChanges();
            }

            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();

            return Json(new { success = true, redirectUrl = Url.Action("Pricebook", "Courses") });


        }

        public ActionResult _DisplayItemList()
        {

            return PartialView();
        }

        public ActionResult _AddItem()
        {
            Pricebook pb = new Pricebook();
            pb.CostPrice = 0;
            pb.IsDefault = true;
            pb.IsValid = true;

            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();

            return PartialView(pb);
        }

        [HttpPost]
      //  [ValidateAntiForgeryToken]
        public JsonResult _AddItem([Bind(Include = "PriceID,CourseID,CourseName,CourseCode,CourseType,OptionName,CostPrice,CostCode,StudentPrice,PublicPrice,IsDefault,IsValid,Remark,ModifiedBy,ModifiedOn")] Pricebook pb)
        {
 
            pb.ModifiedBy = User.Identity.Name;
            pb.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Pricebooks.Add(pb);
                db.SaveChanges();
            }

            ViewData["CoursesAll"] = db.Courses.Where(x => x.IsActive == true).OrderBy(x => x.CourseName).ToList();

            return Json(new { success = true, redirectUrl = Url.Action("Pricebook", "Courses") });


        }


        public ActionResult _PriceList()
        {
            var p = new List<Pricebook>();
            p = db.Pricebooks
                .OrderBy(x => x.CourseName)
                .ToList();

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _PriceList(List<Pricebook> list)
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
                    var det = db.Pricebooks.Find(i.PriceID);
                    if (det != null)
                    {
                        det.Remark = i.Remark;
                        det.CourseID = i.CourseID;
                        det.CourseName = i.CourseName;
                        det.CourseCode = i.CourseCode;
                        det.CourseType = i.CourseType;
                        det.CourseLevel = i.CourseLevel;
                        det.CourseDuration = i.CourseDuration;
                        det.TeacherLevel = i.TeacherLevel;
                        det.OptionName = i.OptionName;
                        det.CostPrice = i.CostPrice;
                        det.CostCode = i.CostCode;
                        det.RegisterFee = i.RegisterFee;
                        det.StudentPrice = i.StudentPrice;
                        det.PublicPrice = i.PublicPrice;
                        det.IsDefault = i.IsDefault;
                        det.IsValid = i.IsValid;
                        det.Remark = i.Remark;
                        det.ModifiedBy = User.Identity.Name;
                        det.ModifiedOn = DateTime.Now;

                    }
                };
                db.SaveChanges();
            };

            return Json(new { success = true, redirectUrl = Url.Action("Pricebook", "Courses") });

        }

        [HttpGet]
        public JsonResult _DelItem(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var det = db.Pricebooks.Find(id);
              //  double position = det.Position;

                db.Entry(det).State = EntityState.Deleted;
                db.SaveChanges();

                trans.Complete();
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }




    }
}