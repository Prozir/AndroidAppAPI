using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Security.OAuth;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AndroidAppAPI
{
    public class AppAuthorizationServiceProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            using (AppUserAuthentication OBJ = new AppUserAuthentication())
            {
                var user = OBJ.ValidateUser(context.UserName, context.Password);
                if (user == "False")
                {
                    context.SetError("invalid_grant", "Username or password is incorrect");
                    return;
                }
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                //identity.AddClaim(new Claim(ClaimTypes.Role, "SuperAdmin"));
                //identity.AddClaim(new Claim(ClaimTypes.Name, "testuser"));
                //identity.AddClaim(new Claim("Email", user.UserEmailID));  

                context.Validated(identity);
            }
        }
    }
}