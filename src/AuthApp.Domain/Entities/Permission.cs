namespace AuthApp.Domain.Entities;

public class Permission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
