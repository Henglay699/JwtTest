namespace JwtTest.Models.Entities;

public class Permission
{
    public int Id { get; set; }
    public AppPermission PermissionName { get; set; }
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
