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
    public class PurchasesController : Controller
    {
        private StarDbContext db = new StarDbContext();

        // GET: Purchases
        public ActionResult Index()
        {
            return View(db.Purchases.ToList());
        }

        // GET: Purchases/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Purchase purchase = db.Purchases.Find(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }
            return View(purchase);
        }

        // GET: Purchases/Create
        public ActionResult Create()
        {
            var p = new Purchase();
            p.CreatedBy = User.Identity.Name;
            p.CreatedOn = DateTime.Now;

            ViewData["RawProductData"] = db.Products.Where(x => x.IsActive == true).OrderBy(x => x.ProductName).OrderBy(x => x.ProductName).ToList();
            ViewData["SupplierData"] = db.Suppliers.Where(x => x.IsActive == true).OrderBy(x => x.SupplierName).ToList();

            return View(p);
        }

        // POST: Purchases/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PurID,DateIn,BatchNo,ProductID,ProductCode,ProductType,ProductName,Material,Length,LengthUnit,Thickness,Dimension,Qty,Unit,BuyingPriceORG,Currency,ExRate,BuyingPriceSGD,Weight,UOM,Country,SupplierID,SupplierName,Remark")] Purchase purchase)
        {
            purchase.CreatedBy = User.Identity.Name;
            purchase.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Purchases.Add(purchase);
                db.SaveChanges();

                var p = new StockDet();
                p.ProductID = purchase.ProductID;
                p.SKU = purchase.ProductCode;
  
 
 
                CreateRawStockDet(p);
             //   UpdateRawStock(p.ProductID, p.ProcessType, p.Qty);

                return RedirectToAction("Index");
            }

            return View(purchase);
        }

        private void UpdateRawStock(int id, string processtype, double qty)
        {
            var p = db.Stocks.Find(id);
            if (p != null)
            {
                if (processtype == "IN") {
                    p.StockIn = p.StockIn + qty;
                };
                if (processtype == "OUT")
                {
                   double newqty = 0 - qty;
                   p.StockOut = p.StockOut + newqty;
                };
                if (processtype == "ADJ")
                {
                    p.StockAdjusted = qty;
                };

                p.InStock = p.InitQty + p.StockIn + p.StockOut + p.StockAdjusted;

                if (ModelState.IsValid)
                {
                    db.Entry(p).State = EntityState.Modified;
                    db.SaveChanges();
                };


            }
        }

        private void CreateRawStockDet(StockDet p)
        {
            if (ModelState.IsValid)
            {
                db.StockDets.Add(p);
                db.SaveChanges();
            };


        }

        // GET: Purchases/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Purchase purchase = db.Purchases.Find(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }

            ViewData["RawProductData"] = db.Products.Where(x => x.IsActive == true).OrderBy(x => x.ProductName).OrderBy(x => x.ProductName).ToList();
            ViewData["SupplierData"] = db.Suppliers.Where(x => x.IsActive == true).OrderBy(x => x.SupplierName).ToList();

            return View(purchase);
        }

        // POST: Purchases/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PurID,DateIn,BatchNo,ProductID,ProductCode,ProductType,ProductName,Material,Length,LengthUnit,Thickness,Dimension,Qty,Unit,BuyingPriceORG,Currency,ExRate,BuyingPriceSGD,Weight,UOM,Country,SupplierID,SupplierName,Remark")] Purchase purchase)
        {
            purchase.CreatedBy = User.Identity.Name;
            purchase.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(purchase).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(purchase);
        }

        // GET: Purchases/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Purchase purchase = db.Purchases.Find(id);
            if (purchase == null)
            {
                return HttpNotFound();
            }
            return View(purchase);
        }

        // POST: Purchases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Purchase purchase = db.Purchases.Find(id);
            db.Purchases.Remove(purchase);
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



        public JsonResult AutoComplete_Supplier(string search)
        {
            var data = db.Suppliers
                       .Where(x => ((x.SupplierName.ToUpper().StartsWith(search.ToUpper())) || (x.SupplierID.ToString().StartsWith(search))) && ((x.IsActive == true)))
                       .ToList().Distinct().OrderBy(x => x.SupplierName).ToList();

            //   var result = data.Where(x => x.HeatNo.ToLower().StartsWith(search.ToLower())).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //  [HttpPost]
        public JsonResult AutoCompleteSelected_Supplier(string search)
        {
            if (search != null)
            {
                int custno = Convert.ToInt32(search);
                var c = db.Suppliers
                           .Where(x => x.SupplierID == custno)
                           .ToList().FirstOrDefault();

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


        public JsonResult AutoComplete_Product(string search)
        {
            var data = db.Products
                       .Where(x => ((x.ProductName.ToUpper().StartsWith(search.ToUpper())) || (x.ProductCode.ToUpper().StartsWith(search.ToUpper()))) && ((x.IsActive == true)))
                       .ToList().Distinct().OrderBy(x => x.ProductName).ToList();

            //   var result = data.Where(x => x.HeatNo.ToLower().StartsWith(search.ToLower())).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //  [HttpPost]
        public JsonResult AutoCompleteSelected_Product(string search)
        {
            if (search != null)
            {
                int newid = Convert.ToInt32(search);
                var c = db.Products
                           .Where(x => x.ProductID == newid)
                           .ToList().FirstOrDefault();

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
