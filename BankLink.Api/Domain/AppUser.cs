using Microsoft.AspNetCore.Identity;

namespace BankLink.Api.Domain
{
    public class AppUser : IdentityUser<int>
    {
        public string? FullName { get; set; }
    }
}
