using CLK.AspNet.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace iTrade.Models
{
    // Context
    public partial class ApplicationDbContext
    {
        // Constructors
        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(new ApplicationDbInitializer());
        }


        // Methods
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext("StarConnection");
        }
    }

    public class ApplicationDbInitializer : DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        // Methods
        protected override void Seed(ApplicationDbContext context)
        {
            // Initialize
            this.InitializeIdentity(context);

            // Base
            base.Seed(context);
        }

        private void InitializeIdentity(ApplicationDbContext context)
        {
            #region Contracts

            if (context == null) throw new ArgumentNullException();

            #endregion

            // Default - User
            const string adminUserName = "admin@starlight.sg";
            const string adminUserPassword = "123456";
            const string adminUserDisplayName = "Admin";
            const string branchID = "1";

            const string guestUserName = "demo@starlight.sg";
            const string guestUserPassword = "123456";
            const string guestUserDisplayName = "Demo";

            const string salesagentUserName = "sales@starlight.sg";
            const string salesagentUserPassword = "123456";
            const string salesagentUserDisplayName = "Sales Agent";

            const string logisticwarehouseUserName = "logistic@starlight.sg";
            const string logisticwarehouseUserPassword = "123456";
            const string logisticwarehouseUserDisplayName = "Logistic / Warehouse";

            const string mgtUserName = "mgt@starlight.sg";
            const string mgtUserPassword = "123456";
            const string mgtUserDisplayName = "Management";

            // Default - Role
            const string adminRoleName = "Admin";
            const string guestRoleName = "Guest";
            const string salesagentRoleName = "Sales Agent";
            const string logisticwarehouseRoleName = "Logistic / Warehouse";
            const string ManagementRoleName = "Management";

            // Default - Permission
            const string aboutPermissionName = "AboutAccess";
            const string contactPermissionName = "ContactAccess";


            // Manager
            var userManager = HttpContext.Current.GetOwinContext().Get<ApplicationUserManager>();
            var roleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            var permissionManager = HttpContext.Current.GetOwinContext().Get<ApplicationPermissionManager>();


            // User
            var adminUser = userManager.FindByName(adminUserName);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser { UserName = adminUserName, Email = adminUserName, DisplayName = adminUserDisplayName, BranchID = branchID };
                userManager.Create(adminUser, adminUserPassword);
                userManager.SetLockoutEnabled(adminUser.Id, false);
            }

            var guestUser = userManager.FindByName(guestUserName);
            if (guestUser == null)
            {
                guestUser = new ApplicationUser { UserName = guestUserName, Email = guestUserName, DisplayName = guestUserDisplayName, BranchID = branchID };
                userManager.Create(guestUser, guestUserPassword);
                userManager.SetLockoutEnabled(guestUser.Id, false);
            }

            var salesagentUser = userManager.FindByName(salesagentUserName);
            if (salesagentUser == null)
            {
                salesagentUser = new ApplicationUser { UserName = salesagentUserName, Email = salesagentUserName, DisplayName = salesagentUserDisplayName, BranchID = branchID };
                userManager.Create(salesagentUser, salesagentUserPassword);
                userManager.SetLockoutEnabled(salesagentUser.Id, false);
            }

            var logisticwarehouseUser = userManager.FindByName(logisticwarehouseUserName);
            if (logisticwarehouseUser == null)
            {
                logisticwarehouseUser = new ApplicationUser { UserName = logisticwarehouseUserName, Email = logisticwarehouseUserName, DisplayName = logisticwarehouseUserDisplayName, BranchID = branchID };
                userManager.Create(logisticwarehouseUser, logisticwarehouseUserPassword);
                userManager.SetLockoutEnabled(logisticwarehouseUser.Id, false);
            }

            var mgtUser = userManager.FindByName(mgtUserName);
            if (mgtUser == null)
            {
                mgtUser = new ApplicationUser { UserName = mgtUserName, Email = mgtUserName, DisplayName = mgtUserDisplayName, BranchID = branchID };
                userManager.Create(mgtUser, mgtUserPassword);
                userManager.SetLockoutEnabled(mgtUser.Id, false);
            }

            // Role
            var adminRole = roleManager.FindByName(adminRoleName);
            if (adminRole == null)
            {
                adminRole = new ApplicationRole(adminRoleName);
                roleManager.Create(adminRole);
            }

            var guestRole = roleManager.FindByName(guestRoleName);
            if (guestRole == null)
            {
                guestRole = new ApplicationRole(guestRoleName);
                roleManager.Create(guestRole);
            }

            var salesagentRole = roleManager.FindByName(salesagentRoleName);
            if (salesagentRole == null)
            {
                salesagentRole = new ApplicationRole(salesagentRoleName);
                roleManager.Create(salesagentRole);
            }

            var logisticwarehouseRole = roleManager.FindByName(logisticwarehouseRoleName);
            if (logisticwarehouseRole == null)
            {
                logisticwarehouseRole = new ApplicationRole(logisticwarehouseRoleName);
                roleManager.Create(logisticwarehouseRole);
            }

            var ManagementRole = roleManager.FindByName(ManagementRoleName);
            if (ManagementRole == null)
            {
                ManagementRole = new ApplicationRole(ManagementRoleName);
                roleManager.Create(ManagementRole);
            }

            // Permission
            var aboutPermission = permissionManager.FindByName(aboutPermissionName);
            if (aboutPermission == null)
            {
                aboutPermission = new ApplicationPermission(aboutPermissionName);
                permissionManager.Create(aboutPermission);
            }

            var contactPermission = permissionManager.FindByName(contactPermissionName);
            if (contactPermission == null)
            {
                contactPermission = new ApplicationPermission(contactPermissionName);
                permissionManager.Create(contactPermission);
            }


            // UserAddToRole 
            IList<string> rolesForUser = null;

            rolesForUser = userManager.GetRoles(adminUser.Id);
            if (rolesForUser.Contains(adminRole.Name)==false)
            {
                userManager.AddToRole(adminUser.Id, adminRole.Name);
            }

            rolesForUser = userManager.GetRoles(guestUser.Id);
            if (rolesForUser.Contains(guestRole.Name) == false)
            {
                userManager.AddToRole(guestUser.Id, guestRole.Name);
            }

            rolesForUser = userManager.GetRoles(salesagentUser.Id);
            if (rolesForUser.Contains(salesagentRole.Name) == false)
            {
                userManager.AddToRole(salesagentUser.Id, salesagentRole.Name);
            }

            rolesForUser = userManager.GetRoles(logisticwarehouseUser.Id);
            if (rolesForUser.Contains(logisticwarehouseRole.Name) == false)
            {
                userManager.AddToRole(logisticwarehouseUser.Id, logisticwarehouseRole.Name);
            }

            rolesForUser = userManager.GetRoles(logisticwarehouseUser.Id);
            if (rolesForUser.Contains(logisticwarehouseRole.Name) == false)
            {
                userManager.AddToRole(logisticwarehouseUser.Id, logisticwarehouseRole.Name);
            }

            rolesForUser = userManager.GetRoles(mgtUser.Id);
            if (rolesForUser.Contains(ManagementRole.Name) == false)
            {
                userManager.AddToRole(mgtUser.Id, ManagementRole.Name);
            }

            // PermissionAddToRole 
            IList<string> rolesForPermission = null;

            rolesForPermission = permissionManager.GetRolesById(aboutPermission.Id);
            if (rolesForPermission.Contains(adminRole.Name) == false)
            {
                permissionManager.AddToRole(aboutPermission.Id, adminRole.Name);
            }

            rolesForPermission = permissionManager.GetRolesById(contactPermission.Id);
            if (rolesForPermission.Contains(adminRole.Name) == false)
            {
                permissionManager.AddToRole(contactPermission.Id, adminRole.Name);
            }
        }
    }


    // Manager
    public partial class ApplicationUserManager
    {
        // Methods
        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            #region Contracts

            if (options == null) throw new ArgumentNullException();
            if (context == null) throw new ArgumentNullException();

            #endregion

            // 建立使用者管理員
            var userManager = new ApplicationUserManager(context.Get<ApplicationDbContext>());
            if (userManager == null) throw new InvalidOperationException();

            // 設定使用者名稱的驗證邏輯
            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // 設定密碼的驗證邏輯
            userManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 5,              // 最小長度
                RequireNonLetterOrDigit = false, // 是否需要一個非字母或是數字
                RequireDigit = false,            // 是否需要一個數字
                RequireLowercase = false,        // 是否需要一個小寫字母
                RequireUppercase = false,        // 是否需要一個大寫字母
            };

            // 設定使用者鎖定詳細資料
            userManager.UserLockoutEnabledByDefault = true;
            userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            userManager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // 註冊雙因素驗證提供者。此應用程式使用手機和電子郵件接收驗證碼以驗證使用者
            // 您可以撰寫專屬提供者，並將它外掛到這裡。
            userManager.RegisterTwoFactorProvider("電話代碼", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "您的安全碼為 {0}"
            });
            userManager.RegisterTwoFactorProvider("電子郵件代碼", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "安全碼",
                BodyFormat = "您的安全碼為 {0}"
            });
            userManager.EmailService = new EmailService();
            userManager.SmsService = new SmsService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                userManager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }

            // 回傳
            return userManager;
        }
    }

    public partial class ApplicationRoleManager
    {
        // Methods
        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            return new ApplicationRoleManager(context.Get<ApplicationDbContext>());
        }
    }

    public partial class ApplicationPermissionManager
    {
        // Methods
        public static ApplicationPermissionManager Create(IdentityFactoryOptions<ApplicationPermissionManager> options, IOwinContext context)
        {
            return new ApplicationPermissionManager(context.Get<ApplicationDbContext>());
        }
    }

    public partial class ApplicationSignInManager
    {
        // Methods
        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync(this.UserManager as ApplicationUserManager);
        }
    }


    // Identity
    public partial class ApplicationUser
    {
        public string DisplayName { get; set; }

        public string BranchID { get; set; }

        // Methods
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(ApplicationUserManager manager)
        {
            // 注意 authenticationType 必須符合 CookieAuthenticationOptions.AuthenticationType 中定義的項目
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // 在這裡新增自訂使用者宣告
            return userIdentity;
        }
    }

    public partial class ApplicationRole
    {
        // Methods

    }

    public partial class ApplicationPermission
    {
        // Methods

    }


    // Service
    public class EmailService : IIdentityMessageService
    {
        // Methods
        public Task SendAsync(IdentityMessage message)
        {
            // 將您的電子郵件服務外掛到這裡以傳送電子郵件。
            return Task.FromResult(0);
        }
    }

    public class SmsService : IIdentityMessageService
    {
        // Methods
        public Task SendAsync(IdentityMessage message)
        {
            // 將您的 SMS 服務外掛到這裡以傳送簡訊。
            return Task.FromResult(0);
        }
    }
}
