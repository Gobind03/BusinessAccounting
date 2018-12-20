using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountingPlanner.Models.Profile;
using AccountingPlanner.System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AccountingPlanner.Controllers.Panel
{
    [Authorize]
    public class ProfileController : Controller
    {
        #region Controller Properties
        private IConfiguration _configuration;
        private IHostingEnvironment _env;
        private CommonHelper _objHelper = new CommonHelper();
        private MySQLGateway _objDataHelper;
        #endregion

        public ProfileController(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            this._objDataHelper = new MySQLGateway(this._configuration.GetConnectionString("Connection"));
        }

        #region Get Profile Page
        public ActionResult Index()
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "profile"));
            parameters.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_user")));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);
            ProfileModel profileModel = new ProfileModel();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                profileModel.name = Convert.ToString(_dtResp.Rows[0]["name"]);
                profileModel.email = Convert.ToString(_dtResp.Rows[0]["email"]);
                profileModel.phone = Convert.ToString(_dtResp.Rows[0]["phone"]);
                profileModel.country = Convert.ToString(_dtResp.Rows[0]["country"]);
                profileModel.dob = Convert.ToString(_dtResp.Rows[0]["dob"]);
                profileModel.city = Convert.ToString(_dtResp.Rows[0]["city"]);
                profileModel.province = Convert.ToString(_dtResp.Rows[0]["state"]);
                profileModel.pin = Convert.ToString(_dtResp.Rows[0]["pin"]);
            }
            else
            {
                ViewData["ErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            ViewData["CountryList"] = GetCountryList();

            return View("~/Views/Panel/Profile/Index.cshtml", profileModel);
        }
        #endregion

        #region Update Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ProfileModel profileModel)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Panel/Profile/Index.cshtml");
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_name", profileModel.name));
            parameters.Add(new KeyValuePair<string, string>("i_phone", profileModel.phone));
            parameters.Add(new KeyValuePair<string, string>("i_country", profileModel.country));
            parameters.Add(new KeyValuePair<string, string>("i_province", profileModel.province));
            parameters.Add(new KeyValuePair<string, string>("i_city", profileModel.city));
            parameters.Add(new KeyValuePair<string, string>("i_pin", profileModel.pin));
            parameters.Add(new KeyValuePair<string, string>("i_dob", profileModel.dob));
            parameters.Add(new KeyValuePair<string, string>("userID", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_user")));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("update_user_profile", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                ViewData["SuccessMessage"] = _dtResp.Rows[0]["message"].ToString();
            }
            else
            {
                ViewData["ErrorMessage"] = "Profile service unavailable";
            }

            ViewData["CountryList"] = GetCountryList();

            return View("~/Views/Panel/Profile/Index.cshtml");
        }
        #endregion

        #region Update Profile Picture
        public async Task<IActionResult> UpdatePicture(AvatarModel avatar)
        {
            if(avatar.image == null)
            {
                TempData["ImageUploadError"] = "Please provide an image.";
                return RedirectToAction("Index", "Profile");
            }

            if(!(avatar.image.ContentType == "image/png" || avatar.image.ContentType == "image/jpg" || avatar.image.ContentType == "image/jpeg"))
            {
                TempData["ImageUploadError"] = "Unsupported image type. Kindly re-upload a JPG/JPEG/PNG only.";
                return RedirectToAction("Index", "Profile");
            }

            string dbPath = "/uploads/profile_picure";
            string path = _env.WebRootPath + dbPath;
            string localFileName = Guid.NewGuid().ToString() + "." + avatar.image.ContentType.Split("/")[avatar.image.ContentType.Split("/").Length - 1];

            byte[] imageStream;
            using (var memoryStream = new MemoryStream())
            {
                await avatar.image.CopyToAsync(memoryStream);
                imageStream = memoryStream.ToArray();
            }

            using (var fs = new FileStream($"{path}/{localFileName}", FileMode.Create, FileAccess.Write))
            {
                await fs.WriteAsync(imageStream, 0, imageStream.Length);
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("url", $"{dbPath}/{localFileName}"));
            parameters.Add(new KeyValuePair<string, string>("userID", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_user")));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("update_profile_picture", parameters);


            if (this._objHelper.checkDBResponse(_dtResp))
            {
                var claims = new List<Claim>
                    {
                        new Claim("name", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "name")),
                        new Claim("id_user", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_user")),
                        new Claim("id_organization", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")),
                        new Claim("organization_name", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "organization_name")),
                        new Claim("profile_img", $"{dbPath}/{localFileName}"),
                        new Claim("pid", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "pid")),
                    };

                var userIdentity = new ClaimsIdentity(claims, "Cookie");

                ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
                await HttpContext.SignInAsync(
                    scheme: CookieAuthenticationDefaults.AuthenticationScheme,
                    principal: principal
                );

                TempData["ImageUploadSuccess"] = _dtResp.Rows[0]["message"].ToString();
            }
            else
            {
                TempData["ImageUploadError"] = "Profile service unavailable";
            }

            return RedirectToAction("Index", "Profile");
        }
        #endregion

        #region PRIVATE: Get Country List
        private DataTable GetCountryList()
        {
            List<KeyValuePair<string, string>> parameters2 = new List<KeyValuePair<string, string>>();
            parameters2.Add(new KeyValuePair<string, string>("selectBy", "country"));
            parameters2.Add(new KeyValuePair<string, string>("param1", ""));
            parameters2.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters2);

            if (_objHelper.checkDBNullResponse(_dtResp))
            {
                return _dtResp;
            }
            else
            {
                return null;
            }
        }
        #endregion
    }
}