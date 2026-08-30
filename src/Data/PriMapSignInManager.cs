using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace PriMap.Data
{
    public class PriMapSignInManager(
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<ApplicationUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<ApplicationUser> confirmation)
        : SignInManager<ApplicationUser>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        public override async Task<bool> CanSignInAsync(ApplicationUser user)
        {
            if (!user.IsActive)
            {
                Logger.LogWarning("Utilizatorul {UserId} este dezactivat și a fost blocat la autentificare.", user.Id);
                return false;
            }

            return await base.CanSignInAsync(user);
        }
    }
}
