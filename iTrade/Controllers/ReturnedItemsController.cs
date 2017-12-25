using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using iTrade.Models;
using iTrade.CustomAttributes;
using System.Transactions;

namespace iTrade.Controllers
{
    public class ReturnedItemsController : Controller
    {
        private StarDbContext db = new StarDbContext();
        private CustomErrorMessage _CE = new CustomErrorMessage();

        // GET: ReturnedItems
        public ActionResult Index()
        {
            return View(db.ReturnedItems.ToList());
        }

        public ActionResult _DisplayResults(int id)
        {
            //   string str = "";
            switch (id)
            {
                case 0:
                    //     str = "";
                    ViewBag.TableNo = 0;
                    break;


            };

            var p = new List<ReturnedItem>();

            p = db.ReturnedItems.OrderByDescending(x => x.ReturnedItemID).ToList();


            return PartialView(p);
        }

        // GET: ReturnedItems/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReturnedItem returnedItem = db.ReturnedItems.Find(id);
            if (returnedItem == null)
            {
                return HttpNotFound();
            }
            return View(returnedItem);
        }

        // GET: ReturnedItems/Create
        public ActionResult Create()
        {
            var returnitem = new ReturnedItem();
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return View(returnitem);
        }

        // POST: ReturnedItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ReturnedItemID,QuoID,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,PostalCode,ReturnedAddress,ReturnedDate,ReturnedTime,Status,Remark,PersonID,PersonName,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn")] ReturnedItem returnedItem)
        {
            Boolean flag = false;
            TempData["errormessage"] = _CE.PostMessage("", false);
            int InvID = Convert.ToInt32(returnedItem.InvRef);
            if (returnedItem.InvRef != null && returnedItem.InvRef != "")
            {
                var inv = db.INVs.Where(x => x.InvID == InvID).FirstOrDefault();

                if (inv == null)
                {
                    TempData["errormessage"] = _CE.PostMessage("This invoice reference does not exist in the system. Please enter invoice number again.", true);
                    ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
                    ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
                    ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
                    return View(returnedItem);
                }
            }

            if (ModelState.IsValid)
            {
                returnedItem.CreatedBy = User.Identity.Name;
                returnedItem.CreatedOn = DateTime.Now;
                db.ReturnedItems.Add(returnedItem);
                db.SaveChanges();

                if (returnedItem.InvRef != "")
                {
                    var invdet = db.INVDETs.Where(x => x.InvID == InvID).ToList();

                    if (invdet != null)
                    {
                        ReturnedItemDetail o = new ReturnedItemDetail();
                        foreach (var item in invdet)
                        {
                            o = new ReturnedItemDetail();
                            o.Amount = item.Amount;
                            o.DiscountedPrice = item.DiscountedPrice;
                            o.Gst = item.Gst;
                            o.IsBundle = item.IsBundle;
                            o.IsControlItem = item.IsControlItem;
                            o.ItemCode = item.ItemCode;
                            o.ItemID = item.ItemID;
                            o.ItemName = item.ItemName;
                            o.ItemType = item.ItemType;
                            o.Nett = item.Nett;
                            o.Position = item.Position;
                            o.Qty = item.Qty;
                            o.RefItemID = item.RefItemID;
                            o.Remark = item.Remark;
                            o.ReturnedItemID = returnedItem.ReturnedItemID;
                            o.SalesType = item.SalesType;
                            o.SellType = item.SellType;
                            o.Unit = item.Unit;
                            o.UnitPrice = item.UnitPrice;

                            db.ReturnedItemDetails.Add(o);
                            db.SaveChanges();
                        }
                    }
                }

                flag = true;
            }

            if (flag)
            {
                return RedirectToAction("Edit", new { id = returnedItem.ReturnedItemID });
            }

            ViewBag.Message = "1";

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return View(returnedItem);
        }

