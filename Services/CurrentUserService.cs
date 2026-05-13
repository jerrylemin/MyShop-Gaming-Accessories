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

    public bool IsAdmin => CurrentUser.Role == UserRole.Admin;

    public bool IsModerator => CurrentUser.Role == UserRole.Moderator;

    public bool IsSale => CurrentUser.Role == UserRole.Sale;

    public bool CanViewProductList => CurrentUser.Role is UserRole.Admin or UserRole.Moderator or UserRole.Sale;

    public bool CanViewImportPrice => CurrentUser.Role is UserRole.Admin or UserRole.Moderator;

    public bool CanViewAllOrders => CurrentUser.Role is UserRole.Admin or UserRole.Moderator;

    public bool CanManageProducts => CurrentUser.Role == UserRole.Admin;

    public bool CanImportProducts => CurrentUser.Role == UserRole.Admin;

    public bool CanManageCategories => CurrentUser.Role == UserRole.Admin;

    public bool CanCreateOrders => CurrentUser.Role is UserRole.Admin or UserRole.Sale;

    public bool CanManageOrders => CurrentUser.Role is UserRole.Admin or UserRole.Sale;

    public bool CanDeleteOrders => CurrentUser.Role == UserRole.Admin;

    public bool CanViewReports => CurrentUser.Role is UserRole.Admin or UserRole.Moderator;

    public bool CanManageCustomers => CurrentUser.Role is UserRole.Admin or UserRole.Sale;

    public bool CanDeleteCustomers => CurrentUser.Role == UserRole.Admin;

    public bool CanManageSettings => CurrentUser.Role == UserRole.Admin;

    public bool CanUseTechnicalTools => CurrentUser.Role == UserRole.Admin;

    public string RoleDisplayName => CurrentUser.Role switch
    {
        UserRole.Admin => "Admin - full access",
        UserRole.Moderator => "Moderator - view product list, review orders, customers, and reports",
        UserRole.Sale => "Sale - view product list, create orders, and manage customers",
        _ => "User"
    };

    public void SetCurrentUser(AppUser user)
    {
        CurrentUser = user;
    }
}
