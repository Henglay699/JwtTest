using JwtTest.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JwtTest.Features.AttendanceFeature;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController(JwtTestContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var attendances = await db.Attendances.ToListAsync();
        return Ok(attendances);
    }
}
