using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace AndroidAppAPI
{
    public class AppUserAuthentication : IDisposable
    {
        private static string connString = ConfigurationManager.ConnectionStrings["Storage_db23ConnString"].ConnectionString;
        SqlConnection con = new SqlConnection(connString);
        public string ValidateUser(string username, string password)
        {
            string Name = username;
            string Pass = password;
            string validateUserQuery = "SELECT TOP 1 UserId FROM dbo.Master_USER WHERE Username = '" + Name + "' AND Password = '" + Pass + "';";
            SqlCommand cmd = new SqlCommand(validateUserQuery, con);
            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();
            if (i == 1)
            {
                return "true";
            }
            else
            {
                return "False";
            }
        }
        public void Dispose()
        {
            //Dispose();  
        }
    }
}