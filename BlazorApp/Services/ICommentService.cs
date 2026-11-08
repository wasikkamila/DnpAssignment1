using ApiContracts;

namespace BlazorApp.Services;

public interface ICommentService
{
    public Task<CommentDto> AddCommentAsync(CreateCommentDto request);
    public Task UpdateCommentAsync(int id, UpdateCommentDto request);

    public Task<List<CommentDto>> GetCommentAsync(int? postId = null,
        int? userId = null);
    public Task DeleteCommentAsync(int id);
}