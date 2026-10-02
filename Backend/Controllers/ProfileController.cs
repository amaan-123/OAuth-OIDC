using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    [Authorize]
    [HttpGet]
    public IActionResult GetProfile()
    {
        return Ok(new
        {
            Message = "You are authenticated!",
            User = User.Identity?.Name,
            Claims = User.Claims.Select(c => new
            {
                c.Type,
                c.Value
            })
        });
    }
}