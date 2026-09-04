using System.ComponentModel.DataAnnotations;
using JwtTest.Models;

namespace JwtTest.Features.UserFeature.DTOs;

public sealed record CreateUserRequest
(
    [Required(ErrorMessage ="Username is required!"), StringLength(150)]
    string Username,
    [Required(ErrorMessage ="Email is required!"), StringLength(40)]
    string Email,
    [Required(ErrorMessage ="Password is required!"),StringLength(150)]
    string Passsword,
    [Required(ErrorMessage ="Atleast one Role is required!")]
    List<Guid> Roles
);
public sealed record UpdateUserRequest
(
    int Id,
    [Required(ErrorMessage ="Username is required!"), StringLength(150)]
    string Username,
    [Required(ErrorMessage ="Email is required!"), StringLength(40)]
    string Email,
    [Required(ErrorMessage ="Password is required!"),StringLength(150)]
    string Passsword,
    [Required(ErrorMessage ="Atleast one Role is required!")]
    List<Guid> Roles
);

public sealed record UserResponse(int Id, string UserName, string Email, List<RoleResponse> Roles);
public sealed record RoleResponse(int Id, string RoleName, List<PermissionResponse> Permissions);
public sealed record PermissionResponse(int Id, string PermissionName);
