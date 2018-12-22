using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountingPlanner.Models.Expense;
using AccountingPlanner.System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AccountingPlanner.Controllers.Panel.Expense
{
    public class BillsController : Controller
    {
        #region Controller Properties
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MySQLGateway _objDataHelper;
        #endregion

        public BillsController(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._objDataHelper = new MySQLGateway(this._configuration.GetConnectionString("Connection"));
        }

        #region Render Page
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["EstimateList"] = null;

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "bill"));
            parameters.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                ViewData["EstimateList"] = _dtResp;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            return View("~/Views/Panel/Bills/Index.cshtml");
        }
        #endregion

        #region Get Add Page
        [HttpGet]
        public IActionResult Create()
        {
            // GET MASTERS
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "vendor"));
            parameters.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);

            ViewBag.CustomerList = _objHelper.checkDBNullResponse(_dtResp) ? _dtResp : null;

            return View("~/Views/Panel/Bills/Create.cshtml");
        }
        #endregion

        #region Insert Bill
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BillsModel billModel)
        {
            if (!ModelState.IsValid)
            {
                // GET MASTERS
                List<KeyValuePair<string, string>> parameters1 = new List<KeyValuePair<string, string>>();

                parameters1.Add(new KeyValuePair<string, string>("selectBy", "vendor"));
                parameters1.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
                parameters1.Add(new KeyValuePair<string, string>("param2", ""));

                DataTable _dtResp1 = _objDataHelper.ExecuteProcedure("entity_master_select", parameters1);

                ViewBag.CustomerList = _objHelper.checkDBNullResponse(_dtResp1) ? _dtResp1 : null;

                return View("~/Views/Panel/Bills/Create.cshtml");
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_id_organization_master", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("i_id_currency_master", billModel.currency));
            parameters.Add(new KeyValuePair<string, string>("i_id_vendor_master", billModel.vendor));
            parameters.Add(new KeyValuePair<string, string>("i_date", billModel.date));
            parameters.Add(new KeyValuePair<string, string>("i_expires", billModel.expires));
            parameters.Add(new KeyValuePair<string, string>("i_footer", billModel.footer));
            parameters.Add(new KeyValuePair<string, string>("i_header", billModel.header));
            parameters.Add(new KeyValuePair<string, string>("i_memo", billModel.memo));
            parameters.Add(new KeyValuePair<string, string>("i_po_so", billModel.po_so));
            parameters.Add(new KeyValuePair<string, string>("i_subhead", billModel.subhead));
            parameters.Add(new KeyValuePair<string, string>("i_version", billModel.version));
            parameters.Add(new KeyValuePair<string, string>("i_created_by", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_user")));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("insert_bill_master", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                // ENTER DETAILS
                parameters = new List<KeyValuePair<string, string>>();

                parameters.Add(new KeyValuePair<string, string>("i_id_bill_master", _dtResp.Rows[0]["bill_id"].ToString()));
                for (int i = 0; i < billModel.billsDetailsList.Count; i++)
                {
                    parameters.Add(new KeyValuePair<string, string>("i_id_tax_master", billModel.billsDetailsList[i].tax));
                    parameters.Add(new KeyValuePair<string, string>("i_id_product_master", billModel.billsDetailsList[i].product));
                    parameters.Add(new KeyValuePair<string, string>("i_description", billModel.billsDetailsList[i].description));
                    parameters.Add(new KeyValuePair<string, string>("i_quantity", billModel.billsDetailsList[i].quantity));
                    parameters.Add(new KeyValuePair<string, string>("i_price", billModel.billsDetailsList[i].price));
                    parameters.Add(new KeyValuePair<string, string>("i_amount", billModel.billsDetailsList[i].amount));

                    DataTable _dtRespDetail = _objDataHelper.ExecuteProcedure("insert_bill_detail", parameters);

                    // Remove for next iteration
                    parameters.Remove(new KeyValuePair<string, string>("i_id_tax_master", billModel.billsDetailsList[i].tax));
                    parameters.Remove(new KeyValuePair<string, string>("i_id_product_master", billModel.billsDetailsList[i].product));
                    parameters.Remove(new KeyValuePair<string, string>("i_description", billModel.billsDetailsList[i].description));
                    parameters.Remove(new KeyValuePair<string, string>("i_quantity", billModel.billsDetailsList[i].quantity));
                    parameters.Remove(new KeyValuePair<string, string>("i_price", billModel.billsDetailsList[i].price));
                    parameters.Remove(new KeyValuePair<string, string>("i_amount", billModel.billsDetailsList[i].amount));
                }


            }
            else
            {
                TempData["ErrorMessage"] = "Bills service unavailable";
            }

            TempData["SuccessMessage"] = "Bill Created Successfuly.";
            return RedirectToAction("Index");
        }
        #endregion
    }
}