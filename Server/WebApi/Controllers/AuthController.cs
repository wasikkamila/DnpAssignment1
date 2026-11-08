using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IUserRepository userRepo) : ControllerBase
{
    // POST /auth/login
    [HttpPost("login")]
    public ActionResult<UserDto> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("UserName and Password are required.");
        }

        User? user = userRepo.GetMany().SingleOrDefault(u =>
            u.UserName.Equals(request.UserName, StringComparison.OrdinalIgnoreCase));

        // Same message for both cases, so we don't reveal which user names exist.
        if (user is null || user.Password != request.Password)
        {
            return Unauthorized("Invalid user name or password.");
        }

        return Ok(new UserDto { Id = user.Id, UserName = user.UserName });
    }
}