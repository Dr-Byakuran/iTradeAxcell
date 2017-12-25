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

namespace iTrade.Controllers
{
    public class ProductsController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();
        private int DefaultWarehouseId = 1;

        // GET: Products
        public ActionResult Index(string txtSearch)
        {
            var result = db.Products.Take(200).ToList();

            if (!string.IsNullOrEmpty(txtSearch))
            {
                result = db.Products.Where(x => x.ProductName.Contains(txtSearch.Trim()) || x.SKU.StartsWith(txtSearch.Trim()) || x.ModelNo.StartsWith(txtSearch.Trim())).Take(200).ToList();
            }
            
            return View(result);
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }


        // GET: Products/Create
        public ActionResult Create()
        {
            var p = new Product();

            p.IsControlItem = true;
            p.ManageStock = true;
            p.IsBundle = false;
            p.IsFeatured = false;
            p.IsActive = true;
            p.CreatedBy = User.Identity.Name;
            p.CreatedOn = DateTime.Now;

            p.CreatePriceOptions(1);
            p.CreatePricebreaks(1);
            p.CreateProductbundles(1);
            p.CreateProductFOCs(1);

            ViewBag.ProductTypes = db.ProductGroups.ToList();
            ViewData["SuppliersAll"] = db.Suppliers.Where(x => x.IsActive == true).OrderBy(x => x.SupplierName).ToList();
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
        public ActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                foreach (PriceOption bb in product.PriceOptions.ToList())
                {
                    if (bb.DeleteItem == true)
                    {
                        product.PriceOptions.Remove(bb);
                    }

                    if (bb.IsDefault)
                    {
                        product.CostCode = bb.CostCode;
                        product.CostPrice = bb.CostPrice;
                        product.RetailPrice = bb.RetailPrice;
                        product.WholesalePrice = bb.WholesalePrice;
                        product.DealerPrice = bb.DealerPrice;
                    }
                }

                foreach (Pricebreak bb in product.Pricebreaks.ToList())
                {
                    if (bb.DeleteItem == true)
                    {
                        product.Pricebreaks.Remove(bb);
                    }
                }
 
                foreach (Productbundle bb in product.Productbundles.ToList())
                {
                    if (bb.DeleteBundle == true || bb.IncSKU == null || bb.IncSKU == "")
                    {
                        product.Productbundles.Remove(bb);          
                    }
                }

                foreach (ProductFOC bb in product.ProductFOCs.ToList())
                {
                    if (bb.DeleteFOC == true || bb.IncSKU == null || bb.IncSKU == "")
                    {
                        product.ProductFOCs.Remove(bb);
                    }
                }

                db.Products.Add(product);
                db.SaveChanges();

                CreateStock(product);

                return RedirectToAction("Index");
            };

            ViewBag.Message = "1";

            ViewBag.ProductTypes = db.ProductGroups.ToList();
            ViewData["SuppliersAll"] = db.Suppliers.Where(x => x.IsActive == true).OrderBy(x => x.SupplierName).ToList();

