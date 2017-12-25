using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using iTrade.Models;
using System.IO;
using iTrade.CustomAttributes;

namespace iTrade.Controllers
{
    public class CompaniesController : Controller
    {
        private StarDbContext db = new StarDbContext();
        private CustomErrorMessage _CE = new CustomErrorMessage();

        // GET: Companies
        public ActionResult Index()
        {
            return View(db.Companies.ToList());
        }

        // GET: Companies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // GET: Companies/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,ChineseName,Address,TelephoneNumber,FaxNumber,EmailAddress,BusinessRegNo,GSTRegNo,LogoImage")] Company company, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    if (upload.ContentLength > 512000)
                    {
                        TempData["errormessage"] = _CE.PostMessage("The file size is too big. Please upload the file that is less than 500KB.", true);
                        return View(company);
                    }

                    if (Path.GetExtension(upload.FileName).ToLower() != ".jpg"
                        && Path.GetExtension(upload.FileName).ToLower() != ".png"
                        && Path.GetExtension(upload.FileName).ToLower() != ".gif"
                        && Path.GetExtension(upload.FileName).ToLower() != ".jpeg")
                    {
                        TempData["errormessage"] = _CE.PostMessage("Please upload the image (.JPG, .PNG, .GIF or .JPEG are allowed).", true);
                        return View(company);
                    }

                    byte[] content;
                    using (var reader = new System.IO.BinaryReader(upload.InputStream))
                    {
                        content = reader.ReadBytes(upload.ContentLength);
                    }
                    company.LogoImage = content;
                }
                company.CreatedBy = User.Identity.Name;
                company.CreatedOn = DateTime.Now;
                db.Companies.Add(company);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(company);
        }

        // GET: Companies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name,ChineseName,Address,TelephoneNumber,FaxNumber,EmailAddress,BusinessRegNo,GSTRegNo,LogoImage,CreatedOn")] Company company, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                if (upload != null && upload.ContentLength > 0)
                {
                    if (upload.ContentLength > 512000)
                    {
                        TempData["errormessage"] = _CE.PostMessage("The file size is too big. Please upload the file that is less than 500KB.", true);
                        return View(company);
                    }

                    if (Path.GetExtension(upload.FileName).ToLower() != ".jpg"
                        && Path.GetExtension(upload.FileName).ToLower() != ".png"
                        && Path.GetExtension(upload.FileName).ToLower() != ".gif"
                        && Path.GetExtension(upload.FileName).ToLower() != ".jpeg")
                    {
                        TempData["errormessage"] = _CE.PostMessage("Please upload the image (.JPG, .PNG, .GIF or .JPEG are allowed).", true);
                        return View(company);
                    }

                    byte[] content;
                    using (var reader = new System.IO.BinaryReader(upload.InputStream))
                    {
                        content = reader.ReadBytes(upload.ContentLength);
                    }
                    company.LogoImage = content;
                }
                company.ModifiedBy = User.Identity.Name;
                company.ModifiedOn = DateTime.Now;
                db.Entry(company).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(company);
        }

        // GET: Companies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Company company = db.Companies.Find(id);
            db.Companies.Remove(company);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
