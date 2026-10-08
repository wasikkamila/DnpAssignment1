using ApiContracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RepositoryContracts;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CommentsController(
    ICommentRepository commentRepo,
    IPostRepository postRepo,
    IUserRepository userRepo) : ControllerBase
{
    // POST /comments
    [HttpPost]
    public async Task<ActionResult<CommentDto>> AddComment([FromBody] CreateCommentDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Body is required.");
        }

        User? author = userRepo.GetMany().SingleOrDefault(u => u.Id == request.UserId);
        if (author is null)
        {
            return BadRequest($"User with ID {request.UserId} does not exist.");
        }

        if (!postRepo.GetMany().Any(p => p.Id == request.PostId))
        {
            return BadRequest($"Post with ID {request.PostId} does not exist.");
        }

        Comment created = await commentRepo.AddAsync(new Comment
        {
            Body = request.Body,
            UserId = request.UserId,
            PostId = request.PostId
        });

        CommentDto dto = ToDto(created, author.UserName);
        return Created($"/comments/{dto.Id}", dto);
    }

    // PUT /comments/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateComment([FromRoute] int id, [FromBody] UpdateCommentDto request)
    {
        Comment? comment = FindComment(id);
        if (comment is null)
        {
            return NotFound($"Comment with ID {id} not found.");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            return BadRequest("Body is required.");
        }

        comment.Body = request.Body;
        await commentRepo.UpdateAsync(comment);
        return NoContent();
    }

    // GET /comments/{id}
    [HttpGet("{id:int}")]
    public ActionResult<CommentDto> GetSingleComment([FromRoute] int id)
    {
        Comment? comment = FindComment(id);
        if (comment is null)
        {
            return NotFound($"Comment with ID {id} not found.");
        }

        return ToDto(comment, UserNames());
    }

    // GET /comments?userId=1&userName=kam&postId=2
    [HttpGet]
    public ActionResult<IEnumerable<CommentDto>> GetComments(
        [FromQuery] int? userId,
        [FromQuery] string? userName,
        [FromQuery] int? postId)
    {
        Dictionary<int, string> userNames = UserNames();
        IQueryable<Comment> comments = commentRepo.GetMany();

        if (userId is not null)
        {
            comments = comments.Where(c => c.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(userName))
        {
            List<int> matchingUserIds = userNames
                .Where(kv => kv.Value.Contains(userName, StringComparison.OrdinalIgnoreCase))
                .Select(kv => kv.Key)
                .ToList();
            comments = comments.Where(c => matchingUserIds.Contains(c.UserId));
        }

        if (postId is not null)
        {
            comments = comments.Where(c => c.PostId == postId);
        }

        return Ok(comments.ToList().Select(c => ToDto(c, userNames)));
    }

    // DELETE /comments/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteComment([FromRoute] int id)
    {
        if (FindComment(id) is null)
        {
            return NotFound($"Comment with ID {id} not found.");
        }

        await commentRepo.DeleteAsync(id);
        return NoContent();
    }

    private Comment? FindComment(int id) => commentRepo.GetMany().SingleOrDefault(c => c.Id == id);

    private Dictionary<int, string> UserNames() => userRepo.GetMany().ToDictionary(u => u.Id, u => u.UserName);

    private static CommentDto ToDto(Comment comment, Dictionary<int, string> userNames) =>
        ToDto(comment, userNames.TryGetValue(comment.UserId, out string? name) ? name : "(unknown user)");

    private static CommentDto ToDto(Comment comment, string userName) => new()
    {
        Id = comment.Id,
        Body = comment.Body,
        UserId = comment.UserId,
        UserName = userName,
        PostId = comment.PostId
    };
}
