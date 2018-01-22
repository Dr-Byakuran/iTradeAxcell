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
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;

namespace iTrade.Controllers
{
    public class StaffsController : Controller
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

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }
        // GET: Staffs
        public ActionResult Index()
        {
            var stafflist = (from t1 in db.Staffs
                             select new
                             {
                                 a = t1
                             }).ToList().Select(y => new StaffModelView()
                             {
                                 StaffID = y.a.StaffID,
                                 FirstName = y.a.FirstName,
                                 LastName = y.a.LastName,
                                 Position = y.a.Position,
                                 DepartmentName = y.a.DepartmentName,
                                 BranchName = y.a.BranchName,
                                 Email = y.a.Email,
                                 MobileNo = y.a.MobileNo,
                                 IsActive = y.a.IsActive,
                                 Remark = y.a.Remark,
                                 CreatedBy = y.a.CreatedBy,
                                 CreatedOn = y.a.CreatedOn,
                                 UserID = y.a.UserID
                             });

            List<StaffModelView> l = new List<StaffModelView>();

            foreach (var item in stafflist)
            {
                if (item.UserID != null && item.UserID != "")
                {
                    var user = UserManager.FindById(item.UserID);
                    if (user != null)
                    {
                        item.DisplayName = user.DisplayName;
                    }
                }

                l.Add((StaffModelView)item);
            }

            return View(l);
        }

        // GET: Staffs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Staff staff = db.Staffs.Find(id);
            if (staff == null)
            {
                return HttpNotFound();
            }
            return View(staff);
        }

        // GET: Staffs/Create
        public ActionResult Create()
        {
            var p = new StaffModelView();
            p.IsActive = true;
            p.CreatedBy = User.Identity.Name;
            p.CreatedOn = DateTime.Now;

            ViewBag.BranchId = new SelectList(db.CompanyBranches.ToList(), "BranchID", "BranchName");

            return View(p);
        }

        // POST: Staffs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "StaffID,FirstName,LastName,Position,DepartmentName,BranchID,BranchName,Email,MobileNo,IsActive,Remark,CreatedBy,CreatedOn,UserID,IsCreateNewUser")] StaffModelView staff)
        {
            Staff o = new Staff();
            o.StaffID = staff.StaffID;
            o.FirstName = staff.FirstName;
            o.LastName = staff.LastName;
            o.Position = staff.Position;
            o.DepartmentName = staff.DepartmentName;
            o.BranchID = staff.BranchID;
            o.BranchName = staff.BranchName;
            o.Email = staff.Email;
            o.MobileNo = staff.MobileNo;
            o.IsActive = staff.IsActive;
            o.UserID = staff.UserID;
            o.CreatedBy = User.Identity.Name;
            o.CreatedOn = DateTime.Now;

            if (ModelState.IsValid)
            {
                db.Staffs.Add(o);
                db.SaveChanges();

                if (staff.IsCreateNewUser)
                    return RedirectToAction("CreateNewUser/" + o.StaffID);
                else
                    return RedirectToAction("Index");
            }

            return View(o);
        }

        // GET: Staffs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Staff staff = db.Staffs.Find(id);
            if (staff == null)
            {
                return HttpNotFound();
            }
            ViewData["seStaffUserList"] = UserManager.Users.ToList().OrderBy(x => x.DisplayName);
            return View(staff);
        }

        // POST: Staffs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "StaffID,FirstName,LastName,Position,DepartmentName,Email,MobileNo,IsActive,Remark,CreatedBy,CreatedOn,UserID")] Staff staff)
        {
            if (ModelState.IsValid)
            {
                db.Entry(staff).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(staff);
        }

        // GET: Staffs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Staff staff = db.Staffs.Find(id);
            if (staff == null)
            {
                return HttpNotFound();
            }
            return View(staff);
        }

        // POST: Staffs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Staff staff = db.Staffs.Find(id);
            db.Staffs.Remove(staff);
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

        public ActionResult CreateNewUser(int id)
        {
            Staff staff = db.Staffs.Find(id);
            StaffRegisterAccount o = new StaffRegisterAccount()
            {
                StaffID = id,
                Email = staff.Email
            };
            ViewBag.RoleId = new SelectList(RoleManager.Roles.ToList(), "Name", "Name");
            return View(o);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateNewUser(StaffRegisterAccount userViewModel, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = userViewModel.Email, Email = userViewModel.Email, DisplayName = userViewModel.DisplayName };
                var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                //Add User to the selected Roles 
                if (adminresult.Succeeded)
                {
                    if (selectedRoles != null)
                    {
                        var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("", result.Errors.First());
                            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                            return View();
                        }
                    }

                    Staff staff = db.Staffs.Find(userViewModel.StaffID);
                    staff.UserID = user.Id;
                    db.Entry(staff).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    ModelState.AddModelError("", adminresult.Errors.First());
                    ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                    return View();

                }
            }
            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");

            return RedirectToAction("Index");
        }

        public JsonResult AutoBranch(string search)
        {
            int branchID = Convert.ToInt32(search);
            if (search != null)
            {
                var c = db.CompanyBranches
                           .Where(x => x.BranchID == branchID)
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
    }
}
