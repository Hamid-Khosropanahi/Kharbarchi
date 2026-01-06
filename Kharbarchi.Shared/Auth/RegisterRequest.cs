using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kharbarchi.Shared.Auth;

public class RegisterRequest
{
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string? Email { get; set; }
    public string? FullName { get; set; }
}