using AspNetCore.Identity.MongoDbCore.Models;

namespace AiClientManager.Web.Auth;

public class ApplicationRole : MongoIdentityRole<Guid>
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
