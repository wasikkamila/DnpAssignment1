using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class ManagePostsView
{
    private readonly CreatePostView createPostView;
    private readonly ListPostsView listPostsView;
    private readonly SinglePostView singlePostView;
    public ManagePostsView(IPostRepository postRepository, ICommentRepository commentRepository, IUserRepository userRepository)
    {
        createPostView = new CreatePostView(postRepository, userRepository);
        listPostsView = new ListPostsView(postRepository);
        singlePostView = new SinglePostView(postRepository, commentRepository, userRepository);
    }

    public async Task ShowMenuAsync()
    {
        bool back = false;
        while (!back)
        {
            Console.WriteLine();
            Console.WriteLine("-------MANAGE POSTS-------");
            Console.WriteLine("1. Create post");
            Console.WriteLine("2. View posts");
            Console.WriteLine("3. View a specific post");
            Console.WriteLine("< Back");
            Console.WriteLine("Choose an option:");
        }

        switch (Console.ReadLine())
        {
            case "1": await createPostView.CreatePostAsync(); 
                break;
            case "2": listPostsView.ListPosts();
                break;
            case "3": await singlePostView.ShowSingleAsync();
                break;
            case "<": back = true;
                break;
            default: Console.WriteLine("Invalid option. Try again");
                break;
        }
    }
}