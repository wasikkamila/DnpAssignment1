namespace CLI.UI.ManagePosts;
using RepositoryContracts;
using Entities;

public class CreatePostView
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;

    public CreatePostView(IPostRepository postRepository, IUserRepository userRepository)
    {
        this.postRepository = postRepository;
        this.userRepository = userRepository;
    }
    
    

    public async Task CreatePostAsync()
    {
        Console.WriteLine("CREATE NEW POST:");
        Console.WriteLine("Title: ");
        string title = Console.ReadLine()??"";

        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("Title cannot be empty");
            return;
        }
        Console.WriteLine();
        Console.WriteLine("Body: ");
        string body = Console.ReadLine()??"";
        Console.WriteLine("User ID: ");
        int userId = int.Parse(Console.ReadLine());

        if (!userRepository.GetMany().Any(u => u.Id == userId))
        {
            Console.WriteLine($"No user found with id {userId}");
            return;
        }
        
        Post pCreated = await postRepository.AddAsync(new Post {Title = title, Body = body, UserId = userId});

        Console.WriteLine($"New post: '{pCreated.Title}' created with id: {pCreated.Id}");
    }
    
    
}