using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        if (await IsUserNameTakenAsync(request.UserName))
        {
            return Conflict($"Username '{request.UserName}' is already in use.");
        }

        User created = await userRepo.AddAsync(new User(request.UserName, request.Password)
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
        User? user = await FindUserAsync(id);
        if (user is null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("UserName and Password are required.");
        }

        if (await IsUserNameTakenAsync(request.UserName, exceptUserId: id))
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
    public async Task<ActionResult<UserDto>> GetSingleUser([FromRoute] int id)
    {
        User? user = await FindUserAsync(id);
        return user is null ? NotFound($"User with ID {id} not found.") : ToDto(user);
    }

    // GET /users?userName=abc
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery] string? userName)
    {
        IQueryable<User> users = userRepo.GetMany();

        if (!string.IsNullOrWhiteSpace(userName))
        {
            string lowerName = userName.ToLower();
            users = users.Where(u => u.UserName.ToLower().Contains(lowerName));
        }

        List<UserDto> result = await users
            .Select(u => new UserDto { Id = u.Id, UserName = u.UserName })
            .ToListAsync();

        return Ok(result);
    }

    // DELETE /users/{id}
    // Also removes the user's posts and comments (and comments on those posts),
    // so no data is left behind.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser([FromRoute] int id)
    {
        if (await FindUserAsync(id) is null)
        {
            return NotFound($"User with ID {id} not found.");
        }

        List<int> postIds = await postRepo.GetMany()
            .Where(p => p.UserId == id)
            .Select(p => p.Id)
            .ToListAsync();
        
        List<int> commentIds = await commentRepo.GetMany()
            .Where(c => c.UserId == id || postIds.Contains(c.PostId))
            .Select(c => c.Id)
            .ToListAsync();

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

    private Task<User?> FindUserAsync(int id) =>
        userRepo.GetMany().SingleOrDefaultAsync(u => u.Id == id);

    private Task<bool> IsUserNameTakenAsync(string userName,
        int? exceptUserId = null)
    {
        string lowerName = userName.ToLower();
        return userRepo.GetMany().AnyAsync(u => 
            u.Id != exceptUserId &&
            u.UserName.ToLower() == lowerName);
    }
    private static UserDto ToDto(User user) => new() { Id = user.Id, UserName = user.UserName };
}
