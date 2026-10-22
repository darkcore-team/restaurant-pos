namespace RestaurantPos.Shared.Infrastructure.Authentication;

public static class PosAuthCookies
{
    public const string AccessToken = "pos_access_token";

    public const string RefreshToken = "pos_refresh_token";

    public const string Csrf = "pos_csrf_token";

    public const string CsrfHeader = "X-CSRF-TOKEN";
}