            return View(product);
        }

        private void CreateStock(Product product)
        {
            var p = new Stock();
            p.ProductID = product.ProductID;
            p.SKU = product.SKU;
            p.ProductName = product.ProductName;
            p.Barcode = product.Barcode;
            p.InitQty = product.Qty;
            p.StockIn = 0;
            p.StockOut = 0;
            p.StockAdjusted = 0;
            p.InStock = product.Qty;
            p.Allocated = 0;
            p.OnHand = 0;
            p.OnOrder = 0;
            p.OnKiv = 0;
            p.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Stocks.Add(p);
                db.SaveChanges();

            };

            CreateStockDet(product);
         //   CreateStockTransaction(product);
        }

        private void CreateStockDet(Product p)
        {
            var det = new StockDet();
            det.ProductID = p.ProductID;
            det.SKU = p.SKU;
            det.ProductName = p.ProductName;
            det.Barcode = p.Barcode;
            det.LocationID = DefaultWarehouseId;
            det.InitQty = p.Qty;
            det.StockIn = 0;
            det.StockOut = 0;
            det.StockAdjusted = 0;
            det.InStock = p.Qty;
            det.Allocated = 0;
            det.OnHand = 0;
            det.OnOrder = 0;
            det.OnKiv = 0;
            det.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.StockDets.Add(det);
                db.SaveChanges();                

            };

            // add product to default location ID 1
            CreateWarehouseStock(p, DefaultWarehouseId);

        }

        private void CreateWarehouseStock(Product p, int locationid)
        {
            var det = new WarehouseStock();
            det.LocationID = locationid;
            det.ProductID = p.ProductID;
            det.SKU = p.SKU;
            det.InitQty = p.Qty;
            det.StockIn = 0;
            det.StockOut = 0;
            det.StockAdjusted = 0;
            det.InStock = p.Qty;
            det.Allocated = 0;
            det.OnHand = 0;
            det.OnOrder = 0;
            det.OnKiv = 0;
            det.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.WarehouseStocks.Add(det);
                db.SaveChanges();

            };
        }

        private void CreateStockTransaction(Product p)
        {
            var st = new StockTransaction();
            st.ProductID = p.ProductID;
            st.SKU = p.SKU;
            st.LocationID = DefaultWarehouseId;
            st.ProcessType = "In";
            st.Qty = p.Qty;
            st.RefNo = "";
            st.SourceFrom = "New product";
            st.Remark = "";

            st.CreatedBy = User.Identity.Name;
            st.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.StockTransactions.Add(st);
                db.SaveChanges();

            };
        }


        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProductTypes = db.ProductGroups.ToList();
            ViewData["SuppliersAll"] = db.Suppliers.Where(x => x.IsActive == true).OrderBy(x => x.SupplierName).ToList();
            var cc = db.CustomSettings.Where(x => x.Name == "CostCode").FirstOrDefault();
            if (cc == null)
                ViewBag.CostCodeSetting = "";
            else
                ViewBag.CostCodeSetting = cc.TextValue;

            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Product product)
        {
            int pid = product.ProductID;

            if (ModelState.IsValid)
            {
             //   db.Products.Attach(product);

                //foreach (var item in product.PriceOptions.ToList())
                //{
                //    db.Entry(item).State = EntityState.Modified;
                //}

                //foreach (var item in product.Pricebreaks.ToList())
                //{
                //    db.Entry(item).State = EntityState.Modified;
                //}


                foreach (var item in product.PriceOptions.ToList())
                {
                    if (item.DeleteItem == true)
                    {
                        PriceOption p = db.PriceOptions.Where(x => x.OptionID == item.OptionID).FirstOrDefault();
                        if (p != null) {
                            db.PriceOptions.Remove(p);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        db.Entry(item).State = (db.PriceOptions.Any(e => e.OptionID == item.OptionID)) ? EntityState.Modified : EntityState.Added;

                    }

                    if (item.IsDefault)
                    {
                        product.CostCode = item.CostCode;
                        product.CostPrice = item.CostPrice;
                        product.RetailPrice = item.RetailPrice;
                        product.WholesalePrice = item.WholesalePrice;
                        product.DealerPrice = item.DealerPrice;
                    }

                }

                foreach (var item in product.Pricebreaks.ToList())
                {
                    if (item.DeleteItem == true)
                    {
                        Pricebreak p = db.Pricebreaks.Where(x => x.PriceID == item.PriceID).FirstOrDefault();
                        if (p != null)
                        {
                            db.Pricebreaks.Remove(p);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        db.Entry(item).State = (db.Pricebreaks.Any(e => e.PriceID == item.PriceID)) ? EntityState.Modified : EntityState.Added;

                    }
                }

 
                foreach (var item in product.Productbundles.ToList())
                {
                    if (item.DeleteBundle == true)
                    {
                        Productbundle p = db.Productbundles.Where(x => x.BunleID == item.BunleID && x.IncSKU == item.IncSKU).FirstOrDefault();
                        if (p != null)
                        {
                            db.Productbundles.Remove(p);
                            db.SaveChanges();
                        }

                     //     db.Entry(p).State = EntityState.Deleted;
                    }
                    else
                    {
                        db.Entry(item).State = (db.Productbundles.Any(e => e.IncSKU == item.IncSKU && e.BunleID == item.BunleID)) ? EntityState.Modified : EntityState.Added;

                    }
                   // db.SaveChanges();
                }
 
                foreach (var item in product.ProductFOCs.ToList())
                {
                    if (item.DeleteFOC == true)
                    {
                        ProductFOC p = db.ProductFOCs.Where(x => x.FocID == item.FocID && x.IncSKU == item.IncSKU).FirstOrDefault();
                        if (p != null)
                        {
                            db.ProductFOCs.Remove(p);
                            db.SaveChanges();
                        }

                        //  db.Entry(p).State = EntityState.Deleted;
                    }
                    else
                    {
                        db.Entry(item).State = (db.ProductFOCs.Any(e => e.IncSKU == item.IncSKU && e.FocID == item.FocID)) ? EntityState.Modified : EntityState.Added;

                    }

                }
 
                product.ModifiedBy = User.Identity.Name;
                product.ModifiedOn = DateTime.Now;
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();

                
                return RedirectToAction("Edit", new { id = product.ProductID });
            }

            ViewBag.ProductTypes = db.ProductGroups.ToList();
            ViewData["SuppliersAll"] = db.Suppliers.Where(x => x.IsActive == true).OrderBy(x => x.SupplierName).ToList();

            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProductTypes = db.ProductGroups.ToList();
            ViewData["SuppliersAll"] = db.Suppliers.Where(x => x.IsActive == true).OrderBy(x => x.SupplierName).ToList();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
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

        public ActionResult _AddBundleItem(int id)
        {
            var p = new INVDET();
            p.SorID = id;

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();

            return PartialView(p);
        }


        //public List<ProductSelection> GetProductList()
        //{
        //    List<ProductSelection> getList = (from p in db.Products
        //                                      where p.IsActive == true
        //                                      select new ProductSelection
        //                                      {
        //                                          ProductID = p.ProductID,
        //                                          SKU = p.SKU,
        //                                          Barcode = p.Barcode,
        //                                          ProductType = p.ProductType,
        //                                          ProductName = p.ProductName,
        //                                          ModelNo = p.ModelNo,
        //                                          IsBundle = p.IsBundle,
        //                                          Unit = p.Unit,
        //                                          CostPrice = p.CostPrice,
        //                                          SellPrice = p.RetailPrice,
        //                                          IsControlItem = p.IsControlItem,
        //                                          AvailableQty = 0

        //                                      }).ToList();

        //    return getList;

        //}

        private double GetOnHandQty(int pid)
        {
            // to get product available qty or OnHand Qty

            return 0d;
        }


        //[HttpGet]
        //public JsonResult AutoComplete_Product(string search)
        //{
        //    var getList = GetProductList();
        //    var data = getList;

        //    string[] words = search.ToUpper().Split(' ').ToArray();

        //    //      var results = from p in getList
        //    //                          .Select(x => x.ProductName.ToUpper())
        //    //                          .Where(x => words.All(y => x.Contains(y)))
        //    //                    select p;



        // //   var result = from p in getList
        // //                let w = p.ProductName.ToUpper().Split(new char[] { '.', '-', '!', ' ', ';', ':', ',' },
        // //                                           StringSplitOptions.RemoveEmptyEntries)
        // //                where (w.Distinct().Intersect(words).Count() == words.Count()) || (p.SKU.ToUpper().StartsWith(search.ToUpper()))
        // //                select p;

        //    var result = from p in getList
        //                 let w = p.ProductName.ToUpper().Split(new char[] { '.', '-', '!', ' ', ';', ':', ',' },
        //                                            StringSplitOptions.RemoveEmptyEntries)
        //                 where (w.Distinct().Intersect(words).Count() == words.Count()) ||
        //                 ((p.SKU.ToUpper().Contains(search.ToUpper())) ||
        //                 (p.ProductName.ToUpper().Contains(search.ToUpper())))
        //                 select p;

        //    data = result.ToList();



        //    if (data == null)
        //    {
        //        return null;
        //    }
        //    else
        //    {
        //        return Json(data.Take(100).ToList(), JsonRequestBehavior.AllowGet);
        //    }


        //}

        ////  [HttpPost]
        //public JsonResult AutoCompleteSelected_Product(string search)
        //{
        //    if (search != null)
        //    {
        //        var getList = GetProductList();

        //        int newid = Convert.ToInt32(search);
        //        var c = getList.Where(x => x.ProductID == newid).FirstOrDefault();

        //        if (c != null)
        //        {

        //            return Json(new { result = c }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return null;
        //        };

        //    }
        //    else
        //    {
        //        return null;
        //    }

        //}

        public ActionResult _DisplayStockTransaction(int id)
        {
            var p = new List<StockTransaction>();

            p = db.StockTransactions.Where(x => x.ProductID == id).OrderByDescending(x => x.CreatedOn).ToList();

            return PartialView(p);
        }

        public ActionResult _DisplaySalesList(int id)
        {
            var p = (from t1 in db.INVs
                 join t2 in db.INVDETs on t1.InvID equals t2.InvID
                 where t2.ItemID == id
                 select new
                 {
                     a = t1,
                     b = t2
                 }).ToList().Select(x => new INVDETSelection()
                 {
                     InvNO = x.a.InvID,
                     InvDate = x.a.InvDate,
                     CustName = x.a.CustName,
                     Qty = x.b.Qty,
                     Nett = x.b.Nett
                 });

            return PartialView(p);
        }
    }
}
