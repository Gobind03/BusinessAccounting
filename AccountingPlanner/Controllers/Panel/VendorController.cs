using AccountingPlanner.Models.Vendor;
using AccountingPlanner.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;

namespace AccountingPlanner.Controllers.Panel
{
    [Authorize]
    public class VendorController : Controller
    {
        #region Controller Properties
        private IConfiguration _configuration;
        private IHostingEnvironment _env;
        private CommonHelper _objHelper = new CommonHelper();
        private MySQLGateway _objDataHelper;
        #endregion

        public VendorController(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            this._objDataHelper = new MySQLGateway(this._configuration.GetConnectionString("Connection"));
        }

        #region Get Vendor Page
        public IActionResult Index()
        {
            ViewData["VendorList"] = null;

            DataTable _dtResp = GetVendorList();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                ViewData["VendorList"] = _dtResp;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            ViewData["CountryList"] = GetCountryList();
            return View("~/Views/Panel/Vendor/Index.cshtml");
        }
        #endregion

        #region Insert Vendor
        [HttpPost]
        public IActionResult Index(VendorModel vendorModel)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Panel/Vendor/Index.cshtml");
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_id_organization_master", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("i_name", vendorModel.vendor_name));
            parameters.Add(new KeyValuePair<string, string>("i_address", vendorModel.address));
            parameters.Add(new KeyValuePair<string, string>("i_country", vendorModel.country.ToString()));
            parameters.Add(new KeyValuePair<string, string>("i_state", vendorModel.province));
            parameters.Add(new KeyValuePair<string, string>("i_city", vendorModel.city));
            parameters.Add(new KeyValuePair<string, string>("i_pin", vendorModel.pin));
            parameters.Add(new KeyValuePair<string, string>("i_phone", vendorModel.phone));
            parameters.Add(new KeyValuePair<string, string>("i_fax", vendorModel.fax));
            parameters.Add(new KeyValuePair<string, string>("i_mobile", vendorModel.mobile));
            parameters.Add(new KeyValuePair<string, string>("i_email", vendorModel.email));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("insert_vendor", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                if (_dtResp.Rows[0]["response"].ToString() == "0")
                {
                    ViewData["ErrorMessage"] = _dtResp.Rows[0]["message"].ToString();
                }
                else
                {
                    ViewData["SuccessMessage"] = "Vendor registered successfuly.";
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Vendor service unavailable";
            }

            ViewData["VendorList"] = null;

            DataTable _dtResp2 = GetVendorList();

            if (this._objHelper.checkDBNullResponse(_dtResp2))
            {
                ViewData["VendorList"] = _dtResp2;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            ViewData["CountryList"] = GetCountryList();
            return View("~/Views/Panel/Vendor/Index.cshtml");
        }
        #endregion

        #region Delete Vendor
        public IActionResult Delete(int id)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_vendor_master", id.ToString()));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("delete_vendor", parameters);
            TempData["DeleteMessage"] = "Vendor Deleted Successfuly.";

            return RedirectToAction("Index");
        }
        #endregion

        private DataTable GetVendorList()
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "vendor"));
            parameters.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);

            return _dtResp;
        }

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