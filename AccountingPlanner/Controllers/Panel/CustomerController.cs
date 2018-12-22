using AccountingPlanner.Models.Customer;
using AccountingPlanner.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Claims;

namespace AccountingPlanner.Controllers.Panel
{
    [Authorize]
    public class CustomerController : Controller
    {
        #region Controller Properties
        private IConfiguration _configuration;
        private IHostingEnvironment _env;
        private CommonHelper _objHelper = new CommonHelper();
        private MySQLGateway _objDataHelper;
        #endregion

        public CustomerController(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            this._objDataHelper = new MySQLGateway(this._configuration.GetConnectionString("Connection"));
        }

        #region Get Customer Page
        public IActionResult Index()
        {
            ViewData["CustomerList"] = null;

            DataTable _dtResp = GetCustomerList();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                ViewData["CustomerList"] = _dtResp;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            ViewData["CountryList"] = GetCountryList();
            return View("~/Views/Panel/Customer/Index.cshtml");
        }
        #endregion

        #region Insert Customer
        [HttpPost]
        public IActionResult Index(CustomerModel customerModel)
        {
            if (!ModelState.IsValid)
            {
                ViewData["CountryList"] = GetCountryList();
                return View("~/Views/Panel/Customer/Index.cshtml");
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_id_organization_master", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("i_name", customerModel.name));
            parameters.Add(new KeyValuePair<string, string>("i_address", customerModel.address));
            parameters.Add(new KeyValuePair<string, string>("i_country", customerModel.country.ToString()));
            parameters.Add(new KeyValuePair<string, string>("i_state", customerModel.state));
            parameters.Add(new KeyValuePair<string, string>("i_city", customerModel.city));
            parameters.Add(new KeyValuePair<string, string>("i_pin", customerModel.pin));
            parameters.Add(new KeyValuePair<string, string>("i_phone", customerModel.phone));
            parameters.Add(new KeyValuePair<string, string>("i_fax", customerModel.fax));
            parameters.Add(new KeyValuePair<string, string>("i_mobile", customerModel.mobile));
            parameters.Add(new KeyValuePair<string, string>("i_email", customerModel.email));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("insert_customer", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                if (_dtResp.Rows[0]["response"].ToString() == "0")
                {
                    ViewData["ErrorMessage"] = _dtResp.Rows[0]["message"].ToString();
                }
                else
                {
                    ViewData["SuccessMessage"] = "Customer registered successfuly.";
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Customer service unavailable";
            }

            ViewData["CustomerList"] = null;

            DataTable _dtResp2 = GetCustomerList();

            if (this._objHelper.checkDBNullResponse(_dtResp2))
            {
                ViewData["CustomerList"] = _dtResp2;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            ViewData["CountryList"] = GetCountryList();
            return View("~/Views/Panel/Customer/Index.cshtml");
        }
        #endregion

        #region Delete Customer
        public ActionResult Delete(int id)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_customer_id", id.ToString()));
            TempData["DeleteMessage"] = "Customer Deleted Successfuly.";

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("delete_customer", parameters);

            return RedirectToAction("Index");
        }
        #endregion

        #region Edit Product
        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewData["CustomerList"] = null;

            DataTable _dtResp = GetCustomerList();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                ViewData["CustomerList"] = _dtResp;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "customer_by_id"));
            parameters.Add(new KeyValuePair<string, string>("param1", id.ToString()));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp1 = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);

            CustomerModel customer = new CustomerModel();
            customer.address = Convert.ToString(_dtResp1.Rows[0]["address"]);
            customer.city = Convert.ToString(_dtResp1.Rows[0]["city"]);
            customer.country = Convert.ToInt32(_dtResp1.Rows[0]["country"]);
            customer.email = Convert.ToString(_dtResp1.Rows[0]["email"]);
            customer.fax = Convert.ToString(_dtResp1.Rows[0]["fax"]);
            customer.mobile = Convert.ToString(_dtResp1.Rows[0]["mobile"]);
            customer.name = Convert.ToString(_dtResp1.Rows[0]["name"]);
            customer.phone = Convert.ToString(_dtResp1.Rows[0]["phone"]);
            customer.state = Convert.ToString(_dtResp1.Rows[0]["state"]);
            customer.pin = Convert.ToString(_dtResp1.Rows[0]["pin"]);

            ViewData["CountryList"] = GetCountryList();
            return View("~/Views/Panel/Customer/Edit.cshtml", customer);
        }
        #endregion

        #region Update Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, CustomerModel product)
        {
            if (!ModelState.IsValid)
            {
                ViewData["CountryList"] = GetCountryList();
                return View("~/Views/Panel/Customer/Edit.cshtml");
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("customer_id", id.ToString()));
            parameters.Add(new KeyValuePair<string, string>("i_name", product.name));
            parameters.Add(new KeyValuePair<string, string>("i_address", product.address));
            parameters.Add(new KeyValuePair<string, string>("i_country", product.country.ToString()));
            parameters.Add(new KeyValuePair<string, string>("i_state", product.state));
            parameters.Add(new KeyValuePair<string, string>("i_city", product.city));
            parameters.Add(new KeyValuePair<string, string>("i_pin", product.pin));
            parameters.Add(new KeyValuePair<string, string>("i_phone", product.phone));
            parameters.Add(new KeyValuePair<string, string>("i_fax", product.fax));
            parameters.Add(new KeyValuePair<string, string>("i_mobile", product.mobile));
            parameters.Add(new KeyValuePair<string, string>("i_email",  product.email));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("update_customer", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                if (_dtResp.Rows[0]["response"].ToString() == "0")
                {
                    ViewData["ErrorMessage"] = _dtResp.Rows[0]["message"].ToString();
                }
                else
                {
                    ViewData["SuccessMessage"] = "Customer updated successfuly.";
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Customer service unavailable";
            }

            ViewData["CustomerList"] = null;

            DataTable _dtResp2 = GetCustomerList();

            if (this._objHelper.checkDBNullResponse(_dtResp2))
            {
                ViewData["CustomerList"] = _dtResp2;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            ViewData["CountryList"] = GetCountryList();
            ViewData["CustomerList"] = GetCustomerList();
            return View("~/Views/Panel/Customer/Edit.cshtml");
        }
        #endregion

        private DataTable GetCustomerList()
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "customer"));
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