using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AndroidAppAPI.Controllers
{
    public class FileController : ApiController
    {
        [HttpPost]
        [Route("api/File/UploadFile")]
        [Authorize]
        public async Task<string> UploadFile()
        {
            var ctx = HttpContext.Current;
            var root = ctx.Server.MapPath("~/AppFileUploads");
            var provider = new MultipartFormDataStreamProvider(root);
            var fileguid = string.Empty;
            var filetype = string.Empty;
            var datadate = string.Empty;
            var type = string.Empty;
            var pannumber = string.Empty;
            var finalfilename = string.Empty;
            var fileuploaddate = string.Empty;
            int masteruserid = 0;
            int filestatus = 0;
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (var key in provider.FormData.AllKeys)
                {
                    foreach (var val in provider.FormData.GetValues(key))
                    {
                        //Console.WriteLine(string.Format("{0}: {1}", key, val));
                        if (key == "filetype")
                            filetype = val;
                        else if (key == "type")
                            type = val;
                        else if (key == "pannumber")
                            pannumber = val;
                        else if (key == "finalfilename")
                            finalfilename = val;
                        else if (key == "masteruserid")
                            masteruserid = Int32.Parse(val);
                        else if (key == "filestatus")
                            filestatus = Int32.Parse(val);

                    }
                }

                foreach (var file in provider.FileData)
                {
                    var name = file.Headers.ContentDisposition.FileName;
                    // remove double quotes from the file name
                    name = name.Trim('"');
                    var localFileName = file.LocalFileName;
                    var filePath = Path.Combine(root, name);
                    //   File.Move(localFileName, filePath);

                    Guid obj = Guid.NewGuid();
                    fileguid = obj.ToString();
                    // datadate = DateTime.Now;
                    // fileuploaddate = DateTime.Now;
                    SaveFilePathToSQLServer(localFileName, filePath, filetype, type, pannumber, finalfilename, masteruserid, filestatus, fileguid);
                }


            }
            catch (Exception e)
            {
                return $"Error: {e.Message}";
            }

            return "File uploaded successfully!";
        }

        private void SaveFileToSQLServer(string localFile, string fileName)
        {
            // get the file contents
            byte[] fileBytes;
            using (var fs = new FileStream(localFile, FileMode.Open, FileAccess.Read))
            {
                fileBytes = new byte[fs.Length];
                fs.Read(fileBytes, 0, Convert.ToInt32(fs.Length));
            }

            var connStr = ConfigurationManager.ConnectionStrings["Storage_db23ConnString"].ConnectionString;

            //push to database
            var query = "Insert into Files(FileBin, Name, Size) " + "values (@FileBin, @Name, @Size);";

            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.Add(
                    "@FileBin",
                    SqlDbType.VarBinary)
                    .Value = fileBytes;

                cmd.Parameters.Add(
                    "@Name",
                    SqlDbType.VarChar, 50)
                    .Value = fileName;

                cmd.Parameters.Add(
                    "@Size",
                    SqlDbType.Int)
                    .Value = fileBytes.Length;

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        private void SaveFilePathToSQLServer(string localFile, string filepath, string filetype, string type, string pannumber, string finalfilename, int masteruserid, int filestatus, string fileguid)
        {
            // 1) Move file to folder
            File.Move(localFile, filepath);

            // 2) connection string
            var connStr = ConfigurationManager.ConnectionStrings["Storage_db23ConnString"].ConnectionString;

            // 3) Insert in DB
            var query = "Insert into FILESUPLOADED(FIELDGUID,FILETYPE,DATADATE,TYPE,PANNUMBER,FINALFILENAME,FILEUPLOADEDON,MASTER_USERID,FILESTATUS,FILEPATH) "
                        + "values (@FileGuid,@FileType,@DataDate,@Type,@Pannumber,@Finalfilename,@Fileuploadedon,@Masteruserid,@Filestatus,@FilePath);";

            using (var conn = new SqlConnection(connStr))
            using (var cmd = new SqlCommand(query, conn))
            {

                cmd.Parameters.Add(
                    "@FileGuid",
                    SqlDbType.VarChar,
                    200)
                    .Value = fileguid;
                cmd.Parameters.Add(
                    "@FileType",
                    SqlDbType.VarChar,
                    200)
                    .Value = filetype;
                cmd.Parameters.Add(
                    "@DataDate",
                    SqlDbType.DateTime,
                    200)
                    .Value = DateTime.Now;
                cmd.Parameters.Add(
                    "@Type",
                    SqlDbType.VarChar,
                    200)
                    .Value = type;
                cmd.Parameters.Add(
                    "@Pannumber",
                    SqlDbType.VarChar,
                    200)
                    .Value = pannumber;
                cmd.Parameters.Add(
                    "@Finalfilename",
                    SqlDbType.VarChar,
                    200)
                    .Value = finalfilename;
                cmd.Parameters.Add(
                    "@Fileuploadedon",
                    SqlDbType.DateTime,
                    200)
                    .Value = DateTime.Now;
                cmd.Parameters.Add(
                    "@Masteruserid",
                    SqlDbType.Int,
                    200)
                    .Value = masteruserid;
                cmd.Parameters.Add(
                    "@Filestatus",
                    SqlDbType.Int,
                    200)
                    .Value = filestatus;

                cmd.Parameters.Add(
                    "@FilePath",
                    SqlDbType.VarChar,
                    200)
                    .Value = filepath;

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }




        [HttpPost]
        [Route("api/File/GetFiles")]
        [Authorize]

        public HttpResponseMessage GetUserFiles([FromUri] string username)
        {
            var connStr = ConfigurationManager.ConnectionStrings["Storage_db23ConnString"].ConnectionString;
            SqlConnection con = new SqlConnection(connStr);

            // get user master user id  from Master User table
            SqlDataAdapter userdatada = new SqlDataAdapter("select * FROM dbo.Master_USER WHERE Username = '" + username + "' ;", con);
            DataTable userdatadt = new DataTable();
            userdatada.Fill(userdatadt);

            int currentuserid = Int32.Parse(userdatadt.Rows[0]["USERID"].ToString());



            //get all uploaded files for the current user id
            SqlDataAdapter da = new SqlDataAdapter("select * FROM dbo.FILESUPLOADED WHERE MASTER_USERID = '" + currentuserid + "' ;", con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
               // string fileuploaddetailsstring = JsonConvert.SerializeObject(dt);
               // return fileuploaddetailsstring;
                return Request.CreateResponse(HttpStatusCode.OK, dt);
            }
            else
            {
               // return "No file upload data available for specified user.";
                return Request.CreateResponse(HttpStatusCode.NotFound, "No file uploaded data available");
            }
        }

        // download file

        [HttpGet]
        [Route("api/File/downloadfile")]
        [Authorize]

        public HttpResponseMessage DownloadFile([FromUri] string filename)
        {
            if (filename != null)
            {
                var result = new HttpResponseMessage(HttpStatusCode.OK);
                var fileName = filename;
                var filePath = HttpContext.Current.Server.MapPath($"~/AppFileUploads/{fileName}");

                var fileBytes = File.ReadAllBytes(filePath);
                var fileMemStream = new MemoryStream(fileBytes);
                result.Content = new StreamContent(fileMemStream);
                var headers = result.Content.Headers;
                headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                headers.ContentDisposition.FileName = fileName;
                headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                //new MediaTypeHeaderValue("application/jpg");
                headers.ContentLength = fileMemStream.Length;
                return result;
            }

            else
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.NotFound, "File not found");
                return response;
            }                

        }
    }
   
}
