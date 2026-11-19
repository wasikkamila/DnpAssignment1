using RepositoryContracts;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EfcRepositories;

public class EfcPostRepository : IPostRepository
{
    private readonly ForumContext ctx;

    public EfcPostRepository(ForumContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<Post> AddAsync(Post post)
    {
        EntityEntry<Post> entry = await ctx.Posts.AddAsync(post);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task UpdateAsync(Post post)
    {
        if (!await ctx.Posts.AnyAsync(x => x.Id == post.Id))
        {
            throw new InvalidOperationException($"Post with ID '{post.Id}' not found");
        }

        ctx.Posts.Update(post);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        Post? existing = await ctx.Posts.SingleOrDefaultAsync(x => x.Id == id);
        if (existing is null)
        {
            throw new InvalidOperationException($"Post with ID '{id}' not found");
        }

        ctx.Posts.Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<Post> GetSingleAsync(int id)
    {
        // AsNoTracking: EF doesn't remember this object after loading it.
        // Without it, calling UpdateAsync later with the same Id would crash,
        // because EF would think there are two different objects with that Id.
        Post? post = await ctx.Posts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (post is null)
        {
            throw new InvalidOperationException($"Post with ID '{id}' not found");
        }

        return post;
    }

    public IQueryable<Post> GetMany()
    {
        return ctx.Posts.AsQueryable();
    }
}