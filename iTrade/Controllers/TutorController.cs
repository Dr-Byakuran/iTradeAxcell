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
    public class TutorController : Controller
    {
        private StarDbContext db = new StarDbContext();
        // GET: Tutor
        public ActionResult Index(string txtFilter)
        {
            var result = db.Tutors.Take(200).ToList();

            if (!string.IsNullOrEmpty(txtFilter))
            {
                result = db.Tutors.Where(x => x.TutorName.Contains(txtFilter) || x.TutorCode.StartsWith(txtFilter)).Take(200).ToList();
            }

            return View(result);
        }

        public ActionResult Create()
        {
            Random ro = new Random();
            int iResult;
            iResult = ro.Next();
            ViewBag.No = iResult;

            //ViewBag.SorId = iResult;
            //ViewBag.SorNo = iResult;
            //CourseViewModel sv = new CourseViewModel();
            //var c = new Course();
            //c.IsActive = true;
            //c.CreatedBy = User.Identity.Name;
            //c.CreatedOn = DateTime.Now;

            //var cs = new CourseSchoolType();
            //cs.CreatedBy = User.Identity.Name;
            //cs.CreatedOn = DateTime.Now;

            //sv.Course = c;
            //sv.CourseSchoolType = cs;
            //return View(sv);
            ViewData["CourseAll"] = db.Courses.Where(x => x.IsActive == true).ToList();

            
            Tutor stu = new Tutor();
            stu.CreatedBy = User.Identity.Name;
            stu.CreatedOn = DateTime.Now;
            stu.IsActive = true;
            return View(stu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Tutor sg)
        {
            //string time = sg.DateOfBirth.ToString();
            //string stime = time.Split('/')[2] + "-" + time.Split('/')[1] + "-" + time.Split('/')[0];
            //sg.DateOfBirth = Convert.ToDateTime(stime);
            if (ModelState.IsValid)
            {
                sg.TutorType = (Request["TutorType"] == null) ? "" : Request["TutorType"].ToString();
                sg.JobType= (Request["JobType"] == null) ? "" : Request["JobType"].ToString();
                sg.Gender = (Request["Gender"] == null) ? "" : Request["Gender"].ToString();

                sg.Subjects = (Request["CourseSubjects"] != null) ? Request["CourseSubjects"].ToString() : "";
                string zhi = "";
                string categoryid = "";
                List<Course> cse = db.Courses.Where(x => x.IsActive == true).ToList();
                if(Request["CourseSubjects"]!=null)
                {
                    string[] val = Request["CourseSubjects"].ToString().Split(',');
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
                }

                sg.AttId = (Request["AttachmenNo"] == null) ? 0 : Convert.ToInt32(Request["AttachmenNo"].ToString());
                sg.SubjectsName = zhi;
                sg.CategoryID = categoryid;
                sg.CreatedBy = User.Identity.Name;
                sg.CreatedOn = DateTime.Now;
                sg.ModifiedBy = User.Identity.Name;
                sg.ModifiedOn = DateTime.Now;
                db.Tutors.Add(sg);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewData["CourseAll"] = db.Courses.Where(x => x.IsActive == true).ToList();
            return View(sg);
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
                tu.JobType= (Request["JobType"] == null) ? "" : Request["JobType"].ToString();
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

        public ActionResult _SelCourse()
        {
            var p = new List<Course>();
            p = db.Courses.ToList();

            return PartialView(p);
        }

        //public void Excel()
        //{
        //    ToExcel Excel = new ToExcel();
        //    DataTable dt = new DataTable();
        //    string str = System.Configuration.ConfigurationManager.ConnectionStrings["StarConnection"].ConnectionString;
        //    SqlConnection sqlcon = new SqlConnection(str);
        //    sqlcon.Open();
        //    string strsql = "select ROW_NUMBER() over(order by TutorID) as No,TutorName,Gender,SubjectsName as Subjects,Qualification,MobileNo,JobType,JoinedDate,IsActive  from [dbo].[Tutors] where IsActive='True'";
        //    SqlDataAdapter da = new SqlDataAdapter(strsql, sqlcon);
        //    DataSet ds = new DataSet();
        //    da.Fill(ds);
        //    dt = ds.Tables[0];
        //    Excel.Excel(dt);
        //    sqlcon.Close();
        //    Response.End();
        //}

        public PartialViewResult UploadAttachment(int? id, string Act)
        {
            Attachment att = db.Attachment.FirstOrDefault(p => p.AttachmentId == id);
            Attachment attach = null;
            if (att != null) attach = db.Attachment.Find(att.Id);
            ViewBag.Act = Act;
            ViewBag.SorId = id;
            ViewBag.SorNo = id;
            return PartialView(attach);
        }

        public static object _lock = new object();
        public JsonResult ProccessUploadAttachments(int Id, string No)
        {
            string absolutePath = "";
            if (Request.Files.Count == 0) return Json(new { files = "" }, JsonRequestBehavior.AllowGet);
            string uploadfileformat = System.Web.Configuration.WebConfigurationManager.AppSettings["UploadfileFormart"] ?? "";
            //List<string> lstExtension = new List<string> { ".ppt", ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".doc" };
            string[] lstExtension = uploadfileformat.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            List<string> result = new List<string>();
            IEnumerable<string> distinctResult = result;
            foreach (string key in Request.Files.AllKeys)
            {
                HttpPostedFileBase file = Request.Files[key];
                if (!lstExtension.Any(p => file.FileName.Contains(p))) continue;
                try
                {
                    System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(Server.MapPath("/Attachments/") + No);
                    if (!info.Exists)
                    {
                        info.Create();
                    }
                    //string relativePath = quo.QuoID+"/" + file.FileName; ;
                    absolutePath = Server.MapPath("/Attachments/") + No + "/" + file.FileName;
                    file.SaveAs(absolutePath);
                    result.Add(file.FileName);
                }
                catch (Exception err)
                {
                    var e = err;
                }
            }
            string AttachmentTypea;
            string Path;
            AttachmentTypea = string.Join(";", result.Select(p => p.Substring(p.LastIndexOf('.'))));
            Path = string.Join(";", result);
            lock (_lock)
            {
                Attachment att = new Attachment();
                if (result.Count > 0)
                {

                    distinctResult = result.Distinct();//去掉重复记录，覆盖重复文件
                    distinctResult.ToList().ForEach(p => db.FileManagers.Add(new Models.FileManager { BusinessType = "SO", No = No, LoginName = User.Identity.Name, Path = p }));
                    List<Attachment> at = db.Attachment.Where(p => p.AttachmentId == Id).ToList();
                    if (at.Count > 0)
                    {
                        IEnumerable<string> lst = at[0].Path.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        distinctResult = distinctResult.Except(lst);

                        att = db.Attachment.Find(at[0].Id);
                        att.AttachmentType += ";" + AttachmentTypea;
                        att.Path += ";" + Path;
                        db.Entry(att).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        att = new Attachment();
                        att.AttachmentId = Id;
                        att.AttachmentType = AttachmentTypea;
                        att.Path = Path;
                        db.Attachment.Add(att);
                        db.SaveChanges();
                    }





                    //if (AttachmentTypea == "" || AttachmentTypea == null)
                    //{
                    //    AttachmentTypea = string.Join(";", result.Select(p => p.Substring(p.LastIndexOf('.'))));
                    //    Path = string.Join(";", result);
                    //}
                    //else
                    //{
                    //    AttachmentTypea += "," + string.Join(";", result.Select(p => p.Substring(p.LastIndexOf('.'))));
                    //    Path += "," + string.Join(";", result);
                    //}

                }
                //ViewData["Type"] = AttachmentTypea;
                //ViewData["Path"] = Path;
                //Attachment att = new Attachment();
                //att.AttachmentId = Id;
                //if (result.Count > 0)
                //{



                //    Tutor tu = db.Tutors.Include(p => p.Attachment).FirstOrDefault(p => p.TutorID == Id);
                //    //SalesOrder order = db.SalesOrders.Include(p => p.Attachment).FirstOrDefault(p => p.SorID == Id);
                //    distinctResult = result.Distinct();//去掉重复记录，覆盖重复文件
                //    distinctResult.ToList().ForEach(p => db.FileManagers.Add(new Models.FileManager { BusinessType = "SO", No = No, CustomerName = tu.TutorName, LoginName = User.Identity.Name, Path = p }));
                //    if (tu.Attachment == null)
                //    {
                //        tu.Attachment = new Attachment { AttachmentType = string.Join(";", result.Select(p => p.Substring(p.LastIndexOf('.')))), Path = string.Join(";", result) };
                //        db.Entry(tu).State = EntityState.Modified;
                //    }
                //    else
                //    {
                //        IEnumerable<string> lst = tu.Attachment.Path.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                //        distinctResult = distinctResult.Except(lst);
                //        lst = lst.Union(result);
                //        tu.Attachment.AttachmentType = string.Join(";", lst.Select(p => p.Substring(p.LastIndexOf('.'))).Distinct());
                //        tu.Attachment.Path = string.Join(";", lst);
                //    }
                //    db.SaveChanges();
                //}
            }
            return Json(new { files = distinctResult }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _RemoveAttachmentById(int sordId, int index)
        {
            string id = sordId.ToString();
            Attachment att = db.Attachment.FirstOrDefault(p => p.AttachmentId == sordId);
            //SalesOrder order = db.SalesOrders.Include(p => p.Attachment).FirstOrDefault(p => p.SorID == sordId);
            if (att != null)
            {
                List<string> paths = att.Path.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (paths.Count <= 1)
                {
                    try
                    {
                        System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(Server.MapPath("/Attachments/") + sordId);
                        if (info.Exists)
                        {
                            info.Delete(true);
                        }
                    }
                    catch (Exception err)
                    {
                        var e = err;
                    }
                    //quo.Attachment = null;
                    att.Path = "";
                    att.AttachmentType = "";

                    db.FileManagers.RemoveRange(db.FileManagers.Where(p => p.BusinessType == "SO" && p.No == id));
                }
                else
                {
                    try
                    {
                        if (System.IO.File.Exists(Server.MapPath("/Attachments/") + sordId + "/" + paths[index]))
                        {
                            string val = paths[index];
                            FileManager file = db.FileManagers.FirstOrDefault(p => p.BusinessType == "SO" && p.No == id && p.Path == val);
                            if (file != null) db.FileManagers.Remove(file);
                            System.IO.File.Delete(Server.MapPath("/Attachments/") + sordId + "/" + paths[index]);
                        }
                    }
                    catch (Exception err)
                    {
                        var e = err;
                    }
                    paths.RemoveAt(index);
                    att.AttachmentType = string.Join(";", paths.Select(p => p.Substring(p.LastIndexOf('.'))));
                    att.Path = string.Join(";", paths);
                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception err)
                {
                    throw err;
                }
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        //public JsonResult _RemoveAttachmentById(int sordId, int index)
        //{
        //    SalesOrder order = db.SalesOrders.Include(p => p.Attachment).FirstOrDefault(p => p.SorID == sordId);
        //    if (order.Attachment != null)
        //    {
        //        List<string> paths = order.Attachment.Path.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
        //        if (paths.Count <= 1)
        //        {
        //            try
        //            {
        //                System.IO.DirectoryInfo info = new System.IO.DirectoryInfo(Server.MapPath("/Attachments/") + order.SorNo);
        //                if (info.Exists)
        //                {
        //                    info.Delete(true);
        //                }
        //            }
        //            catch (Exception err)
        //            {
        //                var e = err;
        //            }
        //            //quo.Attachment = null;
        //            order.Attachment.Path = "";
        //            order.Attachment.AttachmentType = "";
        //            db.FileManagers.RemoveRange(db.FileManagers.Where(p => p.BusinessType == "SO" && p.No == order.SorNo));
        //        }
        //        else
        //        {
        //            try
        //            {
        //                if (System.IO.File.Exists(Server.MapPath("/Attachments/") + order.SorNo + "/" + paths[index]))
        //                {
        //                    FileManager file = db.FileManagers.FirstOrDefault(p => p.BusinessType == "SO" && p.No == order.SorNo && p.Path == paths[index]);
        //                    if (file != null) db.FileManagers.Remove(file);
        //                    System.IO.File.Delete(Server.MapPath("/Attachments/") + order.SorNo + "/" + paths[index]);
        //                }
        //            }
        //            catch (Exception err)
        //            {
        //                var e = err;
        //            }
        //            paths.RemoveAt(index);
        //            order.Attachment.AttachmentType = string.Join(";", paths.Select(p => p.Substring(p.LastIndexOf('.'))));
        //            order.Attachment.Path = string.Join(";", paths);
        //        }
        //        try
        //        {
        //            db.SaveChanges();
        //        }
        //        catch (Exception err)
        //        {
        //            throw err;
        //        }
        //    }
        //    return Json(true, JsonRequestBehavior.AllowGet);
        //}
    }
}