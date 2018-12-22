using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AccountingPlanner.System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AccountingPlanner.Controllers.Generic
{
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    [Route("api/panel")]
    public class Panel : ControllerBase
    {
        #region Controller Properties
        private IConfiguration _configuration;
        private IHostingEnvironment _env;
        private CommonHelper _objHelper = new CommonHelper();
        private MySQLGateway _objDataHelper;
        #endregion

        public Panel(IConfiguration configuration, IHostingEnvironment env)
        {
            this._configuration = configuration;
            this._env = env;
            this._objDataHelper = new MySQLGateway(this._configuration.GetConnectionString("Connection"));
        }

        [HttpGet("BusinessList")]
        public JsonResult BusinessList()
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "user_business"));
            parameters.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_user")));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);
            List<Dictionary<string, string>> resp = new List<Dictionary<string, string>>();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                resp =  _objHelper.ConvertTableToDictionary(_dtResp);
            }

            return new JsonResult(resp);
        }

        [HttpGet("BusinessList/{id}/{name}")]
        public async Task<JsonResult> BusinessListAsync(string id, string name)
        {
            await HttpContext.SignOutAsync(scheme: CookieAuthenticationDefaults.AuthenticationScheme);

            var claims = new List<Claim>
                    {
                        new Claim("name", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "name")),
                        new Claim("id_user", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_user")),
                        new Claim("id_organization", id),
                        new Claim("organization_name", name),
                        new Claim("profile_img", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "profile_img")),
                        new Claim("pid", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "pid"))
                    };

            var userIdentity = new ClaimsIdentity(claims, "Cookie");

            ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
            await HttpContext.SignInAsync(
                scheme: CookieAuthenticationDefaults.AuthenticationScheme,
                principal: principal
            );

            Dictionary<string, string> resp = new Dictionary<string, string>();

            resp.Add("success", true.ToString());

            return new JsonResult(resp);
        }

        [HttpGet("ProductsList")]
        public JsonResult ProductsList()
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "product"));
            parameters.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);
            List<Dictionary<string, string>> resp = new List<Dictionary<string, string>>();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                resp = _objHelper.ConvertTableToDictionary(_dtResp);
            }

            return new JsonResult(resp);
        }

        [HttpGet("CountryList")]
        public JsonResult CountryList()
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "country"));
            parameters.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);
            List<Dictionary<string, string>> resp = new List<Dictionary<string, string>>();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                resp = _objHelper.ConvertTableToDictionary(_dtResp);
            }

            return new JsonResult(resp);
        }

        [HttpGet("Product/{id}")]
        public JsonResult Product(string id)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "product_by_id"));
            parameters.Add(new KeyValuePair<string, string>("param1", id));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);
            List<Dictionary<string, string>> resp = new List<Dictionary<string, string>>();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                resp = _objHelper.ConvertTableToDictionary(_dtResp);
            }

            return new JsonResult(resp);
        }

        [HttpGet("TaxList")]
        public JsonResult TaxList()
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();

            parameters.Add(new KeyValuePair<string, string>("selectBy", "tax"));
            parameters.Add(new KeyValuePair<string, string>("param1", _objHelper.GetTokenData(HttpContext.User.Identity as ClaimsIdentity, "id_organization")));
            parameters.Add(new KeyValuePair<string, string>("param2", ""));

            DataTable _dtResp = _objDataHelper.ExecuteProcedure("entity_master_select", parameters);
            List<Dictionary<string, string>> resp = new List<Dictionary<string, string>>();

            if (this._objHelper.checkDBNullResponse(_dtResp))
            {
                resp = _objHelper.ConvertTableToDictionary(_dtResp);
            }

            return new JsonResult(resp);
        }
    }
}