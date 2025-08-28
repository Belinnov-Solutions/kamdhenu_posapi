using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

public class RoleHierarchyClaimsTransformer : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is ClaimsIdentity identity)
        {
            var roles = identity.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var additionalRoles = new List<string>();

            // Example hierarchy logic
            if (roles.Contains("SuperAdmin"))
            {
                additionalRoles.Add("Admin");
                additionalRoles.Add("Manager");
            }
            else if (roles.Contains("Admin"))
            {
                additionalRoles.Add("Manager");
            }

            foreach (var role in additionalRoles)
            {
                // Add role claim if not already present
                if (!roles.Contains(role))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }
        }

        return Task.FromResult(principal);
    }
}
