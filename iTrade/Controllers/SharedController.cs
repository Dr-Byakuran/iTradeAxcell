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
        // GET: Shared
        public ActionResult Index()
        {
            return View();
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