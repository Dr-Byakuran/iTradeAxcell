using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iTrade.Models;

namespace iTrade.Controllers
{
    public class FileManagerController : ControllerBase
    {
        private StarDbContext db = new StarDbContext();
        // GET: FileManager
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult GetFileInfos(int draw,int start,int length)
        {
            IQueryable<FileManager> result = db.FileManagers;
            string search = Request.Params["search[value]"];
            if (!string.IsNullOrEmpty(search))
            {
                result= result.Where(p => p.CustomerName.Contains(search) || p.BusinessType.Contains(search) || p.LoginName.Contains(search) || p.No.Contains(search) || p.Path.Contains(search));
            }
            result = result.OrderBy(p => p.Id);
            return Json(new { draw= draw, recordsTotal=db.FileManagers.Count(), recordsFiltered= result.Count(), data= result.Skip(start*length).Take(length) },JsonRequestBehavior.AllowGet);
        }
    }
}