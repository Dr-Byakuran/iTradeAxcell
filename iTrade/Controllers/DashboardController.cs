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
    public class DashboardController : Controller
    {
        private StarDbContext db = new StarDbContext();

        // GET: Dashboard
        public ActionResult Index()
        {
            return View();
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
            var p = new List<INV>();

            p = db.INVs.OrderByDescending(x => x.InvID).Take(20).ToList();

            return PartialView(p);
        }

        public ActionResult _DisplayOutstandingInvs()
        {
            var p = new List<INV>();

            p = db.INVs.Where(x => x.IsPaid == false).OrderByDescending(x => x.InvID).Take(20).ToList();

            return PartialView(p);
        }
        private decimal GetTotalUnpaid()
        {
            decimal sumAmount = db.INVs.Where(x => x.IsPaid == false && x.Status != "Void").Sum(x => (decimal?)x.Nett) ?? 0;

            return sumAmount;
        }

        private int GetTodayOrders()
        {
            DateTime d1 = DateTime.Today;

            int countOrd = db.SalesOrders.Count(x => x.InvDate == d1);

            return countOrd; 
        }

        private int GetTodayInvs()
        {
            DateTime d1 = DateTime.Today;

            int countInv = db.INVs.Count(x => x.InvDate == d1 && x.Status != "Void");

            return countInv; 
        }

        private decimal GetTodaySales()
        {
            DateTime d1 = DateTime.Today;

            decimal sumAmount = db.INVs.Where(x => DbFunctions.TruncateTime(x.InvDate) == d1 && x.Status != "Void").Sum(x => (decimal?)x.Nett) ?? 0;

            return sumAmount;
 
        }

        public ActionResult _Accounts()
        {
            return PartialView();
        }

    }
}