using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountingPlanner.Models.Tax;
using AccountingPlanner.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AccountingPlanner.Controllers.Panel
{
    [Authorize]
    public class TaxController : Controller
    {
        #region Controller Properties
        private IConfiguration _configuration;
        private IHostingEnvironment _env;
        private CommonHelper _objHelper = new CommonHelper();
        private MySQLGateway _objDataHelper;
        #endregion

        public TaxController(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            this._objDataHelper = new MySQLGateway(this._configuration.GetConnectionString("Connection"));
        }

        #region Get Tax View
        public ActionResult Index()
        {
            ViewData["TaxList"] = null;

            DataTable _dtResp = GetTaxList();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                ViewData["TaxList"] = _dtResp;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            return View("~/Views/Panel/Tax/Index.cshtml");
        }
        #endregion

        #region Add New Tax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(TaxModel taxModel)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Panel/Tax/Index.cshtml");
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_id_organization", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("i_tax_header", taxModel.header));
            parameters.Add(new KeyValuePair<string, string>("i_type", taxModel.type));
            parameters.Add(new KeyValuePair<string, string>("i_value", taxModel.value.ToString()));
            parameters.Add(new KeyValuePair<string, string>("i_applicability", taxModel.applicability));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("insert_tax", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                if (_dtResp.Rows[0]["response"].ToString() == "0")
                {
                    ViewData["ErrorMessage"] = _dtResp.Rows[0]["message"].ToString();
                }
                else
                {
                    ViewData["SuccessMessage"] = "Tax registered successfuly.";
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Business service unavailable";
            }

            ViewData["TaxList"] = null;

            DataTable _dtResp2 = GetTaxList();

            if (this._objHelper.checkDBNullResponse(_dtResp2))
            {
                ViewData["TaxList"] = _dtResp2;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            return View("~/Views/Panel/Tax/Index.cshtml");

        }
        #endregion

        #region Delete Tax
        public IActionResult Delete (int id)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_tax_master", id.ToString()));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("delete_tax", parameters);
            TempData["DeleteMessage"] = "Tax Deleted Successfuly.";

            return RedirectToAction("Index");
        }
        #endregion

        #region Edit Tax
        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewData["TaxList"] = null;

            DataTable _dtResp = GetTaxList();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                ViewData["TaxList"] = _dtResp;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "tax_by_id"));
            parameters.Add(new KeyValuePair<string, string>("param1", id.ToString()));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp1 = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);

            TaxModel tax = new TaxModel();
            tax.applicability = Convert.ToString(_dtResp1.Rows[0]["applicability"]);
            tax.header = Convert.ToString(_dtResp1.Rows[0]["label"]);
            tax.type = Convert.ToString(_dtResp1.Rows[0]["type"]);
            tax.value = Convert.ToInt32(_dtResp1.Rows[0]["value"]);

            return View("~/Views/Panel/Tax/Edit.cshtml", tax);
        }
        #endregion

        #region Update Tax
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, TaxModel taxModel)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Panel/Tax/Edit.cshtml");
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("tax_id", id.ToString()));
            parameters.Add(new KeyValuePair<string, string>("i_tax_header", taxModel.header));
            parameters.Add(new KeyValuePair<string, string>("i_type", taxModel.type));
            parameters.Add(new KeyValuePair<string, string>("i_value", taxModel.value.ToString()));
            parameters.Add(new KeyValuePair<string, string>("i_applicability", taxModel.applicability));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("update_tax", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                if (_dtResp.Rows[0]["response"].ToString() == "0")
                {
                    ViewData["ErrorMessage"] = _dtResp.Rows[0]["message"].ToString();
                }
                else
                {
                    ViewData["SuccessMessage"] = "Tax updated successfuly.";
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Tax service unavailable";
            }

            ViewData["VendorList"] = null;

            DataTable _dtResp2 = GetTaxList();

            if (this._objHelper.checkDBNullResponse(_dtResp2))
            {
                ViewData["TaxList"] = _dtResp2;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            ViewData["TaxList"] = GetTaxList();
            return View("~/Views/Panel/Tax/Edit.cshtml");
        }
        #endregion

        private DataTable GetTaxList()
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "tax"));
            parameters.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);

            return _dtResp;
        }
    }
}