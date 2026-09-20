using System.Text.Json;
using Entities;
using RepositoryContracts;
namespace FileRepositories;

public class CommentFileRepository : ICommentRepository
{
    private readonly string filePath = "comments.json";
    private static readonly JsonSerializerOptions jsonOptions = new() {WriteIndented = true};

    public CommentFileRepository()
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        List<Comment> comments = await LoadCommentsAsync();
        comment.Id = comments.Count > 0 ? comments.Max(x => x.Id) + 1 : 1;
        comments.Add(comment);
        await SaveCommentsAsync(comments);
        return comment;
    }

    public async Task UpdateAsync(Comment comment)
    {
        List<Comment> comments = await LoadCommentsAsync();
        int index = comments.FindIndex(x => x.Id == comment.Id);
        if (index == -1)
        {
            throw new InvalidOperationException(
                $"Comment with ID:  {comment.Id} was not found");
        }
        comments[index] = comment;
        await SaveCommentsAsync(comments);
    }

    public async Task DeleteAsync(int id)
    {
        List<Comment> comments = await LoadCommentsAsync();
        Comment? commentToRemove = comments.SingleOrDefault(x => x.Id == id);
        if (commentToRemove is null)
        {
            throw new InvalidOperationException(
                $"Comment with ID: {id} not found");
        }

        comments.Remove(commentToRemove);
        await SaveCommentsAsync(comments);
    }

    public async Task<Comment> GetSingleAsync(int id)
    {
        List<Comment> comments = await LoadCommentsAsync();
        Comment? comment = comments.SingleOrDefault(x => x.Id == id);
        if (comment is null)
        {
            throw new InvalidOperationException(
                $"Comment whit ID: {id} was not found");
        }

        return comment;
    }

    public IQueryable<Comment> GetMany()
    {
        return LoadCommentsAsync().Result.AsQueryable();
    }

    private async Task<List<Comment>> LoadCommentsAsync()
    {
        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<Comment>>(json) ?? new List<Comment>();
    }

    private async Task SaveCommentsAsync(List<Comment> comments)
    {
        string json = JsonSerializer.Serialize(comments, jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
}