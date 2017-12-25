using System;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace iTrade.Models
{
    // Context
    public partial class ApplicationDbContext : CLK.AspNet.Identity.EntityFramework.IdentityDbContext<ApplicationUser, ApplicationRole, ApplicationPermission>
    {
        // Constructors
        public ApplicationDbContext() : base("StarConnection") { }

        public ApplicationDbContext(string nameOrConnectionString) : base(nameOrConnectionString) { }
    }


    // Manager
    public partial class ApplicationUserManager : CLK.AspNet.Identity.EntityFramework.UserManager<ApplicationUser, ApplicationRole>
    {
        // Constuctors
        public ApplicationUserManager(ApplicationDbContext context) : base(context) { }
    }

    public partial class ApplicationRoleManager : CLK.AspNet.Identity.EntityFramework.RoleManager<ApplicationRole>
    {
        // Constuctors
        public ApplicationRoleManager(ApplicationDbContext context) : base(context) { }
    }

    public partial class ApplicationPermissionManager : CLK.AspNet.Identity.EntityFramework.PermissionManager<ApplicationRole, ApplicationPermission>
    {
        // Constuctors
        public ApplicationPermissionManager(ApplicationDbContext context) : base(context) { }
    }

    public partial class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        // Constuctors
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) : base(userManager, authenticationManager) { }
    }


    // Identity
    public partial class ApplicationUser : CLK.AspNet.Identity.EntityFramework.IdentityUser
    {
        // Constructors
        public ApplicationUser() : base() { 

        }

        public ApplicationUser(string name) : base(name) { 
 
        }
    }

    public partial class ApplicationRole : CLK.AspNet.Identity.EntityFramework.IdentityRole
    {
        // Constructors
        public ApplicationRole() : base() { }

        public ApplicationRole(string name) : base(name) { }
    }

    public partial class ApplicationPermission : CLK.AspNet.Identity.EntityFramework.IdentityPermission
    {
        // Constructors
        public ApplicationPermission() : base() { }

        public ApplicationPermission(string name) : base(name) { }
    }


    // Authorize
    public partial class ApplicationPermissionAuthorize : CLK.AspNet.Identity.EntityFramework.PermissionAuthorize<ApplicationRole, ApplicationPermission>
    {
        // Constructors
        public ApplicationPermissionAuthorize(ApplicationPermissionManager permissionManager) : base(permissionManager) { }
    }
}
