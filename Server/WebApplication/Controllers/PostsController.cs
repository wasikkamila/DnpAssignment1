using ApiContracts;
using Entities;
using RepositoryContracts;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers;


[ApiController]
[Route("[controller]")]
public class PostsController : ControllerBase
{
   [HttpPost]
   public async Task<ActionResult<PostDto>> AddPost([FromBody] CreatePostDto request)

   private static PostDto ToDto(Post post, string userName) => new()
   {
       Id = post.Id,
       Title = post.Title,
       Body = post.Body,
       UserId = post.UserId,
       UserName = userName
   };
}