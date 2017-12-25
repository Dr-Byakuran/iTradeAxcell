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
    public class GSTsController : Controller
    {
        private StarDbContext db = new StarDbContext();

        // GET: GSTs
        public ActionResult Index()
        {
            return View(db.GstRate.ToList());
        }

        // GET: GSTs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GST gST = db.GstRate.Find(id);
            if (gST == null)
            {
                return HttpNotFound();
            }
            return View(gST);
        }

        // GET: GSTs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: GSTs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,GstRate")] GST gST)
        {
            if (ModelState.IsValid)
            {
                db.GstRate.Add(gST);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(gST);
        }

        // GET: GSTs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GST gST = db.GstRate.Find(id);
            if (gST == null)
            {
                return HttpNotFound();
            }
            return View(gST);
        }

        // POST: GSTs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,GstRate")] GST gST)
        {
            if (ModelState.IsValid)
            {
                db.Entry(gST).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Edit/1");
            }
            return View(gST);
        }

        // GET: GSTs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GST gST = db.GstRate.Find(id);
            if (gST == null)
            {
                return HttpNotFound();
            }
            return View(gST);
        }

        // POST: GSTs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            GST gST = db.GstRate.Find(id);
            db.GstRate.Remove(gST);
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
