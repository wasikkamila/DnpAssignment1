using Entities;
using RepositoryContracts;

namespace CLI.ManageUsers;

public class CreateUserView
{
    private readonly IUserRepository _userRepository;

    public CreateUserView(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task CreateUserAsync()
    {
        Console.WriteLine("--- Create New User ---");
        Console.Write("Username: ");
        string userName = Console.ReadLine() ?? "";
        Console.Write("Password: ");
        string password = Console.ReadLine() ?? "";

        User created = await _userRepository.AddAsync(new User
        {
            UserName = userName,
            Password = password
        });

        Console.WriteLine($"User '{created.UserName}' created with ID {created.Id}.");
    }
}