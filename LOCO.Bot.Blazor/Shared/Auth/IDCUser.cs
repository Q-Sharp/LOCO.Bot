namespace LOCO.Bot.Blazor.Shared.Auth;

public interface IDCUser
{
    string Name { get; set; }
    ulong Id { get; set; }
    string AvatarUrl { get; set; }

    bool IsAuthenticated { get; set; }

    string NameClaimType { get; set; }

    string RoleClaimType { get; set; }

    ICollection<ClaimValue> Claims { get; set; }
}
