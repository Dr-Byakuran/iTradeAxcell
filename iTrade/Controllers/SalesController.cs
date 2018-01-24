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
    public class SalesController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();

        // GET: Sales
        public ActionResult Index()
        {

            return View();
        }

        // [ChildActionOnly] 
        public ActionResult _DisplayResults(string invtype)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            int BranchID = Convert.ToInt32(user.BranchID);

            DateTime datefrom = DateTime.Now.AddMonths(-12);

            ViewData["BranchsAll"] = db.CompanyBranches.ToList();

            var p = new List<INV>();

            if (BranchID == 1)
            {
                p = db.INVs.Where(x => x.InvDate >= datefrom).Take(1000).OrderByDescending(x => x.InvID).ToList();

                return PartialView(p);
            }
            else
            {
                p = db.INVs.Where(x => x.InvDate >= datefrom && x.BranchID ==BranchID).Take(1000).OrderByDescending(x => x.InvID).ToList();

                return PartialView(p);
            }

        }

        // GET: Sales/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            INV item = db.INVs.Find(id);
            if (item == null)
            {
                return HttpNotFound();
            }
            return View(item);
        }

        // GET: Sales/Create
        public ActionResult Create()
        {
            var p = new INV();

            //  p.SarOn.DateCreated = DateTime.Now;
            //  ViewBag.ProductTypes = db.ProductGroups.ToList();

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return View(p);
        }

        // POST: Sales/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "InvID,InvType,InvDate,PoNo,CustNo,CustName,Addr1,Addr2,Addr3,PostalCode,DeliveryAddress,DeliveryDate,DeliveryTime,PaymentTerms,Status,LocationID,LocationName,Remark,PersonID,PersonName")] INV inv)
        {
            inv.PreDiscAmount = 0;
            inv.Discount = 0;
            inv.Amount = 0;
            inv.Gst = 0;
            inv.Nett = 0;
            inv.IsPaid = false;
            inv.CreatedBy = User.Identity.Name;
            inv.CreatedOn = DateTime.Now;

         
            if (ModelState.IsValid)
            {
                db.INVs.Add(inv);
                db.SaveChanges();
                return RedirectToAction("Edit", new { id = inv.InvID });
            };

            ViewBag.Message = "1";

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return View(inv);
        }

        // GET: Sales/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
           INV inv = db.INVs.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            var salespay = db.SalesPaymentMethods.Where(x => x.InvID == id).ToList();

            decimal sPaidAmount = 0;
            foreach (var s in salespay)
            {
                sPaidAmount += s.Amount;
            }

            ViewData["seSalesPaymentAmount"] = sPaidAmount;
            return View(inv);
        }

        // POST: Sales/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "InvID,InvType,InvDate,PoNo,CustNo,CustName,Addr1,Addr2,Addr3,PostalCode,DeliveryAddress,DeliveryDate,DeliveryTime,Status,PreDiscAmount,Discount,Amount,Gst,Nett,PaymentTerms,LocationID,LocationName,Remark,PersonID,PersonName,CreatedOn")] INV inv)
        {
            inv.ModifiedBy = User.Identity.Name;
            inv.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Edit", new { id = inv.InvID });
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return View(inv);
        }


        public ActionResult View(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            INV inv = db.INVs.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            //ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            //ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            if (inv != null)
            {
                ViewBag.InvoiceNo = inv.InvID;
            }
            else
            {
                ViewBag.InvoiceNo = "";
            };

            ViewData["seGSTRate"] = GetGstRate();
            return View(inv);
        }

        public ActionResult InvEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            INV inv = db.INVs.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            if (inv != null)
            {
                ViewBag.InvoiceNo = inv.InvID;
            }
            else
            {
                ViewBag.InvoiceNo = "";
            };

            ViewData["seGSTRate"] = GetGstRate();
            return View(inv);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public JsonResult InvEdit([Bind(Include = "SorID,SorNo,QuoID,QuoNo,DorNo,InvID,InvNo,InvType,InvDate,PoNo,InvRef,CustNo,CustName,CustName2,Addr1,Addr2,Addr3,PostalCode,DeliveryAddress,DeliveryDate,DeliveryTime,Status,PreDiscAmount,Discount,Amount,Gst,Nett,PaidAmount,PaymentTerms,Remark,PersonID,PersonName,CreatedOn")] INV inv)
        {
            if (inv.PersonID == 0)
            {
                inv.PersonName = null;
            }
            inv.ModifiedBy = User.Identity.Name;
            inv.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();
            }

            string str = Request.Form["actionType"];

            if (str == "SaveAndAdd")
            {
                //return RedirectToAction("CrCreate");
                //   return RedirectToAction("CrEdit", new { id = 0 });
            };

            //  return RedirectToAction("CrEdit", new { id = inv.SorID });

            bool isFound = false;
            var clist = GetClientListByUser("CS");
            foreach (var c in clist)
            {
                if (c.CustNo == inv.CustNo)
                {
                    isFound = true;
                }
            }

            if (!isFound)
            {
                var client = db.Clients.Where(x => x.CustNo == inv.CustNo).FirstOrDefault();
                clist.Add(client);
            }

            //   ViewData["ClientsAll"] = clist;
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == inv.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            ViewBag.SalesID = 0;

            var staff = db.Staffs.Where(x => x.Email.Trim() == User.Identity.Name.Trim()).FirstOrDefault();
            if (staff != null)
            {
                ViewBag.StaffID = staff.StaffID;
            }

            //   return View(inv);

            return Json(new { success = true, redirectUrl = Url.Action("InvEdit", "Sales", new { id = 0, str = "0" }) });

        }


        public ActionResult _OrderDetail(int? id, string act)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            INV inv = db.INVs.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewBag.Act = act;
            ViewData["seGSTRate"] = GetGstRate();

            return PartialView(inv);
        }

        public ActionResult _DisplayInvDetsView(int id, string act)
        {
            var p = new List<INVDET>();
            p = db.INVDETs
                .Where(x => (x.InvID == id))
                .OrderBy(x => x.Position)
                .ToList();

            //ViewBag.QuoteNumber = id;
            ViewBag.Act = act;

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _DisplayInvDetsView(List<INVDET> list)
        {
            if (list == null)
            {
                return null;
            }

            var inv = list.FirstOrDefault();
            int invid = inv.InvID;
            string invtype = inv.InvType;
            decimal gst = GetGstRate();

            if (ModelState.IsValid)
            {
                foreach (var i in list)
                {
                    var det = db.INVDETs.Find(i.DetID);
                    if (det != null)
                    {
                        det.ItemDesc = i.ItemDesc;
                        det.Remark = i.Remark;
                        det.Qty = i.Qty;
                        det.Unit = i.Unit;
                        det.UnitPrice = i.UnitPrice;
                        det.Discount = i.Discount;

                        if (i.Discount > 0)
                        {
                            det.Discount = 0 - i.Discount;
                        }

                        det.DiscountedPrice = i.UnitPrice + det.Discount;

                        if (det.DetType == "REGULAR")
                        {
                            int baseqty = 4;
                            decimal unitfee = System.Math.Round((det.DiscountedPrice / baseqty), 2, MidpointRounding.AwayFromZero);
                            det.Amount = System.Math.Round((unitfee * Convert.ToDecimal(det.Qty)), 2, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            det.Amount = System.Math.Round((det.DiscountedPrice * Convert.ToDecimal(det.Qty)), 2, MidpointRounding.AwayFromZero);

                        }

                        det.Gst = System.Math.Round((det.Amount * gst), 2, MidpointRounding.AwayFromZero);
                        det.Nett = det.Amount + det.Gst;
                        det.ModifiedBy = User.Identity.Name;
                        det.ModifiedOn = DateTime.Now;

                    }

                    //   UpdateKivDet(det);
                };
                db.SaveChanges();
            };

            UpdateContractAmount(invid);

            return Json(new { success = true, redirectUrl = Url.Action("View", "Sales", new { id = invid, str = "0" }) });

        }


        public ActionResult Manage(int? id)
        {
            ViewBag.ProductTypes = db.ProductGroups.ToList();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            INV inv = db.INVs.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();


            return View(inv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage([Bind(Include = "InvID,InvType,InvDate,PoNo,CustNo,CustName,Addr1,Addr2,Addr3,PostalCode,DeliveryAddress,DeliveryDate,DeliveryTime,Status,PreDiscAmount,Discount,Amount,Gst,Nett,PaymentTerms,LocationID,LocationName,Remark,PersonID,PersonName,CreatedOn")] INV inv)
        {
            inv.ModifiedBy = User.Identity.Name;
            inv.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Edit", new { id = inv.InvID });
            }

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return View(inv);
        }



        // GET: Sales/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            INV inv = db.INVs.Find(id);
            if (inv == null)
            {
                return HttpNotFound();
            }
            return View(inv);
        }

        // POST: Sales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmedSAR(int id)
        {
            INV inv = db.INVs.Find(id);
            db.INVs.Remove(inv);
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


        public ActionResult _DisplayInvDets(int id)
        {
            var p = new List<INVDET>();
            p = db.INVDETs
                .Where(x => (x.InvID == id))
                .OrderBy(x => x.Position)
                .ToList();

            ViewBag.QuoteNumber = id;

            return PartialView(p);
        }

        [HttpPost]
        public ActionResult _DisplayQuoteDets(List<INVDET> list)
        {
            if (ModelState.IsValid)
            {
                var sarid = 0;

                foreach (var i in list)
                {

                    var c = db.INVDETs.Where(a => a.InvID.Equals(i.InvID)).FirstOrDefault();

                    if (c != null)
                    {
                        c.ItemName = i.ItemName;
                        c.Qty = i.Qty;
                        c.UnitPrice = i.UnitPrice;
                        c.Amount = i.Amount;

                        sarid = i.InvID;
                    }

                }

                db.SaveChanges();

                ViewBag.Message = "Successfully Updated.";
                ViewBag.QuoteNumber = sarid;

                //  update sales agreement total amount
                UpdateContractAmount(sarid);

                //   return RedirectToAction("Edit", new { id = sarid });

            }

            else
            {

                ViewBag.Message = "Failed ! Please try again.";

            }
            return PartialView(list);

        }

        public ActionResult _AddItem(int id)
        {
            var inv = db.INVs.Find(id);

            var p = new INVDET();
            p.InvID = inv.InvID;
            p.SorID = inv.SorID;
            p.SorNo = inv.SorNo;
            p.InvType = inv.InvType;

            //   var getList = GetProductList();

            //  ViewData["ClientsAll"] = GetClientListByUser("CS");
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            //     ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            //  ViewData["ProductData"] = getList;

            var poptions = new List<PriceOption>();
            ViewData["PriceOptionsAll"] = poptions;
            ViewData["seGSTRate"] = GetGstRate();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return PartialView(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddItem(INVDET data)
        {
            var ps = db.Products.Where(x => x.ProductID == -1).FirstOrDefault();

            if ((data.DetType == "PRODUCT") && (data.ItemID != 0))
            {
                ps = db.Products.Where(x => x.ProductID == data.ItemID).FirstOrDefault();
            }

            //decimal costprice = ps.CostPrice;
            //string costcode = Decimal2String(costprice);
            //data.Remark = costcode;

            data.Nett = data.Amount + data.Gst;

            var invdet1 = db.INVDETs.Where(x => x.SorID == data.SorID).ToList();
            double positioncount = invdet1.Count;
            data.Position = positioncount + 1;

            data.ModifiedBy = User.Identity.Name;
            data.ModifiedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.INVDETs.Add(data);
                db.SaveChanges();
            };

            UpdateContractAmount(data.InvID);

            //   AddKivDet(data);

            int bundlecount = 0;



            var totalAmount = db.INVs.Where(x => x.InvID == data.InvID).FirstOrDefault().Nett;
            var detCount = db.INVDETs.Count(x => x.InvID == data.InvID);

            ViewBag.Message = "1";
            var poptions = new List<PriceOption>();
            ViewData["PriceOptionsAll"] = poptions;
            ViewData["seGSTRate"] = GetGstRate();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();

            return Json(new { success = true, totalamount = totalAmount, detcount = detCount, redirectUrl = Url.Action("InvEdit", "Sales", new { id = data.InvID, str = "1" }) });

        }


        public ActionResult _AddDet(int id)
        {
            var p = new INVDET();
            p.InvID = id;

            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
       //     ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();

            return PartialView(p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddDet(INVDET data)
        {

            data.Nett = data.Amount + data.Gst;

            if (ModelState.IsValid)
            {
                db.INVDETs.Add(data);
                db.SaveChanges();
            };

            // add bundle items

            var sku = data.ItemCode;          


                UpdateContractAmount(data.InvID);

            var inv = db.INVs.Where(x => x.InvID == data.InvID).FirstOrDefault();
            var p = db.Products.Where(x => x.ProductID == data.ItemID).FirstOrDefault();

            if ((inv != null) && (p != null)) {

                var st = new StockTransaction();
                st.ProductID = p.ProductID;
                st.SKU = p.SKU;
                st.LocationID = inv.LocationID;
                st.ProcessType = "ALLOCATE";
                st.Qty = data.Qty;
                st.RefNo = inv.InvID.ToString();
                st.SourceFrom = "Sales";
                st.Remark = inv.CustName;

                st.CreatedBy = User.Identity.Name;
                st.CreatedOn = DateTime.Now;

                CreateStockTransaction(st);
                UpdateWarehouseStock(st.LocationID, st.ProductID, st.Qty, st.ProcessType);
                UpdateStockDet(st.ProductID, st.Qty, st.ProcessType);
                UpdateStock(st.ProductID, st.Qty, st.ProcessType);

            }


            ViewBag.Message = "1";

            return Json(new { redirectUrl = Url.Action("Edit", "Sales", new { id = data.InvID } ) });

        }

        private void CreateStockTransaction(StockTransaction st)
        {

            if (ModelState.IsValid)
            {
                db.StockTransactions.Add(st);
                db.SaveChanges();

            };
        }

        private void UpdateWarehouseStock(int locationId, int productId, double qty, string processType)
        {
            var p = db.WarehouseStocks.Where(x => (x.LocationID == locationId) && (x.ProductID == productId)).FirstOrDefault();

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

        private void UpdateStockDet(int productId, double qty, string processType)
        {
            var p = db.StockDets.Where(x => x.ProductID == productId).FirstOrDefault();

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



      //  [HttpGet]
        public JsonResult _AddOverallDiscount(string valInvID, string valDiscount, string valAmount, string valGst, string valNett)
        {
            int id = Convert.ToInt32(valInvID);
            decimal discount = Convert.ToDecimal(valDiscount);
            decimal amount = Convert.ToDecimal(valAmount);
            decimal gst = Convert.ToDecimal(valGst);
            decimal nett = Convert.ToDecimal(valNett);
 
            var inv = db.INVs.Find(id);
            if (inv == null)
            {
                return null;
            };
            inv.Discount = discount;
            inv.Amount = amount;
            inv.Gst = gst;
            inv.Nett = nett;

            if (ModelState.IsValid)
            {
                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }





        // Sales Items

        public ActionResult AddSalesItemEdit(int id)
        {
            INVDET det = new INVDET();

            INV inv = db.INVs.Find(id);
            if (inv == null)
            {
                inv.InvID = 0;
            }
            else
            {
                det.InvID = inv.InvID;
            };


            return PartialView(det);
        }

        [HttpPost]
        public ActionResult AddSalesItemEdit(INVDET det)
        {
            if (ModelState.IsValid)
            {
                db.Entry(det).State = EntityState.Modified;
                db.INVDETs.Add(det);
                db.SaveChanges();

                // update total sales amount

                UpdateContractAmount(det.InvID); 

                return RedirectToAction("Edit", new { id = det.InvID });
            }
            return View(det);
        }



        public ActionResult AddSalesItem(int id)
        {
            INVDET det = new INVDET();

            INV inv = db.INVs.Find(id);
            if (inv == null)
            {
                inv.InvID = 0;
            }
            else
            {
                det.InvID = inv.InvID;
            };


            return PartialView(det);
        }

        [HttpPost]
        public ActionResult AddSalesItem(INVDET det)
        {
            if (ModelState.IsValid)
            {
                db.Entry(det).State = EntityState.Modified;
                db.INVDETs.Add(det);
                db.SaveChanges();

                return RedirectToAction("Edit", new { id = det.InvID});
            }
            return View(det);
        }

 

        [HttpPost]
        public ActionResult _UpdateSalesItem([Bind(Include = "InvID,Qty,UnitPrice,Amount")] INVDET det)
        {

            if (ModelState.IsValid)
            {
                db.Entry(det).State = EntityState.Modified;
                db.SaveChanges();

                UpdateContractAmount(det.InvID); 

                return RedirectToAction("Manage", new { id = det.InvID });
            };

            ViewBag.Message = "Not updated.";

            return PartialView(det);
        }



        private void UpdateContractAmount(int id)
        {
         //   decimal sumPreDiscAmount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.PreDiscAmount) ?? 0;
         //   decimal sumDiscount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.Discount) ?? 0;
            decimal sumAmount = db.INVDETs.Where(c => c.InvID == id).Sum(c => (decimal?)c.Amount) ?? 0;

            decimal gst = GetGstRate();

       //     decimal sumGst = sumAmount * gst;
       //     decimal sumNett = sumAmount + sumGst;

            INV inv = db.INVs.Find(id);
            if (inv != null)
            {
                inv.PreDiscAmount = sumAmount;
            //    inv.Discount = sumDiscount;

                inv.Amount = sumAmount + inv.Discount;
                inv.Gst = System.Math.Round(inv.Amount * gst, 2, MidpointRounding.AwayFromZero); 
                inv.Nett = inv.Amount + inv.Gst;

                if (ModelState.IsValid)
                {
                    db.Entry(inv).State = EntityState.Modified;
                    db.SaveChanges();
                };
            };

        }


        [HttpGet]
        public JsonResult _DelItem(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var det = db.INVDETs.Find(id);
                double position = det.Position;

                int invid = det.InvID;

                var dets = db.INVDETs.Where(x => x.InvID == det.InvID && x.RefItemID == det.DetID).ToList();
                foreach (var dd in dets)
                {
                    db.Entry(dd).State = EntityState.Deleted;
                }

                //   db.INVDETs.Remove(det);
                db.Entry(det).State = EntityState.Deleted;

                db.SaveChanges();

                //  update sales agreement total amount

                UpdateContractAmount(invid);

                var detlist = db.INVDETs.Where(x => x.InvID == invid && x.Position > position).ToList();

                foreach (var item in detlist)
                {
                    item.Position -= 1;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();

                }

                trans.Complete();
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult _ItemMoveUp(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var inv = db.INVDETs.Where(x => x.DetID == id).FirstOrDefault();
                double currentposition = inv.Position;

                if (!inv.IsBundle)
                {
                    var nextinv = db.INVDETs.Where(x => x.InvID == inv.InvID && x.Position == currentposition - 1).FirstOrDefault();
                    nextinv.Position += 1;
                    inv.Position -= 1;

                    db.Entry(nextinv).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(inv).State = EntityState.Modified;
                    db.SaveChanges();

                    if (nextinv.IsBundle)
                    {
                        var bundleinv = db.INVDETs.Where(x => x.InvID == inv.InvID && x.RefItemID == nextinv.DetID).ToList();

                        foreach (var item in bundleinv)
                        {
                            item.Position += 1;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    if (inv.SalesType == "DefaultItem")
                    {
                        var nextinv = db.INVDETs.Where(x => x.InvID == inv.InvID && x.Position == currentposition - 1).FirstOrDefault();
                        var bundleinv = db.INVDETs.Where(x => x.InvID == inv.InvID && x.RefItemID == inv.DetID).ToList();

                        foreach (var item in bundleinv)
                        {
                            item.Position -= 1;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();

                        }
                        nextinv.Position += 1;
                        inv.Position -= 1;

                        db.Entry(nextinv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();


                        if (nextinv.IsBundle)
                        {
                            bundleinv = db.INVDETs.Where(x => x.InvID == inv.InvID && x.RefItemID == nextinv.DetID).ToList();

                            foreach (var item in bundleinv)
                            {
                                item.Position += 1;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                    else if (inv.SalesType == "BundleItem" || inv.SalesType == "FOCItem")
                    {
                        currentposition -= 0.1;
                        var nextinv = db.INVDETs.Where(x => x.InvID == inv.InvID && x.Position == Math.Round(currentposition, 1)).FirstOrDefault();

                        nextinv.Position = Math.Round(nextinv.Position + 0.1, 1);
                        inv.Position = Math.Round(inv.Position - 0.1, 1);

                        db.Entry(nextinv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                trans.Complete();
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }



        public ActionResult _DelDet(int id = 0)
        {
            INVDET det = db.INVDETs.Find(id);
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
                //var det = db.INVDETs.Find(id);
                //db.INVDETs.Remove(det);
                //db.SaveChanges();

                ////  update sales agreement total amount

                //UpdateContractAmount(det.InvID); 
                var det = db.INVDETs.Find(id);
                double position = det.Position;

                int SorID = det.SorID;

                var dets = db.INVDETs.Where(x => x.SorID == det.SorID && x.RefItemID == det.DetID).ToList();
                foreach (var dd in dets)
                {
                    db.Entry(dd).State = EntityState.Deleted;
                }

                //   db.INVDETs.Remove(det);
                db.Entry(det).State = EntityState.Deleted;

                db.SaveChanges();

                //  update sales agreement total amount

                UpdateContractAmount(SorID);

                var detlist = db.INVDETs.Where(x => x.SorID == SorID && x.Position > position).ToList();

                foreach (var item in detlist)
                {
                    var bundleitem = db.KIVDETs.Where(y => y.SorID == SorID && y.InvDetID == item.DetID).FirstOrDefault();
                    item.Position -= 1;
                    bundleitem.Position -= 1;
                    db.Entry(item).State = EntityState.Modified;
                    db.Entry(bundleitem).State = EntityState.Modified;
                    db.SaveChanges();
                }

                trans.Complete();
            }

            return Json(new { success = true });
        }


        public ActionResult _Summary(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            else
            {
                var inv = db.INVs.Find(id);
                var det = new List<INVDET>();
                det = db.INVDETs
                .Where(x => (x.InvID == id))
                .OrderBy(x => x.DetID)
                .ToList();

                var qv = new InvView
                {
                    InvOn = inv,
                    InvDetOn = det
                };

                ViewBag.InvNumber = id;


                return PartialView(qv);
            }

        }



        public ActionResult InvPrint(int id)
        {
            INV sar = db.INVs.Find(id);         
            
            return View(sar);
        }

        public ActionResult _DisplaySalesItems(int id)
        {
            var p = new List<INVDET>();
            p = db.INVDETs
                .Where(x => (x.InvID == id))
                .OrderBy(x => x.Position)
                .ToList();

            return PartialView(p);
        }

        // ********** Paid / Unpaid process


        public ActionResult PaidUnpaid(int? id)
        {
            ViewData["InvoicesAll"] = db.INVs.OrderBy(x => x.InvID).ToList();

            if (id == null)
            {
                var inv = new INV();

                return View(inv);
            }
            else
            {
                INV inv = db.INVs.Find(id);
                if (inv == null)
                {
                    return HttpNotFound();
                }
                else
                {
                    return View(inv);
                }

            };

        }

        public JsonResult _PaidUnpaid(string valInvID, string valDate, string valType)
        {
            int id = Convert.ToInt32(valInvID);

            if (valType == "Paid")
            {
                if (!string.IsNullOrEmpty(valDate))
                {
                    var inv = db.INVs.Find(id);
                    inv.IsPaid = true;
                    inv.PaidDate = Convert.ToDateTime(valDate);
                    inv.ModifiedBy = User.Identity.Name;
                    inv.ModifiedOn = DateTime.Now;

                    if (ModelState.IsValid)
                    {
                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();
                    };

                };

            }
            else if (valType == "Unpaid")
            {
                var inv = db.INVs.Find(id);
                inv.IsPaid = false;
                inv.PaidDate = null;
                inv.ModifiedBy = User.Identity.Name;
                inv.ModifiedOn = DateTime.Now;

                if (ModelState.IsValid)
                {
                    db.Entry(inv).State = EntityState.Modified;
                    db.SaveChanges();
                };
            };


            return Json(new { success = true }, JsonRequestBehavior.AllowGet);

            //     return Json(null, JsonRequestBehavior.AllowGet);
        }


       

        public JsonResult AutoComplete(string search)
        {
            var data = db.Clients
                       .Where(x => ((x.CustName.ToUpper().StartsWith(search.ToUpper())) || (x.CustNo.ToString().StartsWith(search))) && ((x.IsActive == true)))
                       .ToList().Distinct().ToList();

            //   var result = data.Where(x => x.HeatNo.ToLower().StartsWith(search.ToLower())).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //  [HttpPost]
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


        public List<ProductSelection> GetProductList()
        {
            List<ProductSelection> getList = (from p in db.Products
                                              where p.IsActive == true
                                              select new ProductSelection
                                              {
                                                  ProductID = p.ProductID,
                                                  SKU = p.SKU,
                                                  ModelNo = p.ModelNo,
                                                  ProductName = p.ProductName,
                                                  ProductType = p.ProductType,
                                                  Unit = p.Unit,
                                                  CostPrice = p.CostPrice,
                                                  SellPrice = p.RetailPrice,
                                                  AvailableQty = 0

                                              }).ToList();

            return getList;

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



            var result = from p in getList
                         let w = p.ProductName.ToUpper().Split(new char[] { '.', '-', '!', ' ', ';', ':', ',' },
                                                    StringSplitOptions.RemoveEmptyEntries)
                         where (w.Distinct().Intersect(words).Count() == words.Count()) || (p.SKU.ToUpper().StartsWith(search.ToUpper()))
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

        //  [HttpPost]
        public JsonResult AutoCompleteSelected_Product(string search)
        {
            if (search != null)
            {
                var getList = GetProductList();

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

        public ActionResult MultiPagesPrintPage(int id)
        {
            SalesOrder sar = db.SalesOrders.Find(id);

            var count = db.INVDETs.Count(x => x.SorID == id);

            ViewBag.ItemCount = count;

            return View(sar);
        }

        public ActionResult _MultiPagesPrintModal(int id)
        {
            SalesOrder sar = db.SalesOrders.Find(id);

            SalesOrderSelection o = new SalesOrderSelection()
            {
                SorID = sar.SorID,
                InvType = sar.InvType,
                InvDate = sar.InvDate,
                PoNo = sar.PoNo,
                CustNo = sar.CustNo,
                CustName = sar.CustName,
                Addr1 = sar.Addr1,
                Addr2 = sar.Addr2,
                Addr3 = sar.Addr3,
                Addr4 = sar.Addr4,
                Attn = sar.Attn,
                DeliveryAddress = sar.DeliveryAddress,
                DeliveryDate = sar.DeliveryDate,
                DeliveryTime = sar.DeliveryTime,
                PreDiscAmount = sar.PreDiscAmount,
                Discount = sar.Discount,
                Amount = sar.Amount,
                Gst = sar.Gst,
                Nett = sar.Nett,
                PaidAmount = sar.PaidAmount,
                Status = sar.Status,
                PaymentStatus = sar.PaymentStatus,
                PaymentTerms = sar.PaymentTerms,
                LocationID = sar.LocationID,
                LocationName = sar.LocationName,
                Remark = sar.Remark,
                PersonID = sar.PersonID,
                PersonName = sar.PersonName,
                IsPaid = sar.IsPaid,
                PaidDate = sar.PaidDate,
                CreatedBy = sar.CreatedBy,
                CreatedOn = sar.CreatedOn,
                ModifiedBy = sar.ModifiedBy,
                ModifiedOn = sar.ModifiedOn
            };

            o.SalesPaymentMethodList = (from t1 in db.SalesPaymentMethods
                                        where t1.SorID == o.SorID
                                        select new
                                        {
                                            a = t1
                                        }).ToList().Select(x => new SalesPaymentMethod()
                                        {
                                            SalesPaymentMethodID = x.a.SalesPaymentMethodID,
                                            SorID = x.a.SalesPaymentMethodID,
                                            PaymentMethod = x.a.PaymentMethod,
                                            Amount = x.a.Amount,
                                            CreatedBy = x.a.CreatedBy,
                                            CreatedOn = x.a.CreatedOn
                                        });

            var count = db.INVDETs.Count(x => x.SorID == id);

            var salespay = db.SalesPaymentMethods.Where(x => x.SorID == id).ToList();

            decimal sPaidAmount = 0;
            foreach (var s in salespay)
            {
                sPaidAmount += s.Amount;
            }

            ViewData["seSalesPaymentAmount"] = sPaidAmount;

            ViewBag.ItemCount = count;

            return PartialView(o);
        }

        [HttpGet]
        public void SubmitPaymentMethod(int SorID, List<SalesPaymentMethod> PaymentMethodList, Boolean FullPayment)
        {
            SalesPaymentMethod o = new SalesPaymentMethod();
            if (PaymentMethodList.Count > 0)
            {
                for (int i = 0; i <= PaymentMethodList.Count - 1; i++)
                {
                    o = new SalesPaymentMethod()
                    {
                        SorID = SorID,
                        PaymentMethod = PaymentMethodList[i].PaymentMethod,
                        Amount = PaymentMethodList[i].Amount,
                        ChequeNumber = PaymentMethodList[i].ChequeNumber,
                        IsFullPayment = FullPayment
                    };

                    db.SalesPaymentMethods.Add(o);
                    db.SaveChanges();
                }
            }
        }

        [HttpGet]
        public JsonResult _SubmitSalesOrderPreview(int SorID, Boolean CheckBoxCash, Boolean CheckBoxNETS, Boolean CheckBoxCreditCard, Boolean CheckBoxCheque, string CheckBoxCashAmount, string CheckBoxNETSAmount, string CheckBoxCreditCardAmount, string CheckBoxChequeAmount, string CheckBoxChequeNumber)
        {
            SalesOrder oinv = db.SalesOrders.Find(SorID);
            Client client = db.Clients.Find(oinv.CustNo);
            decimal dPaymentAmount = 0;
            decimal dOriginalNett = oinv.Nett;
            decimal dTotalPaid = 0;
            decimal dOutstandingAmount = 0;
            Boolean bFullPayment = false;

            if (CheckBoxCash)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxCashAmount);
            }

            if (CheckBoxNETS)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxNETSAmount);
            }

            if (CheckBoxCreditCard)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxCreditCardAmount);
            }

            if (CheckBoxCheque)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxChequeAmount);
            }

            if (dOriginalNett == dPaymentAmount)
                bFullPayment = true;

            var dbpayment = db.SalesPaymentMethods.ToList().Where(x => x.SorID == SorID);

            foreach (var item in dbpayment)
            {
                dTotalPaid += item.Amount;
            }

            dOutstandingAmount = dOriginalNett - dTotalPaid;

            if (dOutstandingAmount < dPaymentAmount)
                return Json(new { success = false, responseText = "The total amount (" + dPaymentAmount.ToString("C") + ") you input is more than the outstanding amount (" + dOutstandingAmount.ToString("C") + "). Please change again." }, JsonRequestBehavior.AllowGet);

            if (client.AccType == "CR")
            {
                //Check credit limit
            }

            int id = Convert.ToInt32(SorID);

            var sor = db.SalesOrders.Find(id);
            if (sor == null)
            {
                return Json(new { success = false, responseText = "The sales order is not found. No valid data. Please refresh page." }, JsonRequestBehavior.AllowGet);
            };

            var oinvoice = db.INVs.Where(x => x.SorID == SorID).FirstOrDefault();
            INV invs = new INV();
            string sInvoiceStatus = "";
            Boolean IsPaid = false;

            invs = oinvoice;

            if (dOutstandingAmount == dPaymentAmount)
            {
                sor.Status = "Invoiced, Full Paid";
                sInvoiceStatus = "Full Paid";
                IsPaid = true;
            }
            else
            {
                sor.Status = "Invoiced, Partial Paid";
                sInvoiceStatus = "Partially Paid";
            }

            if (ModelState.IsValid)
            {
                db.Entry(sor).State = EntityState.Modified;
                db.SaveChanges();

                invs.Status = sInvoiceStatus;
                invs.IsPaid = IsPaid;
                invs.PaidDate = DateTime.Now;

                db.Entry(invs).State = EntityState.Modified;
                db.SaveChanges();

            };

            List<SalesPaymentMethod> l = new List<SalesPaymentMethod>();
            SalesPaymentMethod oPay = new SalesPaymentMethod();
            if (CheckBoxCash)
            {
                oPay = new SalesPaymentMethod()
                {
                    SorID = SorID,
                    InvID = invs.InvID,
                    PaymentMethod = "Cash",
                    IsFullPayment = bFullPayment,
                    Amount = Convert.ToDecimal(CheckBoxCashAmount),
                    CreatedBy = User.Identity.Name,
                    CreatedOn = DateTime.Now
                };
                l.Add(oPay);
            }

            if (CheckBoxNETS)
            {
                oPay = new SalesPaymentMethod()
                {
                    SorID = SorID,
                    InvID = invs.InvID,
                    PaymentMethod = "NETS",
                    IsFullPayment = bFullPayment,
                    Amount = Convert.ToDecimal(CheckBoxNETSAmount),
                    CreatedBy = User.Identity.Name,
                    CreatedOn = DateTime.Now
                };
                l.Add(oPay);
            }

            if (CheckBoxCreditCard)
            {
                oPay = new SalesPaymentMethod()
                {
                    SorID = SorID,
                    InvID = invs.InvID,
                    PaymentMethod = "Credit Card",
                    IsFullPayment = bFullPayment,
                    Amount = Convert.ToDecimal(CheckBoxCreditCardAmount),
                    CreatedBy = User.Identity.Name,
                    CreatedOn = DateTime.Now
                };
                l.Add(oPay);
            }

            if (CheckBoxCheque)
            {
                oPay = new SalesPaymentMethod()
                {
                    SorID = SorID,
                    InvID = invs.InvID,
                    PaymentMethod = "Cheque",
                    IsFullPayment = bFullPayment,
                    Amount = Convert.ToDecimal(CheckBoxChequeAmount),
                    ChequeNumber = CheckBoxChequeNumber,
                    CreatedBy = User.Identity.Name,
                    CreatedOn = DateTime.Now
                };
                l.Add(oPay);
            }

            if (l.Count > 0)
            {
                for (int i = 0; i <= l.Count - 1; i++)
                {
                    db.SalesPaymentMethods.Add(l[i]);
                    db.SaveChanges();
                }
            }

            return Json(new
            {
                redirectUrl = Url.Action("Edit", "Sales", new { id = invs.InvID }),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }

        //[HttpGet]
        //public ActionResult _ItemMoveUp(int id)
        //{
        //    using (TransactionScope trans = new TransactionScope())
        //    {
        //        var inv = db.INVDETs.Where(x => x.DetID == id).FirstOrDefault();
        //        var kiv = db.KIVDETs.Where(x => x.InvDetID == id).FirstOrDefault();
        //        double currentposition = inv.Position;

        //        if (!inv.IsBundle)
        //        {
        //            var nextinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition - 1).FirstOrDefault();
        //            var nextkiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition - 1).FirstOrDefault();
        //            nextinv.Position += 1;
        //            inv.Position -= 1;
        //            nextkiv.Position += 1;
        //            kiv.Position -= 1;

        //            db.Entry(nextinv).State = EntityState.Modified;
        //            db.SaveChanges();

        //            db.Entry(inv).State = EntityState.Modified;
        //            db.SaveChanges();

        //            db.Entry(nextkiv).State = EntityState.Modified;
        //            db.SaveChanges();

        //            db.Entry(kiv).State = EntityState.Modified;
        //            db.SaveChanges();

        //            if (nextinv.IsBundle)
        //            {
        //                var bundleinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == nextinv.DetID).ToList();

        //                foreach (var item in bundleinv)
        //                {
        //                    item.Position += 1;
        //                    db.Entry(item).State = EntityState.Modified;
        //                    db.SaveChanges();

        //                    var bundlekiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
        //                    bundlekiv.Position += 1;
        //                    db.Entry(bundlekiv).State = EntityState.Modified;
        //                    db.SaveChanges();
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (inv.SalesType == "DefaultItem")
        //            {
        //                var nextinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition - 1).FirstOrDefault();
        //                var bundleinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == inv.DetID).ToList();
        //                var nextkiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition - 1).FirstOrDefault();

        //                foreach (var item in bundleinv)
        //                {
        //                    item.Position -= 1;
        //                    db.Entry(item).State = EntityState.Modified;
        //                    db.SaveChanges();

        //                    var bundlekiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
        //                    bundlekiv.Position -= 1;
        //                    db.Entry(bundlekiv).State = EntityState.Modified;
        //                    db.SaveChanges();
        //                }
        //                nextinv.Position += 1;
        //                inv.Position -= 1;
        //                nextkiv.Position += 1;
        //                kiv.Position -= 1;

        //                db.Entry(nextinv).State = EntityState.Modified;
        //                db.SaveChanges();

        //                db.Entry(inv).State = EntityState.Modified;
        //                db.SaveChanges();

        //                db.Entry(nextkiv).State = EntityState.Modified;
        //                db.SaveChanges();

        //                db.Entry(kiv).State = EntityState.Modified;
        //                db.SaveChanges();

        //                if (nextinv.IsBundle)
        //                {
        //                    bundleinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == nextinv.DetID).ToList();

        //                    foreach (var item in bundleinv)
        //                    {
        //                        item.Position += 1;
        //                        db.Entry(item).State = EntityState.Modified;
        //                        db.SaveChanges();

        //                        var bundlekiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
        //                        bundlekiv.Position += 1;
        //                        db.Entry(bundlekiv).State = EntityState.Modified;
        //                        db.SaveChanges();
        //                    }
        //                }
        //            }
        //            else if (inv.SalesType == "BundleItem" || inv.SalesType == "FOCItem")
        //            {
        //                currentposition -= 0.1;
        //                var nextinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.Position == Math.Round(currentposition, 1)).FirstOrDefault();
        //                var nextkiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.Position == Math.Round(currentposition, 1)).FirstOrDefault();
        //                nextinv.Position = Math.Round(nextinv.Position + 0.1, 1);
        //                inv.Position = Math.Round(inv.Position - 0.1, 1);
        //                nextkiv.Position = Math.Round(nextkiv.Position + 0.1, 1);
        //                kiv.Position = Math.Round(kiv.Position - 0.1, 1);

        //                db.Entry(nextinv).State = EntityState.Modified;
        //                db.SaveChanges();

        //                db.Entry(inv).State = EntityState.Modified;
        //                db.SaveChanges();

        //                db.Entry(nextkiv).State = EntityState.Modified;
        //                db.SaveChanges();

        //                db.Entry(kiv).State = EntityState.Modified;
        //                db.SaveChanges();
        //            }
        //        }
        //        trans.Complete();
        //    }

        //    return Json(new
        //    {

        //    }, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public ActionResult _ItemMoveDown(int id)
        {
            using (TransactionScope trans = new TransactionScope())
            {
                var inv = db.INVDETs.Where(x => x.DetID == id).FirstOrDefault();
                var kiv = db.KIVDETs.Where(x => x.InvDetID == id).FirstOrDefault();
                double currentposition = inv.Position;

                if (!inv.IsBundle)
                {
                    var nextinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition + 1).FirstOrDefault();
                    var nextkiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition + 1).FirstOrDefault();
                    nextinv.Position -= 1;
                    nextkiv.Position -= 1;
                    inv.Position += 1;
                    kiv.Position += 1;

                    db.Entry(nextinv).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(nextkiv).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(inv).State = EntityState.Modified;
                    db.SaveChanges();

                    db.Entry(kiv).State = EntityState.Modified;
                    db.SaveChanges();

                    if (nextinv.IsBundle)
                    {
                        var bundleinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == nextinv.DetID).ToList();

                        foreach (var item in bundleinv)
                        {
                            item.Position -= 1;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();

                            var bundlekiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
                            bundlekiv.Position -= 1;
                            db.Entry(bundlekiv).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    if (inv.SalesType == "DefaultItem")
                    {
                        var nextinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition + 1).FirstOrDefault();
                        var nextkiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.Position == currentposition + 1).FirstOrDefault();

                        if (nextinv == null)
                        {
                            return Json(new { success = false, responseText = "You cannot move it down as it is the last item." }, JsonRequestBehavior.AllowGet);
                        }

                        var bundleinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == inv.DetID).ToList();

                        foreach (var item in bundleinv)
                        {
                            item.Position += 1;
                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();

                            var bundlekiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
                            bundlekiv.Position += 1;
                            db.Entry(bundlekiv).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        nextinv.Position -= 1;
                        inv.Position += 1;
                        nextkiv.Position -= 1;
                        kiv.Position += 1;

                        db.Entry(nextinv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(nextkiv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(kiv).State = EntityState.Modified;
                        db.SaveChanges();

                        if (nextinv.IsBundle)
                        {
                            bundleinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.RefItemID == nextinv.DetID).ToList();

                            foreach (var item in bundleinv)
                            {
                                item.Position -= 1;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();

                                var bundlekiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.InvDetID == item.DetID).FirstOrDefault();
                                bundlekiv.Position -= 1;
                                db.Entry(bundlekiv).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                    else if (inv.SalesType == "BundleItem" || inv.SalesType == "FOCItem")
                    {
                        var nextinv = db.INVDETs.Where(x => x.SorID == inv.SorID && x.Position == Math.Round(currentposition + 0.1, 1)).FirstOrDefault();
                        var nextkiv = db.KIVDETs.Where(x => x.SorID == inv.SorID && x.Position == Math.Round(currentposition + 0.1, 1)).FirstOrDefault();

                        if (nextinv == null)
                        {
                            return Json(new { success = false, responseText = "You cannot move it down as it is the last item." }, JsonRequestBehavior.AllowGet);
                        }
                        //if (nextinv.SalesType == "FOCItem")
                        //{
                        //    return Json(new { success = false, responseText = "You are not allowed to move it down as there is FOC item below." }, JsonRequestBehavior.AllowGet);
                        //}

                        nextinv.Position = Math.Round(nextinv.Position - 0.1, 1);
                        inv.Position = Math.Round(inv.Position + 0.1, 1);
                        nextkiv.Position = Math.Round(nextkiv.Position - 0.1, 1);
                        kiv.Position = Math.Round(kiv.Position + 0.1, 1);

                        db.Entry(nextinv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(inv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(nextkiv).State = EntityState.Modified;
                        db.SaveChanges();

                        db.Entry(kiv).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                trans.Complete();
            }

            return Json(new
            {

            }, JsonRequestBehavior.AllowGet);
        }


        //************************* For Credit Sales *******************************************************// 

        public ActionResult CreditSales()
        {
            return View();
        }

        [HttpGet]
        public JsonResult _SubmitPayment(int InvID, Boolean CheckWithPayment, Boolean CheckBoxCash, Boolean CheckBoxNETS, Boolean CheckBoxCreditCard, Boolean CheckBoxCheque, Boolean CheckBoxCreditNote, string CheckBoxCashAmount, string CheckBoxNETSAmount, string CheckBoxCreditCardAmount, string CheckBoxChequeAmount, string CheckBoxCreditNoteAmount, string CheckBoxNETSNumber, string CheckBoxCreditCardNumber, string CheckBoxChequeNumber, string CheckBoxCreditNoteNumber)
        {
            // Pay current invoice
            var inv = db.INVs.Find(InvID);
            if (inv == null)
            {
                return Json(new { success = false, responseText = "Invoice not found." }, JsonRequestBehavior.AllowGet);
            }
            Client client = db.Clients.Find(inv.CustNo);

            if (client == null)
            {
                return Json(new { success = false, responseText = "Customer not in the system, please check." }, JsonRequestBehavior.AllowGet);
            }


            decimal dPaymentAmount = 0;
            decimal dOriginalNett = inv.Nett;
            decimal dTotalPaid = 0;
            decimal dOutstandingAmount = 0;
            Boolean bFullPayment = false;

            Boolean isWithPayment = CheckWithPayment;

            if (CheckBoxCash)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxCashAmount);
            }

            if (CheckBoxNETS)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxNETSAmount);
            }

            if (CheckBoxCreditCard)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxCreditCardAmount);
            }

            if (CheckBoxCheque)
            {
                dPaymentAmount += Convert.ToDecimal(CheckBoxChequeAmount);
            }

            if (dOriginalNett == dPaymentAmount)
                bFullPayment = true;

            var dbpayment = db.SalesPaymentMethods.ToList().Where(x => x.InvID == InvID);

            foreach (var item in dbpayment)
            {
                dTotalPaid += item.Amount;
            }

            dOutstandingAmount = dOriginalNett - dTotalPaid;

            if (dOutstandingAmount < dPaymentAmount)
            {
                return Json(new { success = false, responseText = "The total amount (" + dPaymentAmount.ToString("N") + ") you input is more than the nett amount (" + dOutstandingAmount.ToString("C") + "). Please change again." }, JsonRequestBehavior.AllowGet);
            }

            if (client.AccType == "CR")
            {
                //Check credit limit
            }

            string sInvoiceStatus = "";
            Boolean IsPaid = false;

            if ((isWithPayment) && (dPaymentAmount > 0))
            {
                inv.PaidAmount += dPaymentAmount;

                if (dOutstandingAmount == dPaymentAmount)
                {
                    sInvoiceStatus = "Full Paid";
                    IsPaid = true;
                }
                else if (dOutstandingAmount > dPaymentAmount)
                {
                    sInvoiceStatus = "Partially Paid";
                }

                inv.Status = sInvoiceStatus;
                inv.IsPaid = IsPaid;
                inv.PaidDate = DateTime.Now;

            }
            if (ModelState.IsValid)
            {
                db.Entry(inv).State = EntityState.Modified;
                db.SaveChanges();
            }

            var SorID = 0;
            var sor = db.SalesOrders.Find(inv.SorID);
            if (sor != null)
            {
                SorID = sor.SorID;
            }


            if (isWithPayment)
            {
                List<SalesPaymentMethod> l = new List<SalesPaymentMethod>();
                SalesPaymentMethod oPay = new SalesPaymentMethod();
                if (CheckBoxCash && Convert.ToDecimal(CheckBoxCashAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = SorID,
                        InvID = inv.InvID,
                        PrID = 0,
                        InvType = inv.InvType,
                        PaymentDate = DateTime.Now,
                        RecordedFrom = inv.InvType,

                        PaymentMethod = "Cash",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxCashAmount),
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }

                if (CheckBoxNETS && Convert.ToDecimal(CheckBoxNETSAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = SorID,
                        InvID = inv.InvID,
                        PrID = 0,
                        InvType = inv.InvType,
                        PaymentDate = DateTime.Now,
                        RecordedFrom = inv.InvType,

                        PaymentMethod = "NETS",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxNETSAmount),
                        ChequeNumber = CheckBoxNETSNumber,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }

                if (CheckBoxCreditCard && Convert.ToDecimal(CheckBoxCreditCardAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = SorID,
                        InvID = inv.InvID,
                        PrID = 0,
                        InvType = inv.InvType,
                        PaymentDate = DateTime.Now,
                        RecordedFrom = inv.InvType,

                        PaymentMethod = "Credit Card",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxCreditCardAmount),
                        ChequeNumber = CheckBoxCreditCardNumber,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }

                if (CheckBoxCheque && Convert.ToDecimal(CheckBoxChequeAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = SorID,
                        InvID = inv.InvID,
                        PrID = 0,
                        InvType = inv.InvType,
                        PaymentDate = DateTime.Now,
                        RecordedFrom = inv.InvType,

                        PaymentMethod = "Cheque",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxChequeAmount),
                        ChequeNumber = CheckBoxChequeNumber,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }

                if (CheckBoxCreditNote && Convert.ToDecimal(CheckBoxCreditNoteAmount) != 0)
                {
                    oPay = new SalesPaymentMethod()
                    {
                        SorID = SorID,
                        InvID = inv.InvID,
                        PrID = 0,
                        InvType = inv.InvType,
                        PaymentDate = DateTime.Now,
                        RecordedFrom = inv.InvType,

                        PaymentMethod = "Credit Note",
                        IsFullPayment = bFullPayment,
                        Amount = Convert.ToDecimal(CheckBoxCreditNoteAmount),
                        ChequeNumber = CheckBoxCreditNoteNumber,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    l.Add(oPay);
                }

                if (l.Count > 0)
                {
                    for (int i = 0; i <= l.Count - 1; i++)
                    {
                        db.SalesPaymentMethods.Add(l[i]);
                        db.SaveChanges();
                    }
                }
            }


            return Json(new
            {
                printUrl = Url.Action("PrintInvoiceAndDO", "Invoice", new { id = inv.InvID }),
                printInvUrl = Url.Action("PrintPreview", "Invoice", new { id = inv.InvID }),
                printDOUrl = Url.Action("DeliveryOrderPrint", "Invoice", new { id = inv.InvID }),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult _VoidOrder(int SorID)
        {
            var inv = db.INVs.Find(SorID);
            if (inv != null)
            {
                inv.Status = "Void";
                inv.ModifiedBy = User.Identity.Name;
                inv.ModifiedOn = DateTime.Now;

                if (ModelState.IsValid)
                {
                    db.Entry(inv).State = EntityState.Modified;
                    db.SaveChanges();
                };
            }

            return Json(new
            {
                success = true,
                printUrl = Url.Action("PrintPreview", "Invoice", new { id = SorID }),
                redirectUrl = Url.Action("CreditSales", "Sales"),
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);


        }



    }
}
