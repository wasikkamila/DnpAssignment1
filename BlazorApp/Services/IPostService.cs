using ApiContracts;

namespace BlazorApp.Services;

public interface IPostService
{
    public Task<PostDto> AddPostAsync(CreatePostDto request);
    public Task UpdatePostAsync(int id, UpdatePost request);
    public Task<PostDto> GetPostAsync(int id, bool includeComments);
    public Task<List<PostDto>> GetPostsAsync(string? titleContains = null, string? userName = null);
    public Task DeletePostAsync(int id);
}