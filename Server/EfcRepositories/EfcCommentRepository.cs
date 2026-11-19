using RepositoryContracts;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EfcRepositories;

public class EfcCommentRepository : ICommentRepository
{
    private readonly ForumContext ctx;

    public EfcCommentRepository(ForumContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        EntityEntry<Comment> entry = await ctx.Comments.AddAsync(comment);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task UpdateAsync(Comment comment)
    {
        if (!await ctx.Comments.AnyAsync(x => x.Id == comment.Id))
        {
            throw new InvalidOperationException($"Comment with ID '{comment.Id}' not found");
        }

        ctx.Comments.Update(comment);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Comment? existing = await ctx.Comments.SingleOrDefaultAsync(x => x.Id == id);
        if (existing is null)
        {
            throw new InvalidOperationException($"Comment with ID '{id}' not found");
        }

        ctx.Comments.Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<Comment> GetSingleAsync(int id)
    {
        // AsNoTracking: EF doesn't remember this object after loading it.
        // Without it, calling UpdateAsync later with the same Id would crash,
        // because EF would think there are two different objects with that Id.
        Comment? comment = await ctx.Comments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (comment is null)
        {
            throw new InvalidOperationException($"Comment with ID '{id}' not found");
        }

        return comment;
    }

    public IQueryable<Comment> GetMany()
    {
        return ctx.Comments.AsQueryable();
    }

}