using ApiContracts;
using Entities;
using RepositoryContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        
        User? author = await FindUserAsync(request.UserId);
        if (author == null)
        {
            return BadRequest($"User with id {request.UserId} not found");
        }

        Post created = await postRepository.AddAsync(new Post(request.Title, request.Body, request.UserId)
        {
            Title = request.Title,
            Body = request.Body,
            UserId = request.UserId,
        });
        
        PostDto dto = new()
        {
            Id = created.Id,
            Title = created.Title,
            Body = created.Body,
            UserId = created.UserId,
            UserName = author.UserName
        };
        
        return Created($"/posts/{dto.Id}", dto);
    }

    [HttpPut("{postId:int}")]
    public async Task<ActionResult> UpdatePost([FromRoute] int postId,
        [FromBody] UpdatePost request)
    {
        Post? post = await FindPostAsync(postId);
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
    public async Task<ActionResult<PostDto>> GetPost([FromRoute] int postId,
        [FromQuery] bool includeAuthor = false,
        [FromQuery] bool includeComments = false)
    {
        IQueryable<Post> queryForPost = postRepository.GetMany()
            .Where(p => p.Id == postId);

        if (includeAuthor)
        {
            queryForPost = queryForPost.Include(p => p.User);
        }

        if (includeComments)
        {
            queryForPost = queryForPost.Include(p => p.Comments);
        }

        PostDto? dto = await queryForPost.Select(post => new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Body = post.Body,
            UserId = post.UserId,
            UserName = post.User.UserName,
            Author = includeAuthor
                ? new UserDto
                    { Id = post.User.Id, UserName = post.User.UserName }
                : null,
            Comments = includeComments
                ? post.Comments.Select(c => new CommentDto()
                {
                    Id = c.Id,
                    Body = c.Body,
                    UserId = c.UserId,
                    UserName = c.User.UserName,
                    PostId = c.PostId
                }).ToList()
                : new List<CommentDto>()
        }).FirstOrDefaultAsync();

        if (dto is null)
        {
            return NotFound($"Post with id {postId} not found");
        }

        return dto;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts(
        [FromQuery] string? titleContains, [FromQuery] int? userId,
        [FromQuery] string? userName)
    {
        IQueryable<Post> posts = postRepository.GetMany();

        if (!string.IsNullOrEmpty(titleContains))
        {
            string lowerTitle = titleContains.ToLower();
            posts = posts.Where(p => p.Title.ToLower().Contains(lowerTitle));
        }

        if (userId is not null)
        {
            posts = posts.Where(p => p.UserId == userId);
        }

        if (!string.IsNullOrEmpty(userName))
        {
            string lowerName = userName.ToLower();
            posts = posts.Where(p => p.User.UserName.ToLower().Contains(lowerName));
        }

        List<PostDto> result = await posts.Select(p => new PostDto
        {
            Id = p.Id,
            Title = p.Title,
            Body = p.Body,
            UserId = p.UserId,
            UserName = p.User.UserName
        }).ToListAsync();

        return Ok(result);
    }

    [HttpGet("{postId:int}/comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsOfPost(
        [FromRoute] int postId)
    {
        if (await FindPostAsync(postId) is null)
        {
            return NotFound($"Post with id {postId} not found");
        }
        
        List<CommentDto> comments = await commentRepository.GetMany()
            .Where(c => c.PostId == postId)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                Body = c.Body,
                UserId = c.UserId,
                UserName = c.User.UserName,
                PostId = c.PostId
            }).ToListAsync();

        return Ok(comments);
    }

    [HttpDelete("{postId:int}")]
    public async Task<ActionResult> DeletePost([FromRoute] int postId)
    {
        if (await FindPostAsync(postId) is null)
        {
            return NotFound($"Post with id {postId} not found");
        }
        
        List<int> commentIds = await commentRepository.GetMany()
            .Where(c => c.PostId == postId)
            .Select(c => c.Id)
            .ToListAsync();

        foreach (int commentId in commentIds)
        {
            await commentRepository.DeleteAsync(commentId);
        }

        await postRepository.DeleteAsync(postId);
        return NoContent();
    }
    
    private Task<Post?> FindPostAsync(int postId) => postRepository.GetMany().SingleOrDefaultAsync(p => p.Id == postId);
    private Task<User?> FindUserAsync(int userId) => userRepository.GetMany().SingleOrDefaultAsync(u => u.Id == userId);
    
}