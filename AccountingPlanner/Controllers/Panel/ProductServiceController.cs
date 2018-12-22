using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountingPlanner.Models.ProductService;
using AccountingPlanner.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AccountingPlanner.Controllers.Panel
{
    [Authorize]
    public class ProductServiceController : Controller
    {
        #region Controller Properties
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MySQLGateway _objDataHelper;
        #endregion

        public ProductServiceController(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._objDataHelper = new MySQLGateway(this._configuration.GetConnectionString("Connection"));
        }

        #region Render Page
        public IActionResult Index()
        {
            ViewData["ProductList"] = null;

            DataTable _dtResp = GetProductList();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                ViewData["ProductList"] = _dtResp;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            return View("~/Views/Panel/ProductService/Index.cshtml");
        }
        #endregion

        #region Add New Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ProductModel productModel)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_id_organization", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("i_product_type", productModel.type));
            parameters.Add(new KeyValuePair<string, string>("i_title", productModel.title));
            parameters.Add(new KeyValuePair<string, string>("i_description", productModel.description));
            parameters.Add(new KeyValuePair<string, string>("i_price", productModel.price));
            parameters.Add(new KeyValuePair<string, string>("i_is_sold", productModel.is_sold));
            parameters.Add(new KeyValuePair<string, string>("i_is_purchased", productModel.is_purchased));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("insert_product", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                if (_dtResp.Rows[0]["response"].ToString() == "0")
                {
                    ViewData["ErrorMessage"] = _dtResp.Rows[0]["message"].ToString();
                }
                else
                {
                    ViewData["SuccessMessage"] = "Product/Service registered successfuly.";
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Product service unavailable";
            }

            ViewData["ProductList"] = null;

            DataTable _dtResp2 = GetProductList();

            if (this._objHelper.checkDBNullResponse(_dtResp2))
            {
                ViewData["ProductList"] = _dtResp2;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/Panel/ProductService/Index.cshtml");
            }

            return View("~/Views/Panel/ProductService/Index.cshtml");

        }
        #endregion


        #region Edit Product
        [HttpGet]
        public IActionResult Edit(int id)
        {
            ViewData["ProductList"] = null;

            DataTable _dtResp = GetProductList();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                ViewData["ProductList"] = _dtResp;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "product_by_id"));
            parameters.Add(new KeyValuePair<string, string>("param1", id.ToString()));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp1 = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);

            ProductModel product = new ProductModel();
            product.description = Convert.ToString(_dtResp1.Rows[0]["description"]);
            product.is_purchased = Convert.ToString(_dtResp1.Rows[0]["is_purchased"]);
            product.is_sold = Convert.ToString(_dtResp1.Rows[0]["is_sold"]);
            product.price = Convert.ToString(_dtResp1.Rows[0]["price"]);
            product.title = Convert.ToString(_dtResp1.Rows[0]["title"]);
            product.type = Convert.ToString(_dtResp1.Rows[0]["type"]);

            return View("~/Views/Panel/ProductService/Edit.cshtml", product);
        }
        #endregion

        #region Update Product
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ProductModel product)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Panel/ProductService/Edit.cshtml");
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("product_id", id.ToString()));
            parameters.Add(new KeyValuePair<string, string>("i_product_type", product.type));
            parameters.Add(new KeyValuePair<string, string>("i_title", product.title));
            parameters.Add(new KeyValuePair<string, string>("i_description", product.description));
            parameters.Add(new KeyValuePair<string, string>("i_price", product.price));
            parameters.Add(new KeyValuePair<string, string>("i_is_sold", product.is_sold));
            parameters.Add(new KeyValuePair<string, string>("i_is_purchased", product.is_purchased));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("update_product", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                if (_dtResp.Rows[0]["response"].ToString() == "0")
                {
                    ViewData["ErrorMessage"] = _dtResp.Rows[0]["message"].ToString();
                }
                else
                {
                    ViewData["SuccessMessage"] = "Product updated successfuly.";
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Product service unavailable";
            }

            ViewData["ProductList"] = null;

            DataTable _dtResp2 = GetProductList();

            if (this._objHelper.checkDBNullResponse(_dtResp2))
            {
                ViewData["ProductList"] = _dtResp2;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }

            ViewData["ProductList"] = GetProductList();
            return View("~/Views/Panel/ProductService/Edit.cshtml");
        }
        #endregion

        #region Delete Product
        public IActionResult Delete(int id)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_product_master", id.ToString()));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("delete_product", parameters);
            TempData["DeleteMessage"] = "Product/Service Deleted Successfuly.";

            return RedirectToAction("Index");
        }
        #endregion

        private DataTable GetProductList()
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "product"));
            parameters.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);

            return _dtResp;
        }
    }
}