using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountingPlanner.Models.Expense;
using AccountingPlanner.System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AccountingPlanner.Controllers.Panel.Expense
{
    public class RecieptController : Controller
    {

        #region Controller Properties
        private IConfiguration _configuration;
        private IHostingEnvironment _env;
        private CommonHelper _objHelper = new CommonHelper();
        private MySQLGateway _objDataHelper;
        #endregion

        public RecieptController(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            this._objDataHelper = new MySQLGateway(this._configuration.GetConnectionString("Connection"));
        }

        public IActionResult Index()
        {
            ViewData["RecieptList"] = null;

            DataTable _dtResp = GetRecieptList();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                ViewData["RecieptList"] = _dtResp;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }
            ViewData["CountryList"] = GetCountryList();

            return View("~/Views/Panel/Reciepts/Index.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RecieptModel recieptModel)
        {
            if (!ModelState.IsValid)
            {
                ViewData["CountryList"] = GetCountryList();
                return View("~/Views/Panel/Reciepts/Index.cshtml");
            }

            if (!(recieptModel.image.ContentType == "image/png" || recieptModel.image.ContentType == "image/jpg" || recieptModel.image.ContentType == "image/jpeg"))
            {
                TempData["ImageUploadError"] = "Unsupported image type. Kindly re-upload a JPG/JPEG/PNG only.";
                return RedirectToAction("Index", "Profile");
            }

            string dbPath = "/uploads/profile_picure";
            string path = _env.WebRootPath + dbPath;
            string localFileName = Guid.NewGuid().ToString() + "." + recieptModel.image.ContentType.Split("/")[recieptModel.image.ContentType.Split("/").Length - 1];

            byte[] imageStream;
            using (var memoryStream = new MemoryStream())
            {
                await recieptModel.image.CopyToAsync(memoryStream);
                imageStream = memoryStream.ToArray();
            }

            using (var fs = new FileStream($"{path}/{localFileName}", FileMode.Create, FileAccess.Write))
            {
                await fs.WriteAsync(imageStream, 0, imageStream.Length);
            }

            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_id_organization", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("i_image", $"{dbPath}/{localFileName}"));
            parameters.Add(new KeyValuePair<string, string>("i_dated", recieptModel.dated));
            parameters.Add(new KeyValuePair<string, string>("i_notes", recieptModel.notes));
            parameters.Add(new KeyValuePair<string, string>("i_category", recieptModel.category));
            parameters.Add(new KeyValuePair<string, string>("i_payment_account", recieptModel.payment_account));
            parameters.Add(new KeyValuePair<string, string>("i_subtotal", recieptModel.subtotal));
            parameters.Add(new KeyValuePair<string, string>("i_currency", recieptModel.currency));
            parameters.Add(new KeyValuePair<string, string>("i_total", recieptModel.total));
            parameters.Add(new KeyValuePair<string, string>("i_added_by", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_user")));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("insert_reciept", parameters);

            if (this._objHelper.checkDBResponse(_dtResp))
            {
                if (_dtResp.Rows[0]["response"].ToString() == "0")
                {
                    ViewData["ErrorMessage"] = _dtResp.Rows[0]["message"].ToString();
                }
                else
                {
                    ViewData["SuccessMessage"] = "Reciept registered successfuly.";
                }
            }
            else
            {
                ViewData["ErrorMessage"] = "Reciept service unavailable";
            }

            ViewData["RecieptList"] = null;

            DataTable _dtResp2 = GetRecieptList();

            if (this._objHelper.checkDBNullResponse(_dtResp2))
            {
                ViewData["RecieptList"] = _dtResp2;
            }
            else
            {
                ViewData["ListErrorMessage"] = "Unable to fetch data. Try again later.";
            }
            ViewData["CountryList"] = GetCountryList();

            return View("~/Views/Panel/Reciepts/Index.cshtml");
        }

        #region Delete Customer
        public ActionResult Delete(int id)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("i_reciept_id", id.ToString()));
            TempData["DeleteMessage"] = "Customer Deleted Successfuly.";

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("delete_reciept", parameters);

            return RedirectToAction("Index");
        }
        #endregion

        private DataTable GetRecieptList()
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "reciept"));
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