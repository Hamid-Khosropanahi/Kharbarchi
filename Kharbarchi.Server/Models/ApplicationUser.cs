using Microsoft.AspNetCore.Identity;

namespace Kharbarchi.Server.Models;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}