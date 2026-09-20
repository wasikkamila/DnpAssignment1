using System.Text.Json;
using Entities;
using RepositoryContracts;

namespace FileRepositories;

public class UserFileRepository : IUserRepository
{
    private readonly string filePath = "users.json";
    private static readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };

    public UserFileRepository()
    {
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, "[]");
        }
    }

    public async Task<User> AddAsync(User user)
    {
        List<User> users = await LoadUsersAsync();
        user.Id = users.Count > 0 ? users.Max(x => x.Id) + 1 : 1;
        users.Add(user);
        await SaveUsersAsync(users);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        List<User> users = await LoadUsersAsync();
        int index = users.FindIndex(x => x.Id == user.Id);
        if (index == -1)
        {
            throw new InvalidOperationException(
                $"User with ID: '{user.Id}' not found");
        }

        users[index] = user;
        await SaveUsersAsync(users);
    }

    public async Task DeleteAsync(int id)
    {
        List<User> users = await LoadUsersAsync();
        User? userToRemove = users.SingleOrDefault(x => x.Id == id);
        if (userToRemove is null)
        {
            throw new InvalidOperationException(
                $"User with ID: {id} not found");
        }

        users.Remove(userToRemove);
        await SaveUsersAsync(users);
    }

    public async Task<User> GetSingleAsync(int id)
    {
        List<User> users = await LoadUsersAsync();
        User? user = users.SingleOrDefault(x => x.Id == id);
        if (user is null)
        {
            throw new InvalidOperationException(
                $"User with ID: {id} not found");
        }

        return user;
    }

    public IQueryable<User> GetMany()
    {
        return LoadUsersAsync().Result.AsQueryable();
    }

    private async Task<List<User>> LoadUsersAsync()
    {
        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
    }

    private async Task SaveUsersAsync(List<User> users)
    {
        string json = JsonSerializer.Serialize(users, jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
}