using iTrade.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace iTrade.Controllers
{
    [RBACAuthorize(Roles = "Admin")]
    public class PermissionsAdminController : Controller
    {
        // Constructors
        public PermissionsAdminController()
        {
        }

        public PermissionsAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager, ApplicationPermissionManager permissionManager)
        {
            this.UserManager = userManager;
            this.RoleManager = roleManager;
            this.PermissionManager = permissionManager;
        }


        // Properties
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


        //
        // GET: /Roles/
        public ActionResult Index()
        {
            return View(PermissionManager.Permissions);
        }

        //
        // GET: /Permissions/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var permission = await PermissionManager.FindByIdAsync(id);

            ViewBag.RoleNames = await PermissionManager.GetRolesByIdAsync(permission.Id);

            return View(permission);
        }

        //
        // GET: /Permissions/Create
        public async Task<ActionResult> Create()
        {
            //Get the list of Roles
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        //
        // POST: /Permissions/Create
        [HttpPost]
        public async Task<ActionResult> Create(PermissionViewModel permissionViewModel, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                var permission = new ApplicationPermission(permissionViewModel.Name);
                var permissionresult = await PermissionManager.CreateAsync(permission);
                //Add Permission to the selected Roles 
                if (permissionresult.Succeeded)
                {
                    if (selectedRoles != null)
                    {
                        var result = await PermissionManager.AddToRolesAsync(permission.Id, selectedRoles);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("", result.Errors.First());
                            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                            return View();
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", permissionresult.Errors.First());
                    ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                    return View();

                }
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
            return View();
        }

        //
        // GET: /Permissions/Edit/1
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var permission = await PermissionManager.FindByIdAsync(id);
            if (permission == null)
            {
                return HttpNotFound();
            }

            var permissionRoles = await PermissionManager.GetRolesByIdAsync(permission.Id);

            return View(new EditPermissionViewModel()
            {
                Id = permission.Id,
                Name = permission.Name,
                RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                {
                    Selected = permissionRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                })
            });
        }

        //
        // POST: /Permissions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Name,Id")] EditPermissionViewModel editPermission, params string[] selectedRole)
        {
            if (ModelState.IsValid)
            {
                var permission = await PermissionManager.FindByIdAsync(editPermission.Id);
                if (permission == null)
                {
                    return HttpNotFound();
                }

                permission.Name = editPermission.Name;

                var permissionRoles = await PermissionManager.GetRolesByIdAsync(permission.Id);

                selectedRole = selectedRole ?? new string[] { };

                var result = await PermissionManager.AddToRolesAsync(permission.Id, selectedRole.Except(permissionRoles).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                result = await PermissionManager.RemoveFromRolesAsync(permission.Id, permissionRoles.Except(selectedRole).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Something failed.");
            return View();
        }
        
        //
        // GET: /Permissions/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var permission = await PermissionManager.FindByIdAsync(id);
            if (permission == null)
            {
                return HttpNotFound();
            }
            return View(permission);
        }

        //
        // POST: /Permissions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id, string deleteUser)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var permission = await PermissionManager.FindByIdAsync(id);
                if (permission == null)
                {
                    return HttpNotFound();
                }
                IdentityResult result;
                if (deleteUser != null)
                {
                    result = await PermissionManager.DeleteAsync(permission);
                }
                else
                {
                    result = await PermissionManager.DeleteAsync(permission);
                }
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
