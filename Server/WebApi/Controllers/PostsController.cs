using ApiContracts;
using Entities;
using RepositoryContracts;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;


[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
    private IUserRepository userRepository;
    private IPostRepository postRepository;
    private ICommentRepository commentRepository;

    public PostsController(IUserRepository userRepository,
        IPostRepository postRepository, ICommentRepository commentRepository)
    {
        this.userRepository = userRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
    }
    
    [HttpPost]
    public async Task<ActionResult<PostDto>> AddPost(
        [FromBody] CreatePostDto request)
    {
        if (string.IsNullOrEmpty(request.Title) ||
            string.IsNullOrEmpty(request.Body))
        {
            return BadRequest("Title and Body are required");
        }
        
        User? author = FindUser(request.UserId);
        if (author == null)
        {
            return BadRequest($"User with id {request.UserId} not found");
        }

        Post created = await postRepository.AddAsync(new Post
        {
            Title = request.Title,
            Body = request.Body,
            UserId = request.UserId,
        });
        
        PostDto dto = ToDto(created, author.UserName);
        return Created($"/posts/{dto.Id}", dto);
    }

    [HttpPut("{postId:int}")]
    public async Task<ActionResult> UpdatePost([FromRoute] int postId,
        [FromBody] UpdatePost request)
    {
        Post? post = FindPost(postId);
        if (post is null)
        {
            return NotFound($"Post with id {postId} not found");
        }

        if (string.IsNullOrEmpty(request.Title) ||
            string.IsNullOrEmpty(request.Body))
        {
            return BadRequest("Title and Body are required");
        }
        
        post.Title = request.Title;
        post.Body = request.Body;
        await postRepository.UpdateAsync(post);
        return NoContent();
    }

    [HttpGet("{postId:int}")]
    public ActionResult<PostDto> GetPost([FromRoute] int postId,
        [FromQuery] bool includeComments = false)
    {
        Post? post = FindPost(postId);
        if (post is null)
        {
            return NotFound($"Post with id {postId} not found");
        }

        Dictionary<int, string> userNames = UserNames();
        PostDto dto = ToDto(post, NameOf(userNames, post.UserId));

        if (includeComments)
        {
            dto.Comments = CommentsOfPost(postId, userNames);
        }

        return dto;
    }

    [HttpGet]
    public ActionResult<IEnumerable<PostDto>> GetPosts(
        [FromQuery] string? titleContains, [FromQuery] int? userId,
        [FromQuery] string? userName)
    {
        Dictionary<int, string> userNames = UserNames();
        IQueryable<Post> posts = postRepository.GetMany();

        if (!string.IsNullOrEmpty(titleContains))
        {
            posts = posts.Where(p => p.Title.Contains(titleContains, StringComparison.OrdinalIgnoreCase));
        }

        if (userId is not null)
        {
            posts = posts.Where(p => p.UserId == userId);
        }

        if (!string.IsNullOrEmpty(userName))
        {
            List<int> matchingUserIds = userNames
                .Where(value => value.Value.Contains(userName, StringComparison.OrdinalIgnoreCase))
                .Select(value => value.Key).ToList();
            posts = posts.Where(p => matchingUserIds.Contains(p.UserId));
        }

        return Ok(posts.ToList().Select(p => ToDto(p, NameOf(userNames, p.UserId))));
    }

    [HttpGet("{postId:int}/comments")]
    public ActionResult<IEnumerable<CommentDto>> GetCommentsOfPost(
        [FromRoute] int postId)
    {
        if (FindPost(postId) is null)
        {
            return NotFound($"Post with id {postId} not found");
        }

        return Ok(CommentsOfPost(postId, UserNames()));
    }

    [HttpDelete("{postId:int}")]
    public async Task<ActionResult> DeletePost([FromRoute] int postId)
    {
        if (FindPost(postId) is null)
        {
            return NotFound($"Post with id {postId} not found");
        }
        
        List<int> commentIds = commentRepository.GetMany()
            .Where(c => c.PostId == postId)
            .Select(c => c.Id)
            .ToList();

        foreach (int commentId in commentIds)
        {
            await commentRepository.DeleteAsync(commentId);
        }

        await postRepository.DeleteAsync(postId);
        return NoContent();
    }
    
    private Post? FindPost(int postId) => postRepository.GetMany().SingleOrDefault(p => p.Id == postId);
    private User? FindUser(int userId) => userRepository.GetMany().SingleOrDefault(u => u.Id == userId);
   
    //id-to-name list
    private Dictionary<int, string> UserNames() => userRepository.GetMany().ToDictionary(u => u.Id, u => u.UserName);
    private static String NameOf(Dictionary<int, string> names, int userId) =>
    names.TryGetValue(userId, out string? name) ? name : "Unknown";
    
    private List<CommentDto> CommentsOfPost(int postId, Dictionary<int, string> userNames) =>
    commentRepository.GetMany()
        .Where(c => c.PostId == postId)
        .ToList()
        .Select(c => new CommentDto
        {
            Id = c.Id,
            Body = c.Body,
            UserId = c.UserId,
            UserName = NameOf(userNames, c.UserId),
            PostId = c.PostId
        }).ToList();
    private static PostDto ToDto(Post post, string userName) => new()
   {
       Id = post.Id,
       Title = post.Title,
       Body = post.Body,
       UserId = post.UserId,
       UserName = userName
   };
}