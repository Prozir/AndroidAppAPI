using System.Linq;
using System.Web.Http;
using System.Security.Claims;
using System.Net.Http;
using System.Net;

namespace AndroidAppAPI.Controllers
{
    public class LoginController : ApiController
    {
        [Authorize(Roles = "SuperAdmin, Admin, User")]
        [HttpGet]
        [Route("api/test/method1")]
        public HttpResponseMessage Post(AppLoginClass myclass)
        {
            var identity = (ClaimsIdentity)User.Identity;
            var roles = identity.Claims
                        .Where(c => c.Type == ClaimTypes.Role)
                        .Select(c => c.Value);

            myclass.Role = roles.ToString();
            myclass.Name = identity.Name;

            return Request.CreateResponse(HttpStatusCode.Created, myclass);
        }
    }
}
