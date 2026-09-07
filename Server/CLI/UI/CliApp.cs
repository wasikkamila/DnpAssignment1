using RepositoryContracts;
using CLI.ManageUsers;
using CLI.ManagePosts;

namespace CLI;

public class CliApp
{
    private readonly ManageUsersView _manageUsersView;
    private readonly ManagePostsView _managePostsView;

    public CliApp(IUserRepository userRepository, IPostRepository postRepository, ICommentRepository commentRepository)
    {
        _manageUsersView = new ManageUsersView(userRepository);
        _managePostsView = new ManagePostsView(postRepository, commentRepository);
    }

    public async Task RunAsync()
    {
        bool running = true;
        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("=== Forum CLI ===");
            Console.WriteLine("1. Manage users");
            Console.WriteLine("2. Manage posts");
            Console.WriteLine("0. Exit");
            Console.Write("Choose an option: ");

            switch (Console.ReadLine())
            {
                case "1": await _manageUsersView.ShowMenuAsync(); break;
                case "2": await _managePostsView.ShowMenuAsync(); break;
                case "0": running = false; break;
                default: Console.WriteLine("Invalid option, try again."); break;
            }
        }
    }
}