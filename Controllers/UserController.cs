using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using System.Web.Script.Serialization;

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

            // get user data from Master User table
            SqlDataAdapter da = new SqlDataAdapter("select * FROM dbo.Master_USER WHERE Username = '" + username + "';", con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            //get master user id          
            int currentuserid = Int32.Parse(dt.Rows[0]["USERID"].ToString());
            int[] panidarr;
            List<int> temppanidlist = new List<int>();

            //get all master pan id from UserPancard mapping table for the master user id
            SqlDataAdapter pancardmappingda = new SqlDataAdapter("select * FROM dbo.USER_PANCARDMAPPING WHERE MASTER_USER_USERID = '" + currentuserid + "';", con);
            DataTable pancardmappingdt = new DataTable();
            pancardmappingda.Fill(pancardmappingdt);

            if( pancardmappingdt.Rows.Count >0)
            {
                //collect allpancard ids
                for(int i=0; i<pancardmappingdt.Rows.Count;i++)
                {
                    temppanidlist.Add(Int32.Parse(pancardmappingdt.Rows[i]["MASTER_PAN_PANID"].ToString()));
                }
            }
            panidarr = temppanidlist.ToArray();

            //get actual pancardnumber from Master Pan table for all the pancard ids
            string allpancardids = string.Empty;

            for(int i = 0; i <panidarr.Length;i++)
            {
                if (i == panidarr.Length - 1)
                    allpancardids += panidarr[i];
                else
                    allpancardids += panidarr[i] + ",";
            }

            SqlDataAdapter pancartdnumberda = new SqlDataAdapter("SELECT PANCARDNUMBER FROM MASTER_PAN WHERE PANID IN (" + allpancardids + ")", con);
            DataTable pancardnumberdt = new DataTable();
            pancartdnumberda.Fill(pancardnumberdt);
            string allpancardnumber = string.Empty;
            
            for(int i=0; i<pancardnumberdt.Rows.Count;i++)
            {
                if (i == pancardnumberdt.Rows.Count - 1)
                    allpancardnumber += pancardnumberdt.Rows[i]["PANCARDNUMBER"].ToString();
                else
                    allpancardnumber += pancardnumberdt.Rows[i]["PANCARDNUMBER"].ToString() + ",";
            }

            if (dt.Rows.Count > 0)
            {
                dt.Rows[0]["PANCARDNUMBER"] = allpancardnumber;
                string userdetailsstring = JsonConvert.SerializeObject(dt);
                return userdetailsstring;
            }
            else
            {
                return "No data available for specified user.";
            }
        }

        public string DataTableToJSONWithJavaScriptSerializer(DataTable table)
        {
            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            List<Dictionary<string, object>> parentRow = new List<Dictionary<string, object>>();
            Dictionary<string, object> childRow;
            foreach (DataRow row in table.Rows)
            {
                childRow = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    childRow.Add(col.ColumnName, row[col]);
                }
                parentRow.Add(childRow);
            }
            return jsSerializer.Serialize(parentRow);
        }
    }
}