        // GET: ReturnedItems/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReturnedItem returnedItem = db.ReturnedItems.Find(id);
            if (returnedItem == null)
            {
                return HttpNotFound();
            }
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == returnedItem.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            return View(returnedItem);
        }

        // POST: ReturnedItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ReturnedItemID,QuoID,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,PostalCode,ReturnedAddress,ReturnedDate,ReturnedTime,Status,Remark,PersonID,PersonName,CreatedBy,CreatedOn,ModifiedBy,ModifiedOn")] ReturnedItem returnedItem)
        {
            if (ModelState.IsValid)
            {
                returnedItem.ModifiedBy = User.Identity.Name;
                returnedItem.ModifiedOn = DateTime.Now;
                db.Entry(returnedItem).State = EntityState.Modified;
                db.SaveChanges();

                ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
                ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
                ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
                ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == returnedItem.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();
                return RedirectToAction("Index");
            }
            return View(returnedItem);
        }

        // GET: ReturnedItems/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReturnedItem returnedItem = db.ReturnedItems.Find(id);
            if (returnedItem == null)
            {
                return HttpNotFound();
            }
            return View(returnedItem);
        }

        // POST: ReturnedItems/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    ReturnedItem returnedItem = db.ReturnedItems.Find(id);
        //    db.ReturnedItems.Remove(returnedItem);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public ActionResult _DisplayInvDets(int id)
        {
            var p = new List<ReturnedItemDetail>();
            p = db.ReturnedItemDetails
                .Where(x => (x.ReturnedItemID == id))
                .OrderBy(x => x.ReturnedItemDetailID)
                .ToList();

            var r = db.ReturnedItems.Where(y => y.ReturnedItemID == id).FirstOrDefault();
            ViewBag.ReturnedItemStatus = r.Status;
            ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        public ActionResult _AddDet(int id)
        {
            var p = new ReturnedItemDetail();
            p.ReturnedItemID = id;

            //   var getList = GetProductList();

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //     ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //  ViewData["ProductData"] = getList;

            return PartialView(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddDet(ReturnedItemDetail data)
        {
            var ps = db.Products.Where(x => x.ProductID == data.ItemID).FirstOrDefault();

            int iID = data.ReturnedItemID;

            decimal costprice = ps.CostPrice;
            string costcode = Decimal2String(costprice);
            data.Remark = costcode;

            data.Nett = data.Amount + data.Gst;

            var invdet1 = db.ReturnedItemDetails.Where(x => x.RefItemID == 0 && x.ReturnedItemID == data.ReturnedItemID).ToList();
            double positioncount = invdet1.Count;
            data.Position = positioncount + 1;

            if (ModelState.IsValid)
            {
                db.ReturnedItemDetails.Add(data);
                db.SaveChanges();
            };

            //   AddKivDet(data);

            int bundlecount = 0;

            foreach (var pb in ps.Productbundles)
            {
                var p = db.Products.Where(x => x.SKU == pb.IncSKU).FirstOrDefault();

                if (p != null)
                {
                    bundlecount++;
                    var det = new ReturnedItemDetail();
                    det.ReturnedItemID = data.ReturnedItemID;
                    det.ItemID = data.ItemID;
                    det.ItemCode = pb.IncSKU;
                    det.ItemType = data.ItemType;
                    det.ItemName = pb.IncProductName;
                    det.Qty = Convert.ToDouble(pb.IncQty);
                    det.Unit = p.Unit;
                    det.Remark = "Bundle Item";
                    det.IsControlItem = p.IsControlItem;
                    det.Position = Convert.ToDouble((positioncount + 1).ToString() + "." + bundlecount.ToString());

                    if (ModelState.IsValid)
                    {
                        db.ReturnedItemDetails.Add(det);
                        db.SaveChanges();

                    };
                }

            }

            ViewBag.Message = "1";

            return Json(new { redirectUrl = Url.Action("Edit//" + iID.ToString(), "ReturnedItems") });

        }

        public ActionResult _AddDetBundle(int id, int qty, int SorID)
        {
            var sor = db.ReturnedItems.Where(x => x.ReturnedItemID == SorID).FirstOrDefault();

            if (sor == null)
            {
                return null;
            }

            var invtype = sor.InvType;


            var dets = new List<ReturnedItemDetail>();
            var p = db.Products.Find(id);

            if (p != null)
            {
                var invdet1 = db.ReturnedItemDetails.Where(x => x.RefItemID == 0 && x.ReturnedItemID == sor.ReturnedItemID).ToList();
                double positioncount = invdet1.Count;
                var det1 = new ReturnedItemDetail();
                det1.ReturnedItemID = SorID;
                //    det1.InvID = 0;
                det1.ItemID = p.ProductID;
                det1.ItemCode = p.SKU;
                det1.ItemType = p.ProductType;
                det1.ItemName = p.ProductName;
                det1.Qty = Convert.ToDouble(qty);
                det1.Unit = p.Unit;
                det1.Remark = "";
                det1.IsControlItem = p.IsControlItem;
                det1.IsBundle = p.IsBundle;
                det1.RefItemID = 0;
                det1.SalesType = "DefaultItem";
                //det1.Position = 0;
                det1.Position = positioncount + 1;
                det1.UnitPrice = p.DealerPrice;
                det1.DiscountedPrice = p.DealerPrice;

                if (invtype == "CS")
                {
                    det1.UnitPrice = p.RetailPrice;
                    det1.DiscountedPrice = p.RetailPrice;
                }

                //det1.Discount = 0;

                if (p.UsePricebreak)
                {
                    var breakqtys = p.Pricebreaks.Where(x => x.BreakQty != null && x.BreakQty >= 0).OrderByDescending(x => x.BreakQty).ToList();

                    foreach (var bq in breakqtys)
                    {
                        if (det1.Qty >= bq.BreakQty)
                        {
                            decimal price1 = Convert.ToDecimal(bq.DealerPrice);

                            if (invtype == "CS")
                            {
                                price1 = Convert.ToDecimal(bq.RetailPrice);
                            }
                            det1.UnitPrice = price1;
                            det1.DiscountedPrice = price1;
                            //det1.Discount = 0;

                            break;
                        }
                    }

                };


                //det1.PreDiscAmount = det1.UnitPrice * qty;
                det1.Amount = det1.DiscountedPrice * qty;
                det1.Gst = 0;
                det1.Nett = det1.Amount + det1.Gst;

                dets.Add(det1);

                int bundlecount = 0;

                foreach (var bb in p.Productbundles)
                {
                    bundlecount++;
                    var det2 = new ReturnedItemDetail();
                    det2.ReturnedItemID = SorID;
                    det2.ItemID = bb.IncProductID;
                    det2.ItemCode = bb.IncSKU;
                    det2.ItemType = bb.IncProductType;
                    det2.ItemName = bb.IncProductName;
                    det2.Qty = bb.IncQty * qty;
                    det2.Unit = p.Unit;
                    det2.Remark = p.ProductID.ToString();
                    det2.IsControlItem = bb.IsControlItem;
                    det2.IsBundle = p.IsBundle;
                    det2.RefItemID = 0;
                    det2.SalesType = "BundleItem";
                    //det2.Position = 1;
                    det2.Position = Convert.ToDouble((positioncount + 1).ToString() + "." + bundlecount.ToString());
                    dets.Add(det2);

                }

                ViewBag.HasFOC = "False";

                if (p.ProductFOCs.Count > 0)
                {
                    ViewBag.HasFOC = "True";

                    double focqty = 0.00;

                    if (p.UsePricebreak)
                    {
                        var breakqtys = p.Pricebreaks.Where(x => x.BreakQty != null && x.BreakQty >= 0).OrderByDescending(x => x.BreakQty).ToList();

                        foreach (var bq in breakqtys)
                        {
                            if (det1.Qty >= bq.BreakQty)
                            {
                                if (!string.IsNullOrEmpty(bq.FocQty.ToString()))
                                {
                                    focqty = Convert.ToDouble(bq.FocQty);
                                };
                                break;
                            }
                        }

                    };

                    foreach (var bb in p.ProductFOCs)
                    {
                        bundlecount++;
                        var det2 = new ReturnedItemDetail();
                        det2.ReturnedItemID = SorID;
                        det2.ItemID = bb.IncProductID;
                        det2.ItemCode = bb.IncSKU;
                        det2.ItemType = bb.IncProductType;
                        det2.ItemName = bb.IncProductName;
                        det2.Qty = bb.IncQty * focqty;
                        det2.Unit = p.Unit;
                        det2.Remark = p.ProductID.ToString();
                        det2.IsControlItem = bb.IsControlItem;
                        det2.IsBundle = p.IsBundle;
                        det2.RefItemID = 0;
                        det2.SalesType = "FOCItem";
                        //det2.Position = 2;
                        det2.Position = Convert.ToDouble((positioncount + 1).ToString() + "." + bundlecount.ToString());
                        dets.Add(det2);

                    }
                }

            }

            return PartialView(dets);
        }

        [HttpPost]
        public ActionResult _AddDetBundle(List<ReturnedItemDetail> list)
        {
            if (list == null)
            {
                return PartialView(list);
            }
            else
            {
                int SorID = list.FirstOrDefault().ReturnedItemID;

                bool IsFirst = true;
                int refid = 0;

                foreach (var det in list)
                {
                    if (det.Qty > 0)
                    {
                        det.RefItemID = refid;
                        det.Nett = det.Amount + det.Gst;

                        if (ModelState.IsValid)
                        {
                            db.ReturnedItemDetails.Add(det);
                            db.SaveChanges();

                            if (IsFirst)
                            {
                                refid = det.ReturnedItemDetailID;
                                IsFirst = false;
                            }

                        };
                    }

                };
                return RedirectToAction("Edit", new { id = SorID });
            }

        }

        public ActionResult _EditDet(int id)
        {
            var p = db.ReturnedItemDetails.Find(id);

            if (p == null)
            {
                return HttpNotFound();
            }

            var product = db.Products.Where(x => x.SKU == p.ItemCode).FirstOrDefault();
            var productdata = GetProductList();

            var getList = productdata;

            if (!string.IsNullOrEmpty(product.ModelNo))
            {
                getList = productdata.Where(x => x.ModelNo != null)
                                    .Where(x => x.ModelNo.ToUpper().StartsWith(product.ModelNo.ToUpper()))
                                    .ToList().Distinct().OrderBy(x => x.ProductName).Take(30).ToList();

                if (getList == null)
                {
                    getList = productdata.Where(x => x.SKU == p.ItemCode)
                                        .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
                }

            }
            else
            {
                getList = productdata.Where(x => x.SKU == p.ItemCode)
                                    .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
            }




            ViewData["ProductChangeList"] = getList;

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _EditDet([Bind(Include = "ReturnedItemDetailID,ReturnedItemID,ItemID,ItemCode,ItemType,ItemName,Qty,Unit,UnitPrice,DiscountedPrice,Amount,Gst,Nett,Remark,IsControlItem,ReasonReturned")] ReturnedItemDetail data)
        {
            Convert.ToDouble(data.Qty);
            Convert.ToDecimal(data.DiscountedPrice);
            Convert.ToDecimal(data.UnitPrice);
            Convert.ToDecimal(data.Amount);
            Convert.ToDecimal(data.Gst);
            Convert.ToDecimal(data.Nett);

            if (ModelState.IsValid)
            {
                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();
            }

            var product = db.Products.Where(x => x.SKU == data.ItemCode).FirstOrDefault();
            var productdata = GetProductList();

            var getList = productdata;

            if (!string.IsNullOrEmpty(product.ModelNo))
            {
                getList = productdata.Where(x => x.ModelNo != null)
                                    .Where(x => x.ModelNo.ToUpper().StartsWith(product.ModelNo.ToUpper()))
                                    .ToList().Distinct().OrderBy(x => x.ProductName).Take(30).ToList();

                if (getList == null)
                {
                    getList = productdata.Where(x => x.SKU == data.ItemCode)
                                        .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
                }

            }
            else
            {
                getList = productdata.Where(x => x.SKU == data.ItemCode)
                                    .ToList().Distinct().OrderBy(x => x.ProductName).ToList();
            }

            ViewData["ProductChangeList"] = getList;

            return Json(new { success = true });
        }

        public ActionResult _EditDetBundle(int id)
        {
            var det = db.ReturnedItemDetails.Find(id);
            if (det == null)
            {
                return HttpNotFound();
            }

            var dets = new List<ReturnedItemDetail>();
            dets.Add(det);

            var p = db.Products.Find(det.ItemID);

            if (p != null)
            {
                foreach (var bb in p.Productbundles)
                {
                    var det2 = new ReturnedItemDetail();
                    det2.ReturnedItemID = det.ReturnedItemID;
                    det2.ItemID = bb.IncProductID;
                    det2.ItemCode = bb.IncSKU;
                    det2.ItemType = bb.IncProductType;
                    det2.ItemName = bb.IncProductName;
                    det2.Qty = Convert.ToDouble(0);
                    det2.Unit = "";
                    det2.Remark = p.ProductID.ToString();
                    det2.IsControlItem = bb.IsControlItem;
                    det2.IsBundle = p.IsBundle;
                    det2.SalesType = "BundleItem";
                    det2.RefItemID = det.ReturnedItemDetailID;
                    det2.Position = bb.Position;

                    var tmp = db.ReturnedItemDetails.Where(x => x.ReturnedItemID == det.ReturnedItemID && x.ItemCode == bb.IncSKU && x.RefItemID == det.ReturnedItemDetailID && x.SalesType == "BundleItem").FirstOrDefault();
                    if (tmp != null)
                    {
                        det2.ReturnedItemDetailID = tmp.ReturnedItemDetailID;
                        det2.Qty = tmp.Qty;
                        det2.Unit = tmp.Unit;

                        //    det2.SalesType = tmp.SalesType;
                    };

                    dets.Add(det2);

                }

                ViewBag.HasFOC = "False";

                if (p.ProductFOCs.Count > 0)
                {
                    ViewBag.HasFOC = "True";
                    foreach (var bb in p.ProductFOCs)
                    {
                        var det2 = new ReturnedItemDetail();
                        det2.ReturnedItemID = det.ReturnedItemID;
                        det2.ItemID = bb.IncProductID;
                        det2.ItemCode = bb.IncSKU;
                        det2.ItemType = bb.IncProductType;
                        det2.ItemName = bb.IncProductName;
                        det2.Qty = Convert.ToDouble(0);
                        det2.Unit = "";
                        det2.Remark = p.ProductID.ToString();
                        det2.IsControlItem = bb.IsControlItem;
                        det2.IsBundle = p.IsBundle;
                        det2.SalesType = "FOCItem";
                        det2.RefItemID = det.ReturnedItemDetailID;
                        det2.Position = bb.Position;

                        var tmp = db.ReturnedItemDetails.Where(x => x.ReturnedItemID == det.ReturnedItemID && x.ItemCode == bb.IncSKU && x.RefItemID == det.ReturnedItemDetailID && x.SalesType == "FOCItem").FirstOrDefault();
                        if (tmp != null)
                        {
                            det2.ReturnedItemDetailID = tmp.ReturnedItemDetailID;
                            det2.Qty = tmp.Qty;
                            det2.Unit = tmp.Unit;
                            //   det2.SalesType = tmp.SalesType;
                        };

                        dets.Add(det2);

                    }
                }



            }

            return PartialView(dets);
        }

        [HttpPost]
        public ActionResult _EditDetBundle(List<ReturnedItemDetail> list)
        {
            int SorID = list.FirstOrDefault().ReturnedItemID;
            int refid = list.FirstOrDefault().ReturnedItemDetailID;
            var item1 = list.FirstOrDefault();

            var det0 = db.ReturnedItemDetails.Find(refid);
            if (det0 != null)
            {
                det0.Qty = item1.Qty;
                det0.DiscountedPrice = item1.DiscountedPrice;
                det0.Amount = item1.DiscountedPrice * Convert.ToDecimal(item1.Qty);
                det0.Gst = item1.Gst;
                det0.Nett = item1.Amount + item1.Gst;

                if (ModelState.IsValid)
                {
                    db.Entry(det0).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }

            if (ModelState.IsValid)
            {
                bool IsFirst = true;
                foreach (ReturnedItemDetail det in list)
                {
                    if (IsFirst)
                    {
                        IsFirst = false;
                    }
                    else
                    {
                        var det1 = db.ReturnedItemDetails.Find(det.ReturnedItemDetailID);

                        if (det1 != null)
                        {
                            det1.Qty = det.Qty;
                            det1.DiscountedPrice = det.DiscountedPrice;
                            det1.Amount = det.DiscountedPrice * Convert.ToDecimal(det.Qty);
                            det1.Gst = det.Gst;
                            det1.Nett = det.Amount + det.Gst;

                        }
                        else
                        {
                            det1 = new ReturnedItemDetail();
                            det1.ReturnedItemID = SorID;
                            det1.ItemID = det.ItemID;
                            det1.ItemCode = det.ItemCode;
                            det1.ItemType = det.ItemType;
                            det1.ItemName = det.ItemName;
                            det1.Qty = Convert.ToDouble(det.Qty);
                            det1.Unit = det.Unit;
                            det1.Remark = det.Remark;
                            det1.IsControlItem = det.IsControlItem;
                            det1.IsBundle = det.IsBundle;
                            det1.SalesType = det.SalesType;
                            det1.RefItemID = refid;

                            var detfirst = db.ReturnedItemDetails.Where(x => x.ReturnedItemDetailID == refid && x.ReturnedItemID == SorID).FirstOrDefault();
                            var detlist = db.ReturnedItemDetails.Where(x => x.RefItemID == refid && x.ReturnedItemID == SorID).ToList();

                            //det1.Position = Convert.ToDouble(det.Position);
                            det1.Position = Convert.ToDouble(detfirst.Position + "." + (detlist.Count + 1));
                            det1.UnitPrice = det.UnitPrice;
                            det1.DiscountedPrice = det.DiscountedPrice;
                            det1.Amount = det.Amount;
                            det1.Gst = det.Gst;
                            det1.Nett = det.Nett;
                            det1.ReasonReturned = det.ReasonReturned;

                            db.Entry(det1).State = EntityState.Added;

                        }
                    }

                };
                db.SaveChanges();
            };

            return Json(new { success = true });

        }

        public ActionResult _DelDet(int id = 0)
        {
            ReturnedItemDetail det = db.ReturnedItemDetails.Find(id);
            if (det == null)
            {
                return HttpNotFound();
            }
            return PartialView("_DelDet", det);
        }


        [HttpPost, ActionName("_DelDet")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var det = db.ReturnedItemDetails.Find(id);
                double position = det.Position;

                int SorID = det.ReturnedItemID;

                var dets = db.ReturnedItemDetails.Where(x => x.ReturnedItemID == det.ReturnedItemID && x.RefItemID == det.ReturnedItemDetailID).ToList();
                foreach (var dd in dets)
                {
                    db.Entry(dd).State = EntityState.Deleted;
                }

                //   db.INVDETs.Remove(det);
                db.Entry(det).State = EntityState.Deleted;

                db.SaveChanges();

                trans.Complete();
            }

            return Json(new { success = true });
        }

        public string Decimal2String(Decimal val)
        {
            string retVal = string.Empty;

            retVal = val.ToString().Replace("1", "A").Replace("2", "I").Replace("3", "K").Replace("4", "C").Replace("5", "H").Replace("6", "N").Replace("7", "M").Replace("8", "E").Replace("9", "R").Replace("0", "Y").Replace(".", "0");

            string finalResult;

            string[] x = retVal.Select(c => c.ToString()).ToArray();

            for (int i = 0; i < x.Length - 1; i++)
            {
                for (int j = i + 1; j < x.Length; j++)
                {
                    if (x[i] == x[j])
                    {

                        x[j] = x[j].Replace(x[j], "S");

                    }

                }
            };

            finalResult = string.Join("", x);
            return finalResult;

        }

        public JsonResult AutoCompleteSelected(string search)
        {
            if (search != null)
            {
                int custno = Convert.ToInt32(search);
                var c = db.Clients
                           .Where(x => x.CustNo == custno)
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
        [HttpGet]
        public JsonResult AutoComplete_Product(string search)
        {
            var getList = GetProductList();
            var data = getList;

            string[] words = search.ToUpper().Split(' ').ToArray();

            //      var results = from p in getList
            //                          .Select(x => x.ProductName.ToUpper())
            //                          .Where(x => words.All(y => x.Contains(y)))
            //                    select p;



            //       var result = from p in getList
            //                    let w = p.ProductName.ToUpper().Split(new char[] { '.', '-', '!', ' ', ';', ':', ',' },
            //                                               StringSplitOptions.RemoveEmptyEntries)
            //                    where (w.Distinct().Intersect(words).Count() == words.Count()) || (p.SKU.ToUpper().StartsWith(search.ToUpper()))
            //                     select p;

            var result = from p in getList
                         let w = p.ProductName.ToUpper().Split(new char[] { '.', '-', '!', ' ', ';', ':', ',' },
                                                    StringSplitOptions.RemoveEmptyEntries)
                         where (w.Distinct().Intersect(words).Count() == words.Count()) ||
                         ((p.SKU.ToUpper().Contains(search.ToUpper())) ||
                         (p.ProductName.ToUpper().Contains(search.ToUpper())))
                         //             || (p.Barcode.ToUpper() == search.ToUpper()))
                         select p;


            data = result.ToList();



            if (data == null)
            {
                return null;
            }
            else
            {
                return Json(data.Take(100).ToList(), JsonRequestBehavior.AllowGet);
            }


        }
        public JsonResult AutoCompleteSelected_Product(string search)
        {
            if (search != null)
            {
                var getList = db.Products.Where(p => p.IsActive == true).ToList();

                int newid = Convert.ToInt32(search);
                var c = getList.Where(x => x.ProductID == newid).FirstOrDefault();

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
        public JsonResult GetPricebreaks(string custno, string itemid)
        {
            if (custno != null && itemid != null)
            {
                int cid = Convert.ToInt32(custno);
                int iid = Convert.ToInt32(itemid);

                var p = db.Products.Where(x => x.ProductID == iid).FirstOrDefault();

                var pbreaks = p.Pricebreaks.Where(x => x.BreakQty != null).OrderBy(x => x.BreakQty).ToList();

                var c = pbreaks;

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
        public List<ProductSelection> GetProductList()
        {
            List<ProductSelection> getList = (from p in db.Products
                                              where p.IsActive == true
                                              select new ProductSelection
                                              {
                                                  ProductID = p.ProductID,
                                                  SKU = p.SKU,
                                                  Barcode = p.Barcode,
                                                  ProductType = p.ProductType,
                                                  ProductName = p.ProductName,
                                                  ModelNo = p.ModelNo,
                                                  IsBundle = p.IsBundle,
                                                  Unit = p.Unit,
                                                  CostPrice = p.CostPrice,
                                                  SellPrice = p.RetailPrice,
                                                  IsControlItem = p.IsControlItem,
                                                  AvailableQty = 0

                                              }).ToList();

            return getList;

        }

        public JsonResult _ConvertToInvoice(string valSorID)
        {
            int id = Convert.ToInt32(valSorID);

            var sor = db.ReturnedItems.Find(id);
            if (sor == null)
            {
                return null;
            };

            sor.Status = "Confirmed";
            if (ModelState.IsValid)
            {
                db.Entry(sor).State = EntityState.Modified;
                db.SaveChanges();

                var r = db.ReturnedItemDetails.Where(x => x.ReturnedItemID == id).ToList();

                foreach (var item in r)
                {
                    var ps = db.Products.Where(y => y.ProductID == item.ItemID).FirstOrDefault();
                    var oStock = db.Stocks.Where(x => x.ProductID == item.ItemID).FirstOrDefault();
                    oStock.StockIn += item.Qty;
                    db.Entry(oStock).State = EntityState.Modified;
                    var oStockDet = db.StockDets.Where(x => x.ProductID == item.ItemID).FirstOrDefault();
                    oStockDet.StockIn += item.Qty;
                    db.Entry(oStockDet).State = EntityState.Modified;
                    StockTransaction oStockTrans = new StockTransaction();
                    oStockTrans.ProductID = item.ItemID;
                    oStockTrans.SKU = ps.SKU;
                    oStockTrans.ProcessType = "IN";
                    oStockTrans.Qty = item.Qty;
                    oStockTrans.RefNo = item.ReturnedItemID.ToString();
                    oStockTrans.SourceFrom = "ReturnedItems";
                    oStockTrans.CreatedBy = User.Identity.Name;
                    oStockTrans.CreatedOn = DateTime.Now;
                    db.StockTransactions.Add(oStockTrans);
                    db.SaveChanges();
                }
            };

            return Json(new
            {
                redirectUrl = Url.Action("CreditNotePrint//" + id, "ReturnedItems"),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult CreditNotePrint(int id)
        {
            ReturnedItem sar = db.ReturnedItems.Find(id);
            var p = db.ReturnedItemDetails
                .Where(x => (x.ReturnedItemID == id))
                .OrderBy(x => x.Position)
                .ToList();
            decimal iTotal = 0;

            foreach (var item in p)
            {
                iTotal += item.Amount;
            }

            ViewData["seTotalAmountCreditNotePrint"] = iTotal;
            return View(sar);
        }
        public ActionResult _DisplaySalesItems(int id)
        {
            var p = new List<ReturnedItemDetail>();
            p = db.ReturnedItemDetails
                .Where(x => (x.ReturnedItemID == id))
                .OrderBy(x => x.Position)
                .ToList();

            return PartialView(p);
        }

        public JsonResult _PopulateInvRef(int InvID, int ReturnedItemID)
        {
            var invdet = db.INVDETs.Where(x => x.InvID == InvID).ToList();

            if (invdet != null)
            {
                ReturnedItemDetail o = new ReturnedItemDetail();
                foreach (var item in invdet)
                {
                    o = new ReturnedItemDetail();
                    o.Amount = item.Amount;
                    o.DiscountedPrice = item.DiscountedPrice;
                    o.Gst = item.Gst;
                    o.IsBundle = item.IsBundle;
                    o.IsControlItem = item.IsControlItem;
                    o.ItemCode = item.ItemCode;
                    o.ItemID = item.ItemID;
                    o.ItemName = item.ItemName;
                    o.ItemType = item.ItemType;
                    o.Nett = item.Nett;
                    o.Position = item.Position;
                    o.Qty = item.Qty;
                    o.RefItemID = item.RefItemID;
                    o.Remark = item.Remark;
                    o.ReturnedItemID = ReturnedItemID;
                    o.SalesType = item.SalesType;
                    o.SellType = item.SellType;
                    o.Unit = item.Unit;
                    o.UnitPrice = item.UnitPrice;

                    db.ReturnedItemDetails.Add(o);
                    db.SaveChanges();
                }

                var re = db.ReturnedItems.Where(x => x.ReturnedItemID == ReturnedItemID).FirstOrDefault();
                re.InvRef = InvID.ToString();
                db.Entry(re).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(new
            {
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
