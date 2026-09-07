using Entities;
using RepositoryContracts;

namespace CLI.UI.ManagePosts;

public class ListPostsView
{
    private readonly IPostRepository postRepository;

    public ListPostsView(IPostRepository postRepository)
    {
        this.postRepository = postRepository;
    }

    public void ListPosts()
    {
        Console.WriteLine("------POSTS-------");
        var posts = postRepository.GetMany();
        if (!posts.Any())
        {
            Console.WriteLine("No posts found");
            return;
        }
        foreach (var post in posts)
            Console.WriteLine($"{post.Id}: {post.Title}");
    }
}