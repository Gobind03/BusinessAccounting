using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountingPlanner.Models.Business;
using AccountingPlanner.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AccountingPlanner.Controllers.Panel
{
    [Authorize]
    public class BusinessController : Controller
    {
        #region Controller Properties
        private IConfiguration _configuration;
        private IHostingEnvironment _env;
        private CommonHelper _objHelper = new CommonHelper();
        private MySQLGateway _objDataHelper;
        #endregion

        public BusinessController(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            this._objDataHelper = new MySQLGateway(this._configuration.GetConnectionString("Connection"));
        }

        public IActionResult Index()
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "business"));
            parameters.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_user")));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);
            ViewData["BusinessList"] = null;

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                ViewData["BusinessList"] = _dtResp;
            }
            else
            {
                ViewData["ErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            ViewData["CountryList"] = GetCountryList();

            return View("~/Views/Panel/Business/Index.cshtml");
        }
        

        public ActionResult Create()
        {
            ViewData["CountryList"] = GetCountryList();
            return View("~/Views/Panel/Business/Create.cshtml");
        }

        #region Add Business
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(BusinessModel businessModel)
        {
            ViewData["CountryList"] = GetCountryList();

            if (!ModelState.IsValid)
            {
                return View("~/Views/Panel/Business/Create.cshtml");
            }

            if (!(businessModel.logo.ContentType == "image/png" || businessModel.logo.ContentType == "image/jpg" || businessModel.logo.ContentType == "image/jpeg"))
            {
                ViewData["ErrorMessage"] = "Unsupported image type. Kindly re-upload a JPG/JPEG/PNG only.";
                return RedirectToAction("Index", "Profile");
            }

            string dbPath = "/uploads/business_logo";
            string path = _env.WebRootPath + dbPath;
            string localFileName = Guid.NewGuid().ToString() + "." + businessModel.logo.ContentType.Split("/")[businessModel.logo.ContentType.Split("/").Length - 1];

            byte[] imageStream;
            using (var memoryStream = new MemoryStream())
            {
                await businessModel.logo.CopyToAsync(memoryStream);
                imageStream = memoryStream.ToArray();
            }

            using (var fs = new FileStream($"{path}/{localFileName}", FileMode.Create, FileAccess.Write))
            {
                await fs.WriteAsync(imageStream, 0, imageStream.Length);
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_organization_name", businessModel.name.ToUpper()));
            parameters.Add(new KeyValuePair<string, string>("i_logo", $"{dbPath}/{localFileName}"));
            parameters.Add(new KeyValuePair<string, string>("i_type_of_business", businessModel.type_of_business));
            parameters.Add(new KeyValuePair<string, string>("i_address_line_1", businessModel.address_line_1));
            parameters.Add(new KeyValuePair<string, string>("i_address_line_2", businessModel.address_line_2));
            parameters.Add(new KeyValuePair<string, string>("i_city", businessModel.city));
            parameters.Add(new KeyValuePair<string, string>("i_country", businessModel.country));
            parameters.Add(new KeyValuePair<string, string>("i_state", businessModel.state));
            parameters.Add(new KeyValuePair<string, string>("i_pin", businessModel.pin));
            parameters.Add(new KeyValuePair<string, string>("i_phone", businessModel.phone));
            parameters.Add(new KeyValuePair<string, string>("i_fax", businessModel.fax));
            parameters.Add(new KeyValuePair<string, string>("i_mobile", businessModel.mobile));
            parameters.Add(new KeyValuePair<string, string>("i_toll_free", businessModel.toll_free));
            parameters.Add(new KeyValuePair<string, string>("i_website", businessModel.website));
            parameters.Add(new KeyValuePair<string, string>("i_currency", businessModel.currency));
            parameters.Add(new KeyValuePair<string, string>("i_type_of_organization", businessModel.type_of_organization));
            parameters.Add(new KeyValuePair<string, string>("userID", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_user")));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("insert_business", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                if(_dtResp.Rows[0]["response"].ToString() == "0")
                {
                    ViewData["ErrorMessage"] = _dtResp.Rows[0]["message"].ToString();
                }
                else
                {
                    TempData["SuccessMessage"] = "Business registered successfuly.";
                    return RedirectToAction("Index", "Business");
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Business service unavailable";
            }

            return View("~/Views/Panel/Business/Create.cshtml");
        }
        #endregion

        // GET: Business/Edit/5
        public ActionResult Edit(int id)
        {
            return View("~/Views/Panel/Business/Create.cshtml");
        }

        // POST: Business/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View("~/Views/Panel/Business/Create.cshtml");
            }
        }

        #region Delete Business
        public ActionResult Delete(int id)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("I_id_organization_master", id.ToString()));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("delete_business", parameters);

            TempData["DeleteMessage"] = "Business Deleted Successfuly!";

            return RedirectToAction("Index");
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