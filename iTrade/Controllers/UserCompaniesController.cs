using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using iTrade.Models;
using System.Threading.Tasks;
using iTrade.CustomAttributes;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace iTrade.Controllers
{
    public class UserCompaniesController : Controller
    {
        private StarDbContext db = new StarDbContext();
        private CustomErrorMessage _CE = new CustomErrorMessage();

        public UserCompaniesController()
        {

        }

        public UserCompaniesController(ApplicationUserManager userManager, ApplicationRoleManager roleManager, ApplicationPermissionManager permissionManager)
        {
            this.UserManager = userManager;
            this.RoleManager = roleManager;
            this.PermissionManager = permissionManager;
        }

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

        private ApplicationPermissionManager _permissionManager;
        public ApplicationPermissionManager PermissionManager
        {
            get
            {
                return _permissionManager ?? HttpContext.GetOwinContext().Get<ApplicationPermissionManager>();
            }
            private set
            {
                _permissionManager = value;
            }
        }
        // GET: UserCompanies
        public async Task<ActionResult> Index()
        {
            var test = await UserManager.Users.ToListAsync();
            EmployeeView o = new EmployeeView();
            List<EmployeeView> list = new List<EmployeeView>();

            foreach (var item in test)
            {
                o = new EmployeeView()
                {
                    UsersAdminID = item.Id,
                    DisplayName = item.DisplayName,
                    Email = item.Email
                };

                var p = (from t1 in db.Companies
                         join t2 in db.UserCompanies on t1.ID equals t2.CompanyID
                         where t2.UsersAdminID == item.Id
                         select new
                         {
                             a = t1,
                             b = t2
                         }).ToList()
                            .Select(x => new CompanySelection()
                            {
                                ID = x.a.ID,
                                Name = x.a.Name,
                                IsDefault = x.b.IsDefault
                            });

                o.CompanyList = p;

                list.Add(o);
            }
            //var l = (from t1 in db.UserCompanies
            //         from t2 in db.Companies.Where(x => x.ID == t1.CompanyID)
            //         select new
            //         {
            //             a = t1,
            //             b = t2
            //         }).ToList()
            //                                .Select(x => new UserCompanySelection()
            //                                {
            //                                    ID = x.a.ID,
            //                                    UsersAdminID = x.a.UsersAdminID,
            //                                    CompanyID = x.a.CompanyID,
            //                                    CreatedOn = x.a.CreatedOn,
            //                                    CreatedBy = x.a.CreatedBy,
            //                                    ModifiedBy = x.a.ModifiedBy,
            //                                    ModifiedOn = x.a.ModifiedOn,
            //                                    CompanyData = new Company()
            //                                    {
            //                                        Name = x.b.Name,
            //                                        ChineseName = x.b.ChineseName,
            //                                        Address = x.b.Address,
            //                                        TelephoneNumber = x.b.TelephoneNumber,
            //                                        FaxNumber = x.b.FaxNumber,
            //                                        EmailAddress = x.b.EmailAddress,
            //                                        BusinessRegNo = x.b.BusinessRegNo,
            //                                        GSTRegNo = x.b.GSTRegNo,
            //                                        LogoImage = x.b.LogoImage,
            //                                        CreatedBy = x.b.CreatedBy,
            //                                        CreatedOn = x.b.CreatedOn,
            //                                        ModifiedBy = x.b.ModifiedBy,
            //                                        ModifiedOn = x.b.ModifiedOn
            //                                    },
            //                                    UserAdminData = new ApplicationUser()
            //                                    {

            //                                    }
            //                                });

            //foreach (var item in l)
            //{
            //    item.UserAdminData = await UserManager.FindByIdAsync(item.UsersAdminID.ToString());
            //}

            return View(list);
        }

        // GET: UserCompanies/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserCompany userCompany = db.UserCompanies.Find(id);
            if (userCompany == null)
            {
                return HttpNotFound();
            }
            return View(userCompany);
        }

        // GET: UserCompanies/Create
        public ActionResult Create(string id)
        {
            var users = UserManager.Users.ToList();
            Company oC = new Company();
            List<Company> listC = new List<Company>();
            string sName = "";

            foreach (var item in users)
            {
                if (id == item.Id)
                {
                    sName = item.DisplayName;
                }
            }

            foreach (var item in db.Companies.ToList())
            {
                oC = new Company()
                {
                    ID = item.ID,
                    Name = item.Name
                };
                listC.Add(oC);
            }

            UserCompanySelection p = new UserCompanySelection()
            {
                CompanyList = listC,
                UsersAdminID = id,
                DisplayName = sName
            };

            ViewData["seUserCompaniesAll"] = (from t1 in db.UserCompanies
                                              from t2 in db.Companies.Where(x => x.ID == t1.CompanyID)
                                              where t1.UsersAdminID == id
                                              select new
                                              {
                                                  a = t1,
                                                  b = t2
                                              }).ToList()
                                            .Select(x => new UserCompanySelection()
                                            {
                                                ID = x.a.ID,
                                                UsersAdminID = x.a.UsersAdminID,
                                                CompanyID = x.a.CompanyID,
                                                CreatedOn = x.a.CreatedOn,
                                                CreatedBy = x.a.CreatedBy,
                                                ModifiedBy = x.a.ModifiedBy,
                                                ModifiedOn = x.a.ModifiedOn,
                                                IsDefault = x.a.IsDefault,
                                                CompanyData = new Company()
                                                {
                                                    Name = x.b.Name,
                                                    ChineseName = x.b.ChineseName,
                                                    Address = x.b.Address,
                                                    TelephoneNumber = x.b.TelephoneNumber,
                                                    FaxNumber = x.b.FaxNumber,
                                                    EmailAddress = x.b.EmailAddress,
                                                    BusinessRegNo = x.b.BusinessRegNo,
                                                    GSTRegNo = x.b.GSTRegNo,
                                                    LogoImage = x.b.LogoImage,
                                                    CreatedBy = x.b.CreatedBy,
                                                    CreatedOn = x.b.CreatedOn,
                                                    ModifiedBy = x.b.ModifiedBy,
                                                    ModifiedOn = x.b.ModifiedOn
                                                }
                                            });

            return View(p);
        }

        // POST: UserCompanies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UsersAdminID,CompanyID,IsDefault")] UserCompanySelection userCompany)
        {
            try
            {
                TempData["errormessage"] = _CE.PostMessage("", false);
                if (ModelState.IsValid)
                {
                    if (userCompany.IsDefault)
                    {
                        var data = db.UserCompanies.Where(x => x.IsDefault == true && x.UsersAdminID == userCompany.UsersAdminID).ToList();

                        if (data != null && data.Count > 0)
                        {
                            TempData["errormessage"] = _CE.PostMessage("This user has default company. Please remove his default company before adding new company.", true);
                            return RedirectToAction("Create");
                        }
                    }

                    var data2 = db.UserCompanies.Where(x => x.CompanyID == userCompany.CompanyID && x.UsersAdminID == userCompany.UsersAdminID).ToList();

                    if (data2 != null && data2.Count > 0)
                    {
                        TempData["errormessage"] = _CE.PostMessage("This company exists in user's profile. Please select other company.", true);
                        return RedirectToAction("Create");
                    }

                    UserCompany o = new UserCompany()
                    {
                        UsersAdminID = userCompany.UsersAdminID,
                        CompanyID = userCompany.CompanyID,
                        IsDefault = userCompany.IsDefault,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now
                    };
                    //userCompany.CreatedBy = User.Identity.Name;
                    //userCompany.CreatedOn = DateTime.Now;
                    db.UserCompanies.Add(o);
                    db.SaveChanges();
                    TempData["errormessage"] = _CE.PostMessage("The company is added successfully.", false);
                    return RedirectToAction("Create");
                }

                return View(userCompany);
            }
            catch (Exception ex)
            {
                TempData["errormessage"] = _CE.PostMessage("Error: " + ex.Message, true);
                return RedirectToAction("Create");
            }
        }

        // GET: UserCompanies/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var users = UserManager.Users.ToList();
            Company oC = new Company();
            List<Company> listC = new List<Company>();
            string sName = "";

            foreach (var item in users)
            {
                if (id == item.Id)
                {
                    sName = item.DisplayName;
                }
            }

            foreach (var item in db.Companies.ToList())
            {
                oC = new Company()
                {
                    ID = item.ID,
                    Name = item.Name
                };
                listC.Add(oC);
            }

            UserCompanySelection p = new UserCompanySelection()
            {
                CompanyList = listC,
                UsersAdminID = id,
                DisplayName = sName
            };

            return View(p);
            //UserCompany userCompany = db.UserCompanies.Find(id);
            //if (userCompany == null)
            //{
            //    return HttpNotFound();
            //}
            //return View(userCompany);
        }

        // POST: UserCompanies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,UsersAdminID,CompanyID,IsDefault")] UserCompany userCompany)
        {
            if (ModelState.IsValid)
            {
                userCompany.ModifiedBy = User.Identity.Name;
                userCompany.ModifiedOn = DateTime.Now;
                db.Entry(userCompany).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(userCompany);
        }

        // GET: UserCompanies/Delete/5
        public ActionResult Delete(int? id, string userid)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                //UserCompany userCompany = db.UserCompanies.Find(id);
                //if (userCompany == null)
                //{
                //    return HttpNotFound();
                //}
                //return View(userCompany);
                UserCompany userCompany = db.UserCompanies.Find(id);
                db.UserCompanies.Remove(userCompany);
                db.SaveChanges();
                TempData["errormessage"] = _CE.PostMessage("The company mapping is deleted successfully.", false);
                return RedirectToAction("Create/" + userid);
            }
            catch (Exception ex)
            {
                TempData["errormessage"] = _CE.PostMessage("Error: " + ex.Message, true);
                return RedirectToAction("Create");
            }
        }

        // POST: UserCompanies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserCompany userCompany = db.UserCompanies.Find(id);
            db.UserCompanies.Remove(userCompany);
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
