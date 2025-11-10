namespace AuthApp.Domain.Permissions;

public static class PermissionCatalog
{
    public const string ManageUsers = "permissions.manage-users";
    public const string ViewUsers = "permissions.view-users";
    public const string ManagePermissions = "permissions.manage-permissions";
    public const string ViewPermissions = "permissions.view-permissions";

    public static IReadOnlyCollection<string> All => new[]
    {
        ManageUsers,
        ViewUsers,
        ManagePermissions,
        ViewPermissions
    };
}
