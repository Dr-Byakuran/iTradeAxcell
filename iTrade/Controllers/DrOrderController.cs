using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iTrade.Models;
using System.Transactions;

namespace iTrade.Controllers
{
    public class DrOrderController : CustomBaseController
    {       
        // GET: DrOrder
        public ActionResult Index()
        {
            var result = db.DrOrders.OrderByDescending(d => d.Id).ToList();
            return View(result);
        }

        public ActionResult Details(int id=0)
        {
            DrOrder drOrder = new DrOrder();
            if (id>0)
            {
                drOrder = db.DrOrders.Find(id);
                if (drOrder==null)
                {
                    return HttpNotFound();
                }

            }
            ViewData["ClientsAll"] = db.Clients.Where(x => x.IsActive == true).OrderBy(x => x.CustName).ToList();
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["WarehouseAll"] = db.Warehouses.Where(x => x.IsActive == true).OrderBy(x => x.LocationName).ToList();
            ViewData["CreditInfo"] = db.ClientCreditSetting.Where(m => m.CustNo == drOrder.CustNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            return View(drOrder);
        }
        [HttpPost]
        public ActionResult Details(DrOrder drOrder)
        {
            int id = drOrder.Id;
            bool result = false;
            decimal Amount=0, Gst=0, Nett = 0;
            if (ModelState.IsValid)
            {
                DrOrder m = db.DrOrders.Find(id);
                if (m!=null)
                {
                    m.InvRef = drOrder.InvRef;
                    m.CustNo = drOrder.CustNo;
                    m.CustName = drOrder.CustName;
                    m.CustName2 = drOrder.CustName2;
                    m.Addr1 = drOrder.Addr1;
                    m.Addr2 = drOrder.Addr2;
                    m.Addr3 = drOrder.Addr3;
                    m.Addr4 = drOrder.Addr4;
                    m.Attn = drOrder.Attn;
                    m.PhoneNo = drOrder.PhoneNo;
                    m.FaxNo = drOrder.FaxNo;
                    m.DeliveryAddress = drOrder.DeliveryAddress;
                    m.DeliveryDate = drOrder.DeliveryDate;
                    m.DeliveryTime = drOrder.DeliveryTime;
                    m.PaymentTerms = drOrder.PaymentTerms;
                    m.PersonID = drOrder.PersonID;
                    m.PersonName = drOrder.PersonName;
                    
                    m.ModifiedBy = User.Identity.Name;
                    m.ModifiedOn = DateTime.Now;
                    if (drOrder.DrOrderDets.Count>0)
                    {
                        int detId = 0;
                        double qty = 0;
                        //float 
                        foreach (var item in drOrder.DrOrderDets)
                        {
                            #region
                            detId = item.DetID;
                            DrOrderDet det = null;
                            if (detId>0)
                            {
                                det = db.DrOrderDets.Find(detId);                                
                            }
                            else
                            {
                                //det = new DrOrderDet();
                                //det.DorID = id;
                            }   
                            if (det!=null)
                            {
                                #region
                                qty = item.DeliverQty;
                                if (qty == 0)
                                { db.DrOrderDets.Remove(det); }
                                else
                                {
                                    det.DeliverQty = qty;
                                    det.Amount = (decimal)qty * det.UnitPrice;
                                    Amount += det.Amount;
                                    if (det.Gst>0)
                                    {
                                        det.Gst = det.Amount * 0.07M;
                                        det.Nett = det.Amount + det.Gst;
                                        Gst += det.Gst;
                                        Nett += det.Nett;
                                    }
                                }
                                #endregion
                            }
                            #endregion
                        }
                    }
                    m.PreDiscAmount = Amount;
                    m.Amount = Amount;
                    m.Gst = Gst;
                    m.Nett = Nett;

                    db.SaveChanges();
                    result = true;                  
                }
            }                 
            return Json(new { success = result, redirectUrl = Url.Action("Details", "DrOrder", new { id = id }) });
        }

        public JsonResult ConvertToInvoice(int id)
        {
            //string newUrl = "/DrOrder/Details/" + id.ToString();
            string msgErr = "The Delivery Order is not found.No valid data.Please refresh page.";
            DrOrder drOrder = db.DrOrders.Find(id);
            if (drOrder == null)
            {
                goto EndAction;
            }
            if (drOrder.Status!= "Draft")
            {
                msgErr = "The Delivery Order status can not be converted";
                goto EndAction;
            }
            INV invs = new INV();
            using (TransactionScope trans = new TransactionScope())
            {
                int kivOrderDetID;
                double qty;
                //if (drOrder.InvType == "CS")
                //{
                //    newInvNo = GetMaxCashInvoiceNumber();
                //}
                //else
                //{
                //    newInvNo = GetMaxCreditInvoiceNumber();
                //};

                string newInvNo = "";
                var sp = db.Staffs.Find(drOrder.PersonID);
                if (sp != null && !string.IsNullOrEmpty(sp.DepartmentName))
                {
                    if (sp.DepartmentName == "SR")
                    {
                        newInvNo = GetSerialNumber(203);
                    }
                    else
                    {
                        newInvNo = GetSerialNumber(103);
                    }
                }
                else
                {
                    newInvNo = GetSerialNumber(103);
                }

                drOrder.InvRef = drOrder.InvRef;
                drOrder.Status = "Invoiced";

                #region
                invs.InvNo = newInvNo;
                invs.SorID = 0;
                //invs.SorID = sor.SorID;
                invs.InvType = drOrder.InvType;
                invs.InvDate = drOrder.InvDate;
                invs.PoNo = drOrder.PoNo;
                invs.DorNo = drOrder.DorNo;
                invs.InvRef = drOrder.InvRef;
                invs.CustNo = drOrder.CustNo;
                invs.CustName = drOrder.CustName;
                invs.CustName2 = drOrder.CustName2;
                invs.Addr1 = drOrder.Addr1;
                invs.Addr2 = drOrder.Addr2;
                invs.Addr3 = drOrder.Addr3;
                invs.Addr4 = drOrder.Addr4;
                invs.Attn = drOrder.Attn;
                invs.DeliveryAddress = drOrder.DeliveryAddress;
                invs.DeliveryTime = drOrder.DeliveryTime;
                invs.DeliveryDate = drOrder.DeliveryDate;
                invs.PreDiscAmount = drOrder.PreDiscAmount;
                invs.Discount = drOrder.Discount;
                invs.Amount = drOrder.Amount;
                invs.Gst = drOrder.Gst;
                invs.Nett = drOrder.Nett;
                //invs.PaidAmount = drOrder.PaidAmount;
                invs.Status = drOrder.Status;
                invs.PaymentStatus = "Unpaid";//sor.PaymentStatus;
                invs.PaymentTerms = drOrder.PaymentTerms;
                invs.LocationID = drOrder.LocationID;
                invs.LocationName = drOrder.LocationName;
                invs.Remark = drOrder.Remark;
                invs.PersonID = drOrder.PersonID;
                invs.PersonName = drOrder.PersonName;
               
                invs.IsPaid = false;
                invs.CreatedBy = User.Identity.Name;
                invs.CreatedOn = DateTime.Now;
                #endregion

                if (ModelState.IsValid)
                {
                    db.INVs.Add(invs);
                    db.SaveChanges();
                }

                drOrder.InvId = invs.InvID;
                List<DrOrderDet> dets = db.DrOrderDets.Where(d => d.DorID == id).ToList();
                if (dets.Count<=0)
                {
                    msgErr = "The Delivery Order details can not be found";
                    goto EndAction;
                }

                foreach (var item in dets)
                {
                    #region
                    kivOrderDetID = item.KivOrderDetID;
                    qty = item.DeliverQty;
                    KivOrderDet kivOrderDet = db.KivOrderDets.Find(kivOrderDetID);
                    if (kivOrderDet==null)
                    {
                        msgErr = "The Delivery Request details can not be found";
                        goto EndAction;
                    }
                    if (qty> kivOrderDet.BalanceQty)
                    {
                        msgErr = "The item[" + item.ItemCode + "] Qty can not greater than request Balance Qty";
                        goto EndAction;
                    }
                    kivOrderDet.BalanceQty -= qty;
                    INVDET invDet = new INVDET
                    {
                        DetType = item.DetType,
                        InvID = invs.InvID,
                        DrOrderDetId = item.DetID,
                        //InvRef=item.in,
                        ItemID = item.ItemID,
                        ItemCode = item.ItemCode,
                        ItemType = item.ItemType,
                        ItemName = item.ItemName,
                        ItemDesc = item.ItemDesc,
                        SellType = item.SellType,
                        Qty = item.DeliverQty,
                        Unit = item.Unit,
                        UnitPrice = item.UnitPrice,
                        DiscountedPrice = item.DiscountedPrice,
                        Amount = item.Amount,
                        Gst = item.Gst,
                        Nett = item.Nett,
                        IsBundle = item.IsBundle,
                        SalesType = item.SellType,
                        Position = item.Position,
                        Remark = item.Remark,
                        ModifiedBy = User.Identity.Name,
                        ModifiedOn = DateTime.Now
                    };
                    db.INVDETs.Add(invDet);
                    #endregion
                }
                db.SaveChanges();
                double BalanceQty = db.KivOrderDets.Where(p => p.KorID == drOrder.KivOrderId).Sum(p => p.BalanceQty);
                if (BalanceQty==0)
                {
                    KivOrder kivOrder = db.KivOrders.Find(drOrder.KivOrderId);
                    if (kivOrder == null)
                    {
                        msgErr = "The Delivery request  can not be found";
                        goto EndAction;
                    }
                    kivOrder.Status2 = "Completed";
                    db.SaveChanges();
                }
                

                trans.Complete();
            }
                return Json(new
                {
                   // printUrl = Url.Action("PrintInvoiceAndDO", "Invoice", new { id = invs.InvID }),
                    printInvUrl = Url.Action("PrintPreview", "Invoice", new { id = invs.InvID }),
                    printDOUrl = Url.Action("DeliveryOrderPrint", "Invoice", new { id = invs.InvID }),
                    //redirectUrl = Url.Action("OrderProcessed", "Orders", new { id = sor.SorID }),
                    isRedirect = true
                }, JsonRequestBehavior.AllowGet);

            /*return Json(new
            {
                //printUrl = Url.Action("PrintPreview", "Invoice", new { id = 1 }),
                redirectUrl = newUrl,
                isRedirect = true
            }, JsonRequestBehavior.AllowGet);*/
            EndAction:
            return Json(new { success = false, responseText = msgErr}, JsonRequestBehavior.AllowGet);

           
        }
    }
}