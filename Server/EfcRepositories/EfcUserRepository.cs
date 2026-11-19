using RepositoryContracts;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;


namespace EfcRepositories;

public class EfcUserRepository : IUserRepository
{
    private readonly ForumContext ctx;

    public EfcUserRepository(ForumContext ctx)
    {
        this.ctx = ctx;
    }

    public async Task<User> AddAsync(User user)
    {
        EntityEntry<User> entry = await ctx.Users.AddAsync(user);
        await ctx.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task UpdateAsync(User user)
    {
        if (!await ctx.Users.AnyAsync(x => x.Id == user.Id))
        {
            throw new InvalidOperationException($"User with ID '{user.Id}' not found");
        }

        ctx.Users.Update(user);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        User? existing = await ctx.Users.SingleOrDefaultAsync(x => x.Id == id);
        if (existing is null)
        {
            throw new InvalidOperationException($"User with ID '{id}' not found");
        }

        ctx.Users.Remove(existing);
        await ctx.SaveChangesAsync();
    }

    public async Task<User> GetSingleAsync(int id)
    {
        // AsNoTracking: EF doesn't remember this object after loading it.
        // Without it, calling UpdateAsync later with the same Id would crash,
        // because EF would think there are two different objects with that Id.
        User? user = await ctx.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (user is null)
        {
            throw new InvalidOperationException($"User with ID '{id}' not found");
        }

        return user;
    }

    public IQueryable<User> GetMany()
    {
        return ctx.Users.AsQueryable();
    }
}