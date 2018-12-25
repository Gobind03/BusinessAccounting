using AccountingPlanner.Models.Auth;
using AccountingPlanner.System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AccountingPlanner.Controllers.Generic
{
    public class AuthController : Controller
    {

        #region Controller Properties
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MySQLGateway _objDataHelper;
        #endregion

        public AuthController(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._objDataHelper = new MySQLGateway(this._configuration.GetConnectionString("Connection"));
        }

        #region Get Login Page
        public IActionResult Login([FromQuery] string returnUrl)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {

                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }
        #endregion

        #region Login Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel loginModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("user_name", loginModel.username));
            parameters.Add(new KeyValuePair<string, string>("pass_word", loginModel.password));
            parameters.Add(new KeyValuePair<string, string>("ip_addr", Request.HttpContext.Connection.RemoteIpAddress.ToString()));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("login_user", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                if (_dtResp.Rows[0]["response"].ToString() == "0")
                {
                    ViewData["ErrorMessage"] = _dtResp.Rows[0]["message"].ToString();
                }
                else
                {
                    var claims = new List<Claim>
                    {
                        new Claim("name", _dtResp.Rows[0]["name"].ToString()),
                        new Claim("id_user", _dtResp.Rows[0]["id_user_master"].ToString()),
                        new Claim("id_organization", _dtResp.Rows[0]["id_organization_master"].ToString()),
                        new Claim("organization_name", _dtResp.Rows[0]["organization_name"].ToString()),
                        new Claim("profile_img", _dtResp.Rows[0]["profile_img"].ToString()),
                        new Claim("pid", _dtResp.Rows[0]["pid"].ToString()),
                    };

                    var userIdentity = new ClaimsIdentity(claims, "Cookie");

                    ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
                    await HttpContext.SignInAsync(
                        scheme: CookieAuthenticationDefaults.AuthenticationScheme,
                        principal: principal
                    );

                    if (string.IsNullOrEmpty(loginModel.ReturnUrl))
                    {
                        if (loginModel.username == "admin")
                        {
                            return RedirectToAction("Index", "AdminUsers");
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        return Redirect(loginModel.ReturnUrl);
                    }
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Login service unavailable";
            }

            return View();
        }
        #endregion

        #region Logout Method
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(scheme: CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Auth");
        }
        #endregion

        #region Get Register Page
        public IActionResult Register()
        {
            return View();
        }
        #endregion

        #region Register Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegistrationModel registrationModel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("full_name", registrationModel.name));
            parameters.Add(new KeyValuePair<string, string>("user_password", registrationModel.password));
            parameters.Add(new KeyValuePair<string, string>("email_address", registrationModel.email));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("user_registration", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                if (_dtResp.Rows[0]["response"].ToString() == "0")
                {
                    ViewData["ErrorMessage"] = _dtResp.Rows[0]["message"].ToString();
                }
                else
                {
                    ViewData["SuccessMessage"] = _dtResp.Rows[0]["message"].ToString();
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Registration service unavailable";
            }

            return View();
        }
        #endregion
    }
}