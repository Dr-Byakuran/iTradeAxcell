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
    public class InventoryController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();

        // GET: Inventory
        public ActionResult Index(string txtSearch)
        {
            var result = db.Products.Take(100).ToList();

            if (!string.IsNullOrEmpty(txtSearch))
            {
                result = db.Products.Where(x => x.ProductName.Contains(txtSearch.Trim()) || x.SKU.StartsWith(txtSearch.Trim()) || x.ModelNo.StartsWith(txtSearch.Trim())).Take(200).ToList();
            }

            var sv = new List<StockView>();

            if (result != null && result.Count > 0)
            {
                foreach (var p in result)
                {
                    var pv = db.Stocks.Where(x => x.ProductID == p.ProductID).FirstOrDefault();
                    if (pv != null)
                    {
                        var stockview = new StockView();
                        stockview.Stock = pv;
                        stockview.Product = p;

                        sv.Add(stockview);
                    }
                };
            };

            return View(sv);
        }

        public ActionResult StockTake()
        {
            var stt = db.StockTakes.ToList();

            return View(stt);
        }


        // GET: Inventory/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Inventory/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "StockID,ProductID,ProductCode,ProductType,ProductName,Material,Length,LengthUnit,StartQty,StockIn,StockOut,StockAdjusted,StockBalance,Unit,Remark,StartedOn")] Stock rawStock)
        {
            if (ModelState.IsValid)
            {
                db.Stocks.Add(rawStock);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(rawStock);
        }

        // GET: Inventory/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            };

            Stock stock = db.Stocks.Find(id);
            if (stock == null)
            {
                return HttpNotFound();
            }

            var product = db.Products.Where(x => x.ProductID == stock.ProductID).FirstOrDefault();

            if (product == null) {
                return HttpNotFound();
            }

            var stockview = new StockView();
            stockview.Stock = stock;
            stockview.Product = product;

            var sv = new List<StockView>();

            return View(stockview);
        }

        // POST: Inventory/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StockView rawStock)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rawStock).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(rawStock);
        }

        // GET: Inventory/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Stock rawStock = db.Stocks.Find(id);
            if (rawStock == null)
            {
                return HttpNotFound();
            }
            return View(rawStock);
        }

        // POST: Inventory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Stock rawStock = db.Stocks.Find(id);
            db.Stocks.Remove(rawStock);
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

        public ActionResult _DisplayStockLocations(int id)
        {
            var p = new List<WarehouseStock>();
            p = db.WarehouseStocks
                .Where(x => (x.ProductID == id))
                .OrderBy(x => x.LocationID)
                .ToList();

            ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _DisplayStockHistory(int id)
        {
            var p = new List<StockTransaction>();
            p = db.StockTransactions
                .Where(x => (x.ProductID == id))
                .OrderByDescending(x => x.TID)
                .ToList();

            ViewBag.QuoteNumber = id;

            return PartialView(p);
        }


        public ActionResult StockTakeNew()
        {
            var st = new StockTake();
            st.CreatedBy = User.Identity.Name;
            st.CreatedOn = DateTime.Now;

            var getList = GetProductList();

            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            ViewData["ProductData"] = getList;

            return View(st);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StockTakeNew([Bind(Include = "SttID,SttName,SttDate,LocationID,LocationName,ItemCount,Status,Remark,CreatedBy,CreatedOn")] StockTake stt)
        {
            if (ModelState.IsValid)
            {
                db.StockTakes.Add(stt);
                db.SaveChanges();

                CreateStockTakeDets(stt.SttID, stt.LocationID);

                return RedirectToAction("StockTakeEdit", new { id = stt.SttID });
            }


            var getList = GetProductList();

            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            ViewData["ProductData"] = getList;

            return View(stt);
        }

        public ActionResult StockTakeEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StockTake stt = db.StockTakes.Find(id);
            if (stt == null)
            {
                return HttpNotFound();
            }
            var getList = GetProductList();

            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            ViewData["ProductData"] = getList;

            return View(stt);
        }


        private void CreateStockTakeDets(int sttid, int locationid)
        {
            var products = db.Products.Where(x => x.IsActive == true).OrderBy(x => x.ProductName).ToList();

            foreach (var p in products)
            {
                var std = new StockTakeDet();
                std.SttID = sttid;
                std.LocationID = locationid;
                std.ProductID = p.ProductID;
                std.SKU = p.SKU;
                std.ProductName = p.ProductName;
                std.Barcode = p.Barcode;
                std.InStock = 0;
                std.Allocated = 0;
                std.OnHand = 0;
                std.OnKiv = 0;
                std.StockTakeQty = 0;
                std.DifferentQty = 0;

                if (ModelState.IsValid)
                {
                    db.StockTakeDets.Add(std);
                    db.SaveChanges();
 
                }


            }
            


        }

        public ActionResult _DisplayResults(int id)
        {
            var p = new List<StockTakeDet>();

            p = db.StockTakeDets.Where(x => x.SttID ==  id).OrderBy(x => x.ProductName).ToList();

            return PartialView(p);
        }


        public JsonResult _StockTakeSave(string valProductID, string valStockTakeQty, string valSttID, string valLocationID)
        {
            int pid = Convert.ToInt32(valProductID);
            double qty = Convert.ToDouble(valStockTakeQty);
            int sid = Convert.ToInt32(valSttID);
            int lid = Convert.ToInt32(valLocationID);

            var std = db.StockTakeDets.Where(x => x.ProductID == pid && x.SttID == sid && x.LocationID == lid).FirstOrDefault();

            if (std != null)
            {   
                std.StockTakeQty = qty;

                if (ModelState.IsValid)
                {
                    db.Entry(std).State = EntityState.Modified;
                    db.SaveChanges();

                        //   return Json(new { redirectUrl = Url.Action("Edit", "Products", new { id = pid }) });

                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }





        public ActionResult StockTransfer()
        {
            var getList = GetProductList();
 
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            ViewData["ProductData"] = getList;

            return View();
        }

        public JsonResult _Transfer( string valDate, string valVariantID, string valQty, string valFromLocationID, string valToLocationID)
        {
            DateTime txtDate = Convert.ToDateTime(valDate);
            int txtVariantID = Convert.ToInt32(valVariantID);
            double txtQty = Convert.ToDouble(valQty);
            int txtLocationIDFrom = Convert.ToInt32(valFromLocationID);
            int txtLocationIDTo = Convert.ToInt32(valToLocationID);


            var p = db.Products.Where(x => x.ProductID == txtVariantID).FirstOrDefault();

            if (p != null)
            {
                // add to destination location

                var st = new StockTransaction();
                st.ProductID = p.ProductID;
                st.SKU = p.SKU;
                st.LocationID = txtLocationIDTo;
                st.ProcessType = "IN";
                st.Qty = txtQty;
                st.RefNo = txtLocationIDFrom.ToString();
                st.SourceFrom = "Stock Transfer";
                st.Remark = txtDate.ToShortDateString();

                st.CreatedBy = User.Identity.Name;
                st.CreatedOn = DateTime.Now;

                CreateStockTransaction(st);
                UpdateWarehouseStock(st.LocationID, st.ProductID, st.Qty, st.ProcessType);
                UpdateStockDet(st.ProductID, st.Qty, st.ProcessType);
                UpdateStock(st.ProductID, st.Qty, st.ProcessType);


                // deduct from source

                st.LocationID = txtLocationIDFrom;
                st.ProcessType = "OUT";
                st.Qty = 0 - txtQty;
                st.RefNo = txtLocationIDTo.ToString();

                CreateStockTransaction(st);
                UpdateWarehouseStock(st.LocationID, st.ProductID, st.Qty, st.ProcessType);
                UpdateStockDet(st.ProductID, st.Qty, st.ProcessType);
                UpdateStock(st.ProductID, st.Qty, st.ProcessType);

            }
            
            ViewBag.Message = "Success";

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }




        private void CreateStockTransaction(StockTransaction st)
        {

            if (ModelState.IsValid)
            {
                db.StockTransactions.Add(st);
                db.SaveChanges();

            };
        }

        private void UpdateWarehouseStock(int locationId, int variantId, double qty, string processType)
        {
            var p = db.WarehouseStocks.Where(x => (x.LocationID == locationId) && (x.ProductID == variantId)).FirstOrDefault();

            if (p == null)
            {
                var pv = db.Products.Where(x => x.ProductID == variantId).FirstOrDefault();

                var det = new WarehouseStock();
                det.LocationID = locationId;
                det.ProductID = pv.ProductID;
                det.SKU = pv.SKU;
                det.InitQty = 0;
                det.StockIn = 0;
                det.StockOut = 0;
                det.StockAdjusted = 0;
                det.InStock = 0;
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

                p = db.WarehouseStocks.Where(x => (x.LocationID == locationId) && (x.ProductID == variantId)).FirstOrDefault();

            };

            if (p != null)
            {
                if (processType == "IN")
                {
                    p.StockIn = p.StockIn + qty;
                    p.InStock = p.InStock + qty;
                    p.OnHand = p.InStock - p.Allocated;

                };
                if (processType == "OUT")
                {
                    p.StockOut = p.StockOut + qty;
                    p.InStock = p.InStock + qty;
                    p.OnHand = p.InStock - p.Allocated;

                };
                if (processType == "ADJ")
                {
                    p.StockAdjusted = qty;
                };
                if (processType == "ALLOCATE")
                {
                    p.Allocated = p.Allocated + qty;
                    p.OnHand = p.InStock - qty;
                };

                if (ModelState.IsValid)
                {
                    db.Entry(p).State = EntityState.Modified;
                    db.SaveChanges();
                };
            }

        }

        private void UpdateStockDet(int variantId, double qty, string processType)
        {
            var p = db.StockDets.Where(x => x.ProductID == variantId).FirstOrDefault();

            if (p != null)
            {
                if (processType == "IN")
                {
                    p.StockIn = p.StockIn + qty;
                    p.InStock = p.InStock + qty;
                    p.OnHand = p.InStock - p.Allocated;

                };
                if (processType == "OUT")
                {
                    p.StockOut = p.StockOut + qty;
                    p.InStock = p.InStock + qty;
                    p.OnHand = p.InStock - p.Allocated;

                };
                if (processType == "ADJ")
                {
                    p.StockAdjusted = qty;
                };
                if (processType == "ALLOCATE")
                {
                    p.Allocated = p.Allocated + qty;
                    p.OnHand = p.InStock - qty;
                };

                if (ModelState.IsValid)
                {
                    db.Entry(p).State = EntityState.Modified;
                    db.SaveChanges();
                };
            }

        }

        private void UpdateStock(int productId, double qty, string processType)
        {
            var p = db.Stocks.Where(x => x.ProductID == productId).FirstOrDefault();

            if (p != null)
            {
                if (processType == "IN")
                {
                    p.StockIn = p.StockIn + qty;
                    p.InStock = p.InStock + qty;
                    p.OnHand = p.InStock - p.Allocated;

                };
                if (processType == "OUT")
                {
                    p.StockOut = p.StockOut + qty;
                    p.InStock = p.InStock + qty;
                    p.OnHand = p.InStock - p.Allocated;

                };
                if (processType == "ADJ")
                {
                    p.StockAdjusted = qty;
                };
                if (processType == "ALLOCATE")
                {
                    p.Allocated = p.Allocated + qty;
                    p.OnHand = p.InStock - qty;
                };

                if (ModelState.IsValid)
                {
                    db.Entry(p).State = EntityState.Modified;
                    db.SaveChanges();
                };
            }

        }

        //public List<ProductSelection> GetProductList()
        //{
        //    List<ProductSelection> getList = (from p in db.Products
        //                                      where p.IsActive == true
        //                                      select new ProductSelection
        //                                      {
        //                                          ProductID = p.ProductID,
        //                                          SKU = p.SKU,
        //                                          ModelNo = p.ModelNo,
        //                                          ProductName = p.ProductName,
        //                                          ProductType = p.ProductType,
        //                                          Unit = p.Unit,
        //                                          CostPrice = p.CostPrice,
        //                                          SellPrice = p.RetailPrice,
        //                                          AvailableQty = 0

        //                                      }).ToList();

        //    return getList;

        //}



        //public List<ProductSelection> GetProductList2()
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


        //[HttpGet]
        //public JsonResult AutoComplete_Product(string search)
        //{
        //    var getList = GetProductList2();
        //    var data = getList;

        //    string[] words = search.ToUpper().Split(' ').ToArray();

        //    //      var results = from p in getList
        //    //                          .Select(x => x.ProductName.ToUpper())
        //    //                          .Where(x => words.All(y => x.Contains(y)))
        //    //                    select p;



        //    //       var result = from p in getList
        //    //                    let w = p.ProductName.ToUpper().Split(new char[] { '.', '-', '!', ' ', ';', ':', ',' },
        //    //                                               StringSplitOptions.RemoveEmptyEntries)
        //    //                    where (w.Distinct().Intersect(words).Count() == words.Count()) || (p.SKU.ToUpper().StartsWith(search.ToUpper()))
        //    //                     select p;

        //    var result = from p in getList
        //                 let w = p.ProductName.ToUpper().Split(new char[] { '.', '-', '!', ' ', ';', ':', ',' },
        //                                            StringSplitOptions.RemoveEmptyEntries)
        //                 where (w.Distinct().Intersect(words).Count() == words.Count()) ||
        //                 ((p.SKU.ToUpper().Contains(search.ToUpper())) ||
        //                 (p.ProductName.ToUpper().Contains(search.ToUpper())) ||
        //                 (p.Barcode.ToUpper() == search.ToUpper()))
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
        //        var getList = db.Products.Where(p => p.IsActive == true).ToList();

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

        public JsonResult AutoCompleteSelected_StockTakeDet(string search, string valSttID, string valLocationID)
        {
            int sid = Convert.ToInt32(valSttID);
            int lid = Convert.ToInt32(valLocationID);
 
            if (search != null)
            {
                int newid = Convert.ToInt32(search);
                var c = db.StockTakeDets.Where(x => x.ProductID == newid && x.LocationID == lid && x.SttID == sid).FirstOrDefault();

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


        //public JsonResult _KIVInvoice(int ItemID)
        //{
        //    List<KIVDET> kivDet = db.KIVDETs.Where(m => m.ItemID == ItemID).ToList();
        //    List<KIVInvoice> kivInvoices = new List<KIVInvoice>();

        //    foreach (var item in kivDet)
        //    {
        //        KIVInvoice kivInvoice = new KIVInvoice();
        //        List<KIVDeliveryDetail> kivDelDetail = db.KIVDeliveryDetails.Where(m => m.DetID == item.DetID).ToList().OrderByDescending(m => m.KIVDelDetailsID).ToList();
        //        INV inv = db.INVs.Find(item.InvID);

        //        string strYear = inv.CreatedOn.Year.ToString();
        //        string InvID = string.Format("INV-{0}{1:000000}", strYear, item.InvID.ToString());
        //        double kivBalQty = 0;
        //        if (kivDelDetail.Count > 0)
        //        {
        //            kivBalQty = kivDelDetail.First().KivBalanceQty;
        //        }
        //        kivInvoice.InvID = InvID;
        //        kivInvoice.ItemID = ItemID;
        //        kivInvoice.KIVQty = kivBalQty;

        //        kivInvoices.Add(kivInvoice);
        //    }
        //    return Json(kivInvoices, JsonRequestBehavior.AllowGet);
        //}




    }
}
