using RepositoryContracts;

namespace CLI.ManageUsers;

public class ListUsersView
{
    private readonly IUserRepository _userRepository;

    public ListUsersView(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public void ShowUsers()
    {
        Console.WriteLine("--- All Users ---");
        var users = _userRepository.GetMany();

        if (!users.Any())
        {
            Console.WriteLine("No users found.");
            return;
        }

        foreach (var user in users)
            Console.WriteLine($"[{user.Id}] {user.UserName}");
    }
}