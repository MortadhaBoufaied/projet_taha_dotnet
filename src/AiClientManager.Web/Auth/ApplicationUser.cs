using AspNetCore.Identity.MongoDbCore.Models;

namespace AiClientManager.Web.Auth;

public class ApplicationUser : MongoIdentityUser<Guid>
{
    public ApplicationUser() : base() { }
    public ApplicationUser(string userName, string email) : base(userName, email) { }
}
