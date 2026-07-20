namespace JwtTest.Middlewares.auth;

public interface IPermissionService
{
    Task<bool> HasPermission(int userId, string permission);
}
