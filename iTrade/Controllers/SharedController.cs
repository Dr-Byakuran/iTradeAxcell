using iTrade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace iTrade.Controllers
{
    public class SharedController : Controller
    {
        private StarDbContext db = new StarDbContext();

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _userManager = value;
            }
        }
        // GET: Shared
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult _UserProfile()
        {
            var branches = db.CompanyBranches.Where(x => x.BranchID != 1 && x.IsActive == true).ToList();

            int oid = 1;
            string outletname = "";
            string accessoutlets = "1";

            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                oid = Convert.ToInt32(user.BranchID);
                var branch = db.CompanyBranches.Find(oid);

                if (branch != null)
                {
                    outletname = branch.BranchName;
                }
                else
                {
                    var br = new CompanyBranch();
                    br.BranchName = "Default";
                    br.CompID = 1;
                    br.IsActive = true;
                    br.CreatedBy = user.Email;
                    br.CreatedOn = DateTime.Now;

                    db.CompanyBranches.Add(br);
                    db.SaveChanges();

                    oid = br.BranchID;
                    outletname = br.BranchName;

                    var comp = db.Companies.Find(br.CompID);
                    //if (comp == null)
                    //{
                    //    var cp = new Company();
                    //    cp.Name = "Your Company Name";
                    //    cp.CreatedBy = user.Email;
                    //    cp.CreatedOn = DateTime.Now;

                    //    db.Companies.Add(cp);
                    //    db.SaveChanges();
                    //}

                }

                var staff = db.Staffs.Where(x => x.Email == user.Email).FirstOrDefault();
                if (staff != null)
                {
                    accessoutlets = staff.BranchID;
                }
            }

            ViewBag.OutletID = oid;
            ViewBag.OutletName = outletname;
            ViewBag.AccessOutlets = accessoutlets;

            return PartialView(branches);
        }

        public ActionResult _UserCompanyLogo()
        {
            string s = User.Identity.GetUserId();
            var data = (from t1 in db.Companies
                        join t2 in db.UserCompanies on t1.ID equals t2.CompanyID
                        where t2.UsersAdminID == s && t2.IsDefault == true
                        select new
                        {
                            a = t1
                        }).ToList()
                        .Select(x => new Company()
                        {
                            ID = x.a.ID,
                            Name = x.a.Name,
                            ChineseName = x.a.ChineseName,
                            Address = x.a.Address,
                            TelephoneNumber = x.a.TelephoneNumber,
                            FaxNumber = x.a.FaxNumber,
                            EmailAddress = x.a.EmailAddress,
                            BusinessRegNo = x.a.BusinessRegNo,
                            GSTRegNo = x.a.GSTRegNo,
                            LogoImage = x.a.LogoImage
                        });

            Company o = new Company();

            foreach (var item in data)
            {
                o = new Company();
                o.ID = item.ID;
                o.Name = item.Name;
                o.ChineseName = item.ChineseName;
                o.Address = item.Address;
                o.TelephoneNumber = item.TelephoneNumber;
                o.FaxNumber = item.FaxNumber;
                o.EmailAddress = item.EmailAddress;
                o.BusinessRegNo = item.BusinessRegNo;
                o.GSTRegNo = item.GSTRegNo;
                o.LogoImage = item.LogoImage;
            }

            return PartialView(o);
        }
    }
}