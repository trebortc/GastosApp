using Microsoft.AspNetCore.Identity;

namespace GastosApp.Infrastructure.Identity;

/// <summary>Rol de la aplicacion (nivel global, no de grupo).</summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
