using EMY.HostManager.Bussines;
using EMY.HostManager.Entities;
using EMY.HostManager.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EMY.HostManager.Web.Controllers
{
    public class AccountController : Controller
    {
        HostManagerFactory factory;
        public AccountController()
        {
            this.factory = new HostManagerFactory();
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Error = false;
            ViewBag.ErrorMessage = "";
            LoginUserViewModel login = new LoginUserViewModel();
            return PartialView(login);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginUserViewModel login)
        {
            CheckAdminUser();
            var res = await factory.Users.CheckLoginUser(login.UserName, login.Password);
            if (res.IsSuccess && res.resultType == 1)
            {
                var loggedinUser = (User)res.Data;

                List<Claim> mainClaim = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, loggedinUser.GetName),
                    new Claim(ClaimTypes.Name, loggedinUser.UserID.ToString())
                };

                var authList = await factory.Users.GetAllRoles(loggedinUser.UserID);

                foreach (var role in authList)
                {
                    var rl = new Claim(ClaimTypes.Role, role);
                    mainClaim.Add(rl);
                }

                ClaimsIdentity MainIdentity = new ClaimsIdentity(mainClaim, "EmyIdentity");

                ClaimsPrincipal userPrincipal = new ClaimsPrincipal(MainIdentity);

                await HttpContext.SignInAsync(
                    SystemStatics.DefaultScheme,
                    userPrincipal,
                    new AuthenticationProperties { IsPersistent = login.RememberMe }
                    );
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = res.Message;
                ViewBag.Error = true;
                return PartialView(login);
            }


        }

        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme)]
        public async Task<IActionResult> LogOut()
        {
            if (HttpContext.Request.Cookies.Count > 0)
            {
                var siteCookies = HttpContext.Request.Cookies.Where(c => c.Key.Contains(".AspNetCore.") || c.Key.Contains("Microsoft.Authentication"));
                foreach (var cookie in siteCookies)
                {
                    Response.Cookies.Delete(cookie.Key);
                }
            }
            await HttpContext.SignOutAsync(scheme: SystemStatics.DefaultScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        async void CheckAdminUser()
        {
            string UserName = "EnesMY";
            string Password = "123456";
            string Role = "Admin";
            var res = await factory.Users.CheckLoginUser(UserName, Password);
            if (res.resultType == -1)
            {
                var user = new User
                {

                    Name = "Enes Murat",
                    LastName = "YILDIRIM",
                    UserName = UserName,
                    Password = Password,
                    IsActive = true,
                    IsDeleted = false
                };


                await factory.Users.Add(user, 0);

                UserRole newRole = new UserRole()
                {
                    UserID = user.UserID,
                    FormName = Role,
                    AuthorizeType = AuthType.Full
                };
                await factory.Users.AddRole(newRole, user.UserID);
            }

        }

        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme)]
        public async Task<IActionResult> MyProfile()
        {
            var user = await factory.Users.GetByUserID(int.Parse(User.Identity.Name));
            if (user == null) Redirect("/Account/Login");
            return View(user);
        }
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme), HttpPost]
        public async Task<IActionResult> ChangeMyPassword(string oldPassword, string newPassword)
        {
            var me = await factory.Users.GetByUserID(int.Parse(User.Identity.Name));
            if (me.PasswordControl(oldPassword))
            {
                await factory.Users.ChangePassword(int.Parse(User.Identity.Name), newPassword);
                return Ok("Password changed!");
            }
            else
            {
                return ValidationProblem("Old Password is not match!");
            }
        }
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "AdminFull")]
        public async Task<IActionResult> UserList()
        {
            var userlist = await factory.Users.GetAll();
            return View(userlist);

        }
        [HttpGet]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "AdminFull")]
        public IActionResult Register()
        {
            ViewBag.Error = false;
            ViewBag.ErrorMessage = "";
            User usr = new User();
            return View(usr);
        }
        [HttpGet]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "AdminFull")]
        public async Task<IActionResult> RoleManager(int userID)
        {
            var userroles = await factory.Users.GetAllRoles(userID);
            ViewBag.UserID = userID;
            var user = await factory.Users.GetByUserID(userID);
            ViewBag.User = user.GetName;
            return View(userroles.ToList());
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "AdminFull")]
        public async Task<IActionResult> Register(User user)
        {
            ViewBag.Error = true;
            User usr = await factory.Users.GetUserByUserName(user.UserName);
            if (usr != null)
            {
                ViewBag.ErrorMessage = "This user name already registered!";
                return View(usr);
            }

            user.Password = "123456";
            user.IsActive = true;
            user.IsDeleted = false;
            await factory.Users.Add(user, int.Parse(User.Identity.Name));

            return Redirect("UserList");
        }

        [HttpGet, Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "AdminFull")]
        public async Task<IActionResult> Activate(int userID)
        {
            await factory.Users.Activate(userID, int.Parse(User.Identity.Name));
            return Redirect("UserList");

        }

        [HttpGet, Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "AdminFull")]
        public async Task<IActionResult> DeActivate(int userID)
        {
            await factory.Users.DeActivate(userID, int.Parse(User.Identity.Name));
            return Redirect("UserList");

        }

        [HttpGet, Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "AdminFull")]
        public async Task<IActionResult> ResetPassword(int userID)
        {
            await factory.Users.ChangePassword(userID, "123456");
            return Redirect("UserList");
        }
        [HttpPost]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "AdminFull")]
        public async Task<IActionResult> AddRole(int userID, string formName, AuthType rollType)
        {

            bool existRole = await factory.Users.CheckRoleIsExist(userID, formName, rollType);
            if (existRole) return ValidationProblem("Role already exist in system!");
            var authrole = new UserRole()
            {
                FormName = formName,
                AuthorizeType = rollType,
                UserID = userID
            };
            await factory.Users.AddRole(authrole, userID);
            return Ok("Role added!");
        }
        [HttpPost]
        [Authorize(AuthenticationSchemes = SystemStatics.DefaultScheme, Roles = "AdminFull")]
        public async Task<IActionResult> DeleteRole(int userID, string formName, AuthType rollType)
        {
            await factory.Users.RemoveRole(userID, formName, rollType, int.Parse(User.Identity.Name));
            return Ok("Role Deleted!");
        }

    }
}
