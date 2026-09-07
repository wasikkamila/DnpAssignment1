
using RepositoryContracts;
using Entities;

namespace CLI.UI.ManagePosts;

public class SinglePostView
{
    private readonly IPostRepository postRepository;
    private readonly IUserRepository userRepository;
    private readonly ICommentRepository commentRepository;

    public SinglePostView(IPostRepository postRepository, ICommentRepository commentRepository, IUserRepository userRepository)
    {
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
        this.userRepository = userRepository;
    }

    public async Task ShowSingleAsync()
    {
        Console.WriteLine("------VIEW POST-----");
        Console.WriteLine("Post ID:");
        int postId = int.Parse(Console.ReadLine());

        Post post;
        try
        {
            post = await postRepository.GetSingleAsync(postId);
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"No post {postId} has been found");
            return;
        }

        PrintPost(post);
        Console.WriteLine("Do you want to add a comment? (y/n)");
        if ((Console.ReadLine() ?? "").Trim().ToLower() == "y")
        {
            await AddCommentAsync(postId);
        }
    }

    private void PrintPost(Post post)
    {
        Console.WriteLine();
        Console.WriteLine($"Title: {post.Title}");
        Console.WriteLine($"Body: {post.Body}");
        Console.WriteLine($"Posted by User ID: {post.UserId} ");
        Console.WriteLine("Comments:");

        var comments = commentRepository.GetMany().Where(c => c.PostId == post.Id).ToList();
        if (!comments.Any())
        {
            Console.WriteLine("No comments yet");
        }
        else
        {
            foreach (var comment in comments)
                Console.WriteLine($"User ID: {comment.UserId}:  {comment.Body}");
        }
    }
    private async Task AddCommentAsync(int postId)
    {
        Console.WriteLine("User ID: ");
        int userId = int.Parse(Console.ReadLine());

        if (!userRepository.GetMany().Any(u => u.Id == userId))
        {
            Console.WriteLine($"No user {userId} has been found");
            return;
        }

        Console.WriteLine("add comment:");
        string body = Console.ReadLine()??"";
        
        Comment cCreated = await commentRepository.AddAsync(new Comment{Body = body, UserId = userId});
        Console.WriteLine($"Comment added with ID: {cCreated.Id}");
    }
}