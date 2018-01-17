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
    public class ClientsController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();

        // GET: Clients
        public ActionResult Index(string txtFilter)
        {
            var result = db.Clients.Take(200).ToList();

            if (!string.IsNullOrEmpty(txtFilter))
            {
                result = db.Clients.Where(x => x.CustName.Contains(txtFilter) || x.AccNo.StartsWith(txtFilter)).Take(200).ToList();
            }

            ViewData["BranchAll"] = db.CompanyBranches.Where(x => x.IsActive == true).OrderBy(x => x.BranchID).ToList();

            return View(result);
        }

        // GET: Clients/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Clients.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            return View(client);
        }

        // GET: Clients/Create
        public ActionResult Create()
        {
            ClientViewModel clientViewModel = new ClientViewModel();
            var c = new Client();
            c.IsActive = true;
            c.CreatedBy = User.Identity.Name;
            c.CreatedOn= DateTime.Now;
            c.CreateClientContact(1);

            var cs = new ClientCreditSetting();
            cs.IsCreditAllowed = false;
            cs.CreatedBy = User.Identity.Name;
            cs.CreatedOn = DateTime.Now;

            //var pp = new ContactPerson();
            //pp.IsDefault = true;
            //pp.ModifiedBy = User.Identity.Name;
            //pp.ModifiedOn = DateTime.Now;

            clientViewModel.Client = c;
            clientViewModel.ClientCreditSetting = cs;

            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["BranchAll"] = db.CompanyBranches.Where(x => x.IsActive == true).OrderBy(x => x.BranchID).ToList();

            return View(clientViewModel);
        }

        // POST: Clients/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        //public ActionResult Create([Bind(Include = "CustNO,CustName,Addr1,Addr2,Addr3,PostalCode,Country,PhoneNo,FaxNo,ContactPerson,PrimaryEmail,Website,Group,IsActive,Remark,AssignedTo,SalesPersonID,SalesPersonName,CreatedBy,CreatedOn")] Client client)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ClientViewModel clientViewModel)
        {
            Client client = clientViewModel.Client;
            ClientCreditSetting clientCreditSetting = clientViewModel.ClientCreditSetting;

            var newnum = GetMaxClientNumber();
            client.AccNo = newnum;
            client.CreatedBy = User.Identity.Name;
            client.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                foreach (ClientContact bb in client.ClientContacts.ToList())
                {
                    if (bb.DeleteItem == true)
                    {
                        client.ClientContacts.Remove(bb);
                    }
                    else
                    {
                        bb.CustNo = client.CustNo;
                    }         
                }

                db.Clients.Add(client);
                db.SaveChanges();

                if (clientCreditSetting.IsCreditAllowed == true && clientCreditSetting.CreditLimit > 0 && clientCreditSetting.PaymentTerms > 0)
                {
                    decimal accountBal = 0, overdueLimit = 0;
                    if (clientCreditSetting.AccountBalance == null) clientCreditSetting.AccountBalance = accountBal;
                    if (clientCreditSetting.OverdueLimit == null) clientCreditSetting.OverdueLimit = overdueLimit;

                    db.ClientCreditSetting.Add(clientCreditSetting);
                    clientCreditSetting.CustNo = client.CustNo;
                    clientCreditSetting.CreatedBy = User.Identity.Name;
                    clientCreditSetting.CreatedOn = DateTime.Now;
                    clientCreditSetting.ModifiedBy = User.Identity.Name;
                    clientCreditSetting.ModifiedOn = DateTime.Now;
                    db.SaveChanges();
                }

                return RedirectToAction("Index");
            }

            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["BranchAll"] = db.CompanyBranches.Where(x => x.IsActive == true).OrderBy(x => x.BranchID).ToList();

            return View(clientViewModel);
        }

        // GET: Clients/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClientViewModel clientViewModel = new ClientViewModel();
            Client client = db.Clients.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            ClientCreditSetting clientCreditSetting = new ClientCreditSetting();
            clientCreditSetting = db.ClientCreditSetting.Where(m => m.CustNo == id).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

            if (clientCreditSetting==null)
            {
                ClientCreditSetting _clientCreditSetting = new ClientCreditSetting();
                _clientCreditSetting.IsCreditAllowed = false;

                clientCreditSetting = _clientCreditSetting;
            }

            clientViewModel.Client = client;
            clientViewModel.ClientCreditSetting = clientCreditSetting;

            ViewBag.Status = "GeneralInfo";
            ViewBag.CustNo = id;

            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["BranchAll"] = db.CompanyBranches.Where(x => x.IsActive == true).OrderBy(x => x.BranchID).ToList();

            return View(clientViewModel);
        }

        // POST: Clients/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
         //public ActionResult ManageClient([Bind(Include = "CustNO,CustName,Addr1,Addr2,Addr3,PostalCode,Country,PhoneNo,FaxNo,ContactPerson,PrimaryEmail,Website,Group,IsActive,Remark,AssignedTo,SalesPersonID,SalesPersonName,CreatedBy,CreatedOn")] Client client)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ClientViewModel clientViewModel)
        {
            Client client = new Client();
            ClientCreditSetting clientCreditSetting = new ClientCreditSetting();
            int custNo = 0;
            if (ModelState.IsValid)
            {
                    client = clientViewModel.Client;

                    foreach (var item in client.ClientContacts.ToList())
                    {
                        if (item.DeleteItem == true)
                        {
                            ClientContact p = db.ClientContacts.Where(x => x.PersonID == item.PersonID).FirstOrDefault();
                            if (p != null)
                            {
                                db.ClientContacts.Remove(p);
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            item.CustNo = client.CustNo;
                            db.Entry(item).State = (db.ClientContacts.Any(e => e.PersonID == item.PersonID)) ? EntityState.Modified : EntityState.Added;

                        }

                    }

                    client.ModifiedBy = User.Identity.Name;
                    client.ModifiedOn = DateTime.Now;

                    db.Entry(client).State = EntityState.Modified;
                    db.SaveChanges();

                    custNo = client.CustNo;
           //         ViewBag.Status = "GeneralInfo";
       
 
           //         string strCustNo = Request.Form["strCustNo"];
          //          custNo = int.Parse(strCustNo);
                    clientCreditSetting = clientViewModel.ClientCreditSetting;
                    ClientCreditSetting _clientCreditSetting = new ClientCreditSetting();
                    List<ClientCreditSetting> clientCreditSettingList = db.ClientCreditSetting.Where(m => m.CustNo == custNo).OrderByDescending(m => m.ModifiedOn).ToList();
                    if (clientCreditSettingList.Count > 0)
                    {
                        _clientCreditSetting = clientCreditSettingList.First();
                        _clientCreditSetting.ModifiedBy = User.Identity.Name;
                        _clientCreditSetting.ModifiedOn = DateTime.Now;
                        _clientCreditSetting.CreditLimit = clientCreditSetting.CreditLimit;
                        _clientCreditSetting.IsCreditAllowed = clientCreditSetting.IsCreditAllowed;
                        _clientCreditSetting.PaymentTerms = clientCreditSetting.PaymentTerms;
                        _clientCreditSetting.OverdueLimit = clientCreditSetting.OverdueLimit;
                        db.Entry(_clientCreditSetting).State = EntityState.Modified;
                        db.SaveChanges();

                        clientCreditSetting = _clientCreditSetting;
                    }
                    else
                    {
                        if (clientCreditSetting.IsCreditAllowed == true)
                        {
                            db.ClientCreditSetting.Add(clientCreditSetting);
                            clientCreditSetting.CustNo = custNo;
                            clientCreditSetting.CreatedBy = User.Identity.Name;
                            clientCreditSetting.CreatedOn = DateTime.Now;
                            clientCreditSetting.ModifiedBy = User.Identity.Name;
                            clientCreditSetting.ModifiedOn = DateTime.Now;
                            db.SaveChanges();
                        }
                    }
                //    ViewBag.Status = "FinancialInfo";
              
                //return RedirectToAction("Index");
            }

            ViewBag.CustNo = custNo;

        //    client = db.Clients.Find(custNo);
        //    clientCreditSetting = db.ClientCreditSetting.Where(m => m.CustNo == custNo).OrderByDescending(m => m.ModifiedOn).ToList().FirstOrDefault();

        //    clientViewModel.Client = client;
        //    clientViewModel.ClientCreditSetting = clientCreditSetting;

            ViewBag.Status = Request.Form["selectedTab"];

            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["BranchAll"] = db.CompanyBranches.Where(x => x.IsActive == true).OrderBy(x => x.BranchID).ToList();

            return View(clientViewModel);
        }


        // GET: Clients/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Client client = db.Clients.Find(id);
            if (client == null)
            {
                return HttpNotFound();
            }
            ViewData["StaffsAll"] = db.Staffs.Where(x => x.IsActive == true).OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
            ViewData["BranchAll"] = db.CompanyBranches.Where(x => x.IsActive == true).OrderBy(x => x.BranchID).ToList();

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Client client = db.Clients.Find(id);
            db.Clients.Remove(client);
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
