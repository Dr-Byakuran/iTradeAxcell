using iTrade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace iTrade.Controllers
{
    public class TutorRateController : Controller
    {
        private StarDbContext db = new StarDbContext();
        // GET: TutorRate
        public ActionResult Index(string txtFilter)
        {
            var result = db.Tutors.Take(200).ToList();

            if (!string.IsNullOrEmpty(txtFilter))
            {
                result = db.Tutors.Where(x => x.TutorName.Contains(txtFilter) || x.TutorCode.StartsWith(txtFilter)).Take(200).ToList();
            }

            return View(result);
        }
    }
}