using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController(
    IUserRepository userRepo,
    IPostRepository postRepo,
    ICommentRepository commentRepo) : ControllerBase
{
    // POST /users
    [HttpPost]
    public async Task<ActionResult<UserDto>> AddUser([FromBody] CreateUserDto request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("UserName and Password are required.");
        }

        if (IsUserNameTaken(request.UserName))
        {
            return Conflict($"Username '{request.UserName}' is already in use.");
        }

        User created = await userRepo.AddAsync(new User
        {
            UserName = request.UserName,
            Password = request.Password
        });

        UserDto dto = ToDto(created);
        return Created($"/users/{dto.Id}", dto);
    }

    // PUT /users/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserDto request)
    {
        User? user = FindUser(id);
        if (user is null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("UserName and Password are required.");
        }

        if (IsUserNameTaken(request.UserName, exceptUserId: id))
        {
            return Conflict($"Username '{request.UserName}' is already in use.");
        }

        user.UserName = request.UserName;
        user.Password = request.Password;
        await userRepo.UpdateAsync(user);
        return NoContent();
    }

    // GET /users/{id}
    [HttpGet("{id:int}")]
    public ActionResult<UserDto> GetSingleUser([FromRoute] int id)
    {
        User? user = FindUser(id);
        return user is null ? NotFound($"User with ID {id} not found.") : ToDto(user);
    }

    // GET /users?userName=abc
    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetUsers([FromQuery] string? userName)
    {
        IQueryable<User> users = userRepo.GetMany();

        if (!string.IsNullOrWhiteSpace(userName))
        {
            users = users.Where(u => u.UserName.Contains(userName, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(users.ToList().Select(ToDto));
    }

    // DELETE /users/{id}
    // Also removes the user's posts and comments (and comments on those posts),
    // so no orphaned data is left behind.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser([FromRoute] int id)
    {
        if (FindUser(id) is null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        List<int> postIds = postRepo.GetMany().Where(p => p.UserId == id).Select(p => p.Id).ToList();
        List<int> commentIds = commentRepo.GetMany()
            .Where(c => c.UserId == id || postIds.Contains(c.PostId))
            .Select(c => c.Id)
            .ToList();

        foreach (int commentId in commentIds)
        {
            await commentRepo.DeleteAsync(commentId);
        }

        foreach (int postId in postIds)
        {
            await postRepo.DeleteAsync(postId);
        }

        await userRepo.DeleteAsync(id);
        return NoContent();
    }

    private User? FindUser(int id) => userRepo.GetMany().SingleOrDefault(u => u.Id == id);

    private bool IsUserNameTaken(string userName, int? exceptUserId = null) =>
        userRepo.GetMany().Any(u =>
            u.Id != exceptUserId &&
            u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));

    private static UserDto ToDto(User user) => new() { Id = user.Id, UserName = user.UserName };
}
