using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountingPlanner.System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AccountingPlanner.Controllers.Admin
{
    public class AdminUsersController : Controller
    {
        #region Controller Properties
        private IConfiguration _configuration;
        private CommonHelper _objHelper = new CommonHelper();
        private MySQLGateway _objDataHelper;
        #endregion

        public AdminUsersController(IConfiguration configuration)
        {
            this._configuration = configuration;
            this._objDataHelper = new MySQLGateway(this._configuration.GetConnectionString("Connection"));
        }

        public IActionResult Index()
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "users"));
            parameters.Add(new KeyValuePair<string, string>("param1", ""));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);
            List<Dictionary<string, string>> resp = new List<Dictionary<string, string>>();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                ViewData["UsersList"] = _dtResp;
            }

            return View("~/Views/Admin/Users/Index.cshtml");
        }

        public IActionResult ChangeStatus(int id)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("user_id", id.ToString()));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("change_user_status", parameters);
            List<Dictionary<string, string>> resp = new List<Dictionary<string, string>>();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                ViewData["UsersList"] = _dtResp;
            }

            return RedirectToAction("Index");
        }
    }
}