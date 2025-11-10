namespace AuthApp.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute(params string[] permissions) : Attribute
{
    public IReadOnlyCollection<string> Permissions { get; } = Normalize(permissions);

    public static RequirePermissionAttribute For(params string[] permissions) => new(permissions);

    private static IReadOnlyCollection<string> Normalize(IEnumerable<string> permissions) =>
        permissions?
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim().ToLowerInvariant())
            .Distinct()
            .ToArray() ?? Array.Empty<string>();
}
