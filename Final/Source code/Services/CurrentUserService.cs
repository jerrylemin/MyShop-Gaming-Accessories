using ProjectTest.Models;

namespace ProjectTest.Services;

public class CurrentUserService
{
    public AppUser CurrentUser { get; private set; } = new()
    {
        Id = 1,
        Username = "admin",
        DisplayName = "Administrator",
        Role = UserRole.Admin
    };

    public bool CanViewImportPrice => CurrentUser.Role is UserRole.Admin or UserRole.Moderator;

    public bool CanViewAllOrders => CurrentUser.Role is UserRole.Admin or UserRole.Moderator;

    public void SetCurrentUser(AppUser user)
    {
        CurrentUser = user;
    }
}
