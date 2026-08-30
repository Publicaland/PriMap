using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using PriMap.Data;

namespace PriMap.Components.Account
{
    internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IEmailSender emailSender = new NoOpEmailSender();

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
            emailSender.SendEmailAsync(email, "Confirmă adresa de email", $"Te rugăm să îți confirmi contul făcând click <a href='{confirmationLink}'>aici</a>.");

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
            emailSender.SendEmailAsync(email, "Resetează parola", $"Te rugăm să resetezi parola făcând click <a href='{resetLink}'>aici</a>.");

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
            emailSender.SendEmailAsync(email, "Resetează parola", $"Te rugăm să resetezi parola folosind următorul cod: {resetCode}");
    }
}
