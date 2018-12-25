using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountingPlanner.Models.Invoice;
using AccountingPlanner.System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AccountingPlanner.Controllers.Panel.Ìncome
{
    public class InvoiceController : Controller
    {
        #region Controller Properties
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MySQLGateway _objDataHelper;
        #endregion

        public InvoiceController(IConfiguration configuration)
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

            parameters.Add(new KeyValuePair<string, string>("selectBy", "invoice"));
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

            return View("~/Views/Panel/Invoice/Index.cshtml");
        }
        #endregion

        #region Get Add Page
        [HttpGet]
        public IActionResult Create()
        {
            // GET MASTERS
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "customer"));
            parameters.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);

            ViewBag.CustomerList = _objHelper.checkDBNullResponse(_dtResp) ? _dtResp : null;

            return View("~/Views/Panel/Invoice/Create.cshtml");
        }
        #endregion

        #region Insert Estimate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(InvoiceModel invoiceModel)
        {
            if (!ModelState.IsValid)
            {
                // GET MASTERS
                List<KeyValuePair<string, string>> parameters1 = new List<KeyValuePair<string, string>>();

                parameters1.Add(new KeyValuePair<string, string>("selectBy", "customer"));
                parameters1.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
                parameters1.Add(new KeyValuePair<string, string>("param2", ""));

                DataTable _dtResp1 = _objDataHelper.ExecuteProcedure("entity_master_select", parameters1);

                ViewBag.CustomerList = _objHelper.checkDBNullResponse(_dtResp1) ? _dtResp1 : null;

                return View("~/Views/Panel/Invoice/Create.cshtml");
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_id_organization_master", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("i_id_currency_master", invoiceModel.currency));
            parameters.Add(new KeyValuePair<string, string>("i_id_customer_master", invoiceModel.cutomer));
            parameters.Add(new KeyValuePair<string, string>("i_invoice_number", invoiceModel.invoice_number));
            parameters.Add(new KeyValuePair<string, string>("i_due_date", invoiceModel.due_date));
            parameters.Add(new KeyValuePair<string, string>("i_invoice_date", invoiceModel.invoice_date));
            parameters.Add(new KeyValuePair<string, string>("i_order_number", invoiceModel.order_number));
            parameters.Add(new KeyValuePair<string, string>("i_created_by", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_user")));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("insert_invoice_master", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                // ENTER DETAILS
                parameters = new List<KeyValuePair<string, string>>();

                parameters.Add(new KeyValuePair<string, string>("i_id_invoice_master", _dtResp.Rows[0]["invoice_id"].ToString()));
                for (int i = 0; i < invoiceModel.invoiceDetailList.Count; i++)
                {
                    parameters.Add(new KeyValuePair<string, string>("i_id_tax_master", invoiceModel.invoiceDetailList[i].tax));
                    parameters.Add(new KeyValuePair<string, string>("i_id_product_master", invoiceModel.invoiceDetailList[i].product));
                    parameters.Add(new KeyValuePair<string, string>("i_description", invoiceModel.invoiceDetailList[i].description));
                    parameters.Add(new KeyValuePair<string, string>("i_quantity", invoiceModel.invoiceDetailList[i].quantity));
                    parameters.Add(new KeyValuePair<string, string>("i_price", invoiceModel.invoiceDetailList[i].price));
                    parameters.Add(new KeyValuePair<string, string>("i_amount", invoiceModel.invoiceDetailList[i].amount));

                    DataTable _dtRespDetail = _objDataHelper.ExecuteProcedure("insert_invoice_detail", parameters);

                    // Remove for next iteration
                    parameters.Remove(new KeyValuePair<string, string>("i_id_tax_master", invoiceModel.invoiceDetailList[i].tax));
                    parameters.Remove(new KeyValuePair<string, string>("i_id_product_master", invoiceModel.invoiceDetailList[i].product));
                    parameters.Remove(new KeyValuePair<string, string>("i_description", invoiceModel.invoiceDetailList[i].description));
                    parameters.Remove(new KeyValuePair<string, string>("i_quantity", invoiceModel.invoiceDetailList[i].quantity));
                    parameters.Remove(new KeyValuePair<string, string>("i_price", invoiceModel.invoiceDetailList[i].price));
                    parameters.Remove(new KeyValuePair<string, string>("i_amount", invoiceModel.invoiceDetailList[i].amount));
                }


            }
            else
            {
                TempData["ErrorMessage"] = "Invoice service unavailable";
            }

            TempData["SuccessMessage"] = "Invoice Created Successfuly.";
            return RedirectToAction("Index");
        }
        #endregion
    }
}