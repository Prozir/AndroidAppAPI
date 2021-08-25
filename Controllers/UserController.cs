using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AndroidAppAPI.Controllers
{
    public class UserController : ApiController
    {
        [HttpPost]
        [Route("api/User/GetUserData")]
        [Authorize]        
        public string GetUserData([FromUri] string username)
        {
            var connStr = ConfigurationManager.ConnectionStrings["Storage_db23ConnString"].ConnectionString;
            SqlConnection con = new SqlConnection(connStr);

            SqlDataAdapter da = new SqlDataAdapter("select * FROM dbo.Master_USER WHERE Username = '" + username + "';", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                return JsonConvert.SerializeObject(dt);
            }
            else
            {
                return "No data available for specified user.";
            }
        }     
    }
}
