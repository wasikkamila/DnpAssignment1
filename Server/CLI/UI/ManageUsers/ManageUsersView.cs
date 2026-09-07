using RepositoryContracts;

namespace CLI.ManageUsers;

public class ManageUsersView
{
    private readonly CreateUserView _createUserView;
    private readonly ListUsersView _listUsersView;

    public ManageUsersView(IUserRepository userRepository)
    {
        _createUserView = new CreateUserView(userRepository);
        _listUsersView = new ListUsersView(userRepository);
    }

    public async Task ShowMenuAsync()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("--- Manage Users ---");
            Console.WriteLine("1. Create new user");
            Console.WriteLine("2. See all users");
            Console.WriteLine("0. Back");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine())
            {
                case "1": await _createUserView.CreateUserAsync(); break;
                case "2": _listUsersView.ShowUsers(); break;
                case "0": back = true; break;
                default: Console.WriteLine("Invalid option, try again."); break;
            }
        }
    }
}