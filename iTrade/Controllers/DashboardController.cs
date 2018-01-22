using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using iTrade.Models;
using Microsoft.AspNet.Identity;

namespace iTrade.Controllers
{
    public class DashboardController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();

        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult SwitchOutlet(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
                if (user != null)
                {
                    user.BranchID = id;
                    UserManager.Update(user);
                }                
            }

            return RedirectToAction("Index", "Dashboard");
            //return View();
        }

        public ActionResult _Sales()
        {
            decimal todaySals = GetTodaySales();
            int totalInv = GetTodayInvs();
            int totalOrd = GetTodayOrders();
            decimal totalUnpaid = GetTotalUnpaid();

            ViewBag.TotalSales = todaySals;
            ViewBag.TotalInvoices = totalInv;
            ViewBag.TotalOrders = totalOrd;
            ViewBag.TotalUnpaid = totalUnpaid;

            return PartialView();
        }

        public ActionResult _DisplayRecentInvs()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            int BranchID = Convert.ToInt32(user.BranchID);

            var p = new List<INV>();

            if (BranchID == 1)
            {
                p = db.INVs.OrderByDescending(x => x.InvID).Take(20).ToList();
            }
            else
            {
                p = db.INVs.Where(x => x.BranchID == BranchID).OrderByDescending(x => x.InvID).Take(20).ToList();
            }

            return PartialView(p);
        }

        public ActionResult _DisplayOutstandingInvs()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            int BranchID = Convert.ToInt32(user.BranchID);

            var p = new List<INV>();

            if (BranchID == 1)
            {
                p = db.INVs.Where(x => x.IsPaid == false).OrderByDescending(x => x.InvID).Take(20).ToList();
            }
            else
            {
                p = db.INVs.Where(x => x.IsPaid == false && x.BranchID == BranchID).OrderByDescending(x => x.InvID).Take(20).ToList();
            }

            return PartialView(p);
        }
        private decimal GetTotalUnpaid()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            int BranchID = Convert.ToInt32(user.BranchID);
            if (BranchID == 1)
            {
                decimal sumAmount = db.INVs.Where(x => x.IsPaid == false && x.Status != "Void").Sum(x => (decimal?)x.Nett) ?? 0;

                return sumAmount;
            }
            else
            {
                decimal sumAmount = db.INVs.Where(x => x.IsPaid == false && x.Status != "Void" && x.BranchID ==BranchID).Sum(x => (decimal?)x.Nett) ?? 0;

                return sumAmount;
            }
        }

        private int GetTodayOrders()
        {
            DateTime d1 = DateTime.Today;

            int countOrd = db.SalesOrders.Count(x => x.InvDate == d1);

            return countOrd; 
        }

        private int GetTodayInvs()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            int BranchID = Convert.ToInt32(user.BranchID);

            DateTime d1 = DateTime.Today;
            if (BranchID == 1)
            {
                int countInv = db.INVs.Count(x => x.InvDate == d1 && x.Status != "Void");

                return countInv;
            }
            else
            {
                int countInv = db.INVs.Count(x => x.InvDate == d1 && x.Status != "Void" && x.BranchID == BranchID);

                return countInv;
            }
        }

        private decimal GetTodaySales()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            int BranchID = Convert.ToInt32(user.BranchID);

            DateTime d1 = DateTime.Today;

            if (BranchID == 1)
            {
                decimal sumAmount = db.INVs.Where(x => DbFunctions.TruncateTime(x.InvDate) == d1 && x.Status != "Void").Sum(x => (decimal?)x.Nett) ?? 0;

                return sumAmount;
            }
            else
            {
                decimal sumAmount = db.INVs.Where(x => DbFunctions.TruncateTime(x.InvDate) == d1 && x.Status != "Void" && x.BranchID == BranchID).Sum(x => (decimal?)x.Nett) ?? 0;

                return sumAmount;
            }
 
        }

        public ActionResult _Accounts()
        {
            return PartialView();
        }

    }
}