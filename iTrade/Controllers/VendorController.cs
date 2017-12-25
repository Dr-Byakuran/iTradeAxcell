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
    public class VendorController : Controller
    {
        private StarDbContext db = new StarDbContext();

        // GET: Vendors
        public ActionResult Index(string txtFilter)
        {
            var result = db.Vendors.Take(200).ToList();

            if (!string.IsNullOrEmpty(txtFilter))
            {
                result = db.Vendors.Where(x => x.CustName.Contains(txtFilter) || x.AccNo.StartsWith(txtFilter)).Take(200).ToList();
            }

            return View(result);
        }

        // GET: Vendors/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendor Vendor = db.Vendors.Find(id);
            if (Vendor == null)
            {
                return HttpNotFound();
            }
            return View(Vendor);
        }

        // GET: Vendors/Create
        public ActionResult Create()
        {
            VendorViewModel VendorViewModel = new VendorViewModel();
            var c = new Vendor();
            c.IsActive = true;
            c.CreatedBy = User.Identity.Name;
            c.CreatedOn = DateTime.Now;

            var cs = new VendorCreditSetting();
            cs.IsCreditAllowed = false;
            cs.CreatedBy = User.Identity.Name;
            cs.CreatedOn = DateTime.Now;

            VendorViewModel.Vendor = c;
            VendorViewModel.VendorCreditSetting = cs;

            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(VendorViewModel);
        }

        // POST: Vendors/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        //public ActionResult Create([Bind(Include = "CustNO,CustName,Addr1,Addr2,Addr3,PostalCode,Country,PhoneNo,FaxNo,ContactPerson,PrimaryEmail,Website,Group,IsActive,Remark,AssignedTo,SalesPersonID,SalesPersonName,CreatedBy,CreatedOn")] Vendor Vendor)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VendorViewModel VendorViewModel)
        {
            Vendor Vendor = VendorViewModel.Vendor;
            VendorCreditSetting VendorCreditSetting = VendorViewModel.VendorCreditSetting;
            Vendor.CreatedBy = User.Identity.Name;
            Vendor.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Vendors.Add(Vendor);
                db.SaveChanges();

                if (VendorCreditSetting.IsCreditAllowed == true && VendorCreditSetting.CreditLimit > 0 && VendorCreditSetting.PaymentTerms > 0)
                {
                    decimal accountBal = 0, overdueLimit = 0;
                    if (VendorCreditSetting.AccountBalance == null) VendorCreditSetting.AccountBalance = accountBal;
                    if (VendorCreditSetting.OverdueLimit == null) VendorCreditSetting.OverdueLimit = overdueLimit;

                    db.VendorCreditSetting.Add(VendorCreditSetting);
                    VendorCreditSetting.CustNo = Vendor.CustNo;
                    VendorCreditSetting.CreatedBy = User.Identity.Name;
                    VendorCreditSetting.CreatedOn = DateTime.Now;
                    VendorCreditSetting.ModifiedBy = User.Identity.Name;
                    VendorCreditSetting.ModifiedOn = DateTime.Now;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }

            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(VendorViewModel);
        }

        // GET: Vendors/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            VendorViewModel VendorViewModel = new VendorViewModel();
            Vendor Vendor = db.Vendors.Find(id);
            if (Vendor == null)
            {
                return HttpNotFound();
            }
            VendorCreditSetting VendorCreditSetting = new VendorCreditSetting();
            VendorCreditSetting = db.VendorCreditSetting.Where(m => m.CustNo == id).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            if (VendorCreditSetting == null)
            {
                VendorCreditSetting _VendorCreditSetting = new VendorCreditSetting();
                _VendorCreditSetting.IsCreditAllowed = false;

                VendorCreditSetting = _VendorCreditSetting;
            }

            VendorViewModel.Vendor = Vendor;
            VendorViewModel.VendorCreditSetting = VendorCreditSetting;

            ViewBag.Status = "GeneralInfo";
            ViewBag.CustNo = id;

            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(VendorViewModel);
        }

        // POST: Vendors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //public ActionResult ManageVendor([Bind(Include = "CustNO,CustName,Addr1,Addr2,Addr3,PostalCode,Country,PhoneNo,FaxNo,ContactPerson,PrimaryEmail,Website,Group,IsActive,Remark,AssignedTo,SalesPersonID,SalesPersonName,CreatedBy,CreatedOn")] Vendor Vendor)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VendorViewModel VendorViewModel)
        {
            Vendor Vendor = new Vendor();
            VendorCreditSetting VendorCreditSetting = new VendorCreditSetting();
            int custNo = 0;
            if (ModelState.IsValid)
            {
                Vendor = VendorViewModel.Vendor;
                Vendor.ModifiedBy = User.Identity.Name;
                Vendor.ModifiedOn = DateTime.Now;

                db.Entry(Vendor).State = EntityState.Modified;
                db.SaveChanges();

                custNo = Vendor.CustNo;
                //         ViewBag.Status = "GeneralInfo";


                //         string strCustNo = Request.Form["strCustNo"];
                //          custNo = int.Parse(strCustNo);
                VendorCreditSetting = VendorViewModel.VendorCreditSetting;
                VendorCreditSetting _VendorCreditSetting = new VendorCreditSetting();
                List<VendorCreditSetting> VendorCreditSettingList = db.VendorCreditSetting.Where(m => m.CustNo == custNo).OrderByDescending(m => m.ModifiedOn).ToList();
                if (VendorCreditSettingList.Count > 0)
                {
                    _VendorCreditSetting = VendorCreditSettingList.First();
                    _VendorCreditSetting.ModifiedBy = User.Identity.Name;
                    _VendorCreditSetting.ModifiedOn = DateTime.Now;
                    _VendorCreditSetting.CreditLimit = VendorCreditSetting.CreditLimit;
                    _VendorCreditSetting.IsCreditAllowed = VendorCreditSetting.IsCreditAllowed;
                    _VendorCreditSetting.PaymentTerms = VendorCreditSetting.PaymentTerms;
                    _VendorCreditSetting.OverdueLimit = VendorCreditSetting.OverdueLimit;
                    db.Entry(_VendorCreditSetting).State = EntityState.Modified;
                    db.SaveChanges();

                    VendorCreditSetting = _VendorCreditSetting;
                }
                else
                {
                    if (VendorCreditSetting.IsCreditAllowed == true)
                    {
                        db.VendorCreditSetting.Add(VendorCreditSetting);
                        VendorCreditSetting.CustNo = custNo;
                        VendorCreditSetting.CreatedBy = User.Identity.Name;
                        VendorCreditSetting.CreatedOn = DateTime.Now;
                        VendorCreditSetting.ModifiedBy = User.Identity.Name;
                        VendorCreditSetting.ModifiedOn = DateTime.Now;
                        db.SaveChanges();
                    }
                }
                //    ViewBag.Status = "FinancialInfo";

                //return RedirectToAction("Index");
            }

            ViewBag.CustNo = custNo;

            //    Vendor = db.Vendors.Find(custNo);
            //    VendorCreditSetting = db.VendorCreditSetting.Where(m => m.CustNo == custNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            //    VendorViewModel.Vendor = Vendor;
            //    VendorViewModel.VendorCreditSetting = VendorCreditSetting;

            ViewBag.Status = Request.Form["selectedTab"];

            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(VendorViewModel);
        }


        // GET: Vendors/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Vendor Vendor = db.Vendors.Find(id);
            if (Vendor == null)
            {
                return HttpNotFound();
            }
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            return View(Vendor);
        }

        // POST: Vendors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Vendor Vendor = db.Vendors.Find(id);
            db.Vendors.Remove(Vendor);
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