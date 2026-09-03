using Entities;
using InMemoryRepositories;
using RepositoryContracts;
using RepositoryContracts;


//users
Console.WriteLine("USERS:");
IUserRepository userRepository = new UserInMemoryRepository();
PrintUsers(userRepository.GetMany());

User addedUser = await userRepository.AddAsync(new User {UserName = "kamila", Password = "password"});
Console.WriteLine($"New user: Id: {addedUser.Id}, UserName: {addedUser.UserName}, Password: {addedUser.Password}");

User fetchedUser = await userRepository.GetSingleAsync(addedUser.Id);
fetchedUser.UserName = "kamilawasik";
await userRepository.UpdateAsync(fetchedUser);

User updatedUser = await userRepository.GetSingleAsync(addedUser.Id);
Console.WriteLine($"username updated: {updatedUser.UserName}");

await userRepository.DeleteAsync(addedUser.Id);
Console.WriteLine($"Deleted user {addedUser.Id}. Remaining users:");
PrintUsers(userRepository.GetMany());

//posts
Console.WriteLine();
Console.WriteLine("POSTS:");
IPostRepository postRepository = new PostInMemoryRepository();
PrintPosts(postRepository.GetMany());

Post addedPost = await postRepository.AddAsync(new Post{ Title = "New post", Body = "Body text", UserId = 1 });
Console.WriteLine($"New post: Id: {addedPost.Id}, Title: {addedPost.Title}");

Post fetchedPost = await postRepository.GetSingleAsync(addedPost.Id);
fetchedPost.Title = "updated title";
await postRepository.UpdateAsync(fetchedPost);

Post updatedPost = await postRepository.GetSingleAsync(addedPost.Id);
Console.WriteLine($"Title updated: {updatedPost.Title}");

await postRepository.DeleteAsync(addedPost.Id);
Console.WriteLine($"deleted post {addedPost.Id}. Remaining posts:");
PrintPosts(postRepository.GetMany());

//comments
Console.WriteLine();
Console.WriteLine("COMMENTS:");
ICommentRepository commentRepository = new CommentInMemoryRepository();
PrintComments(commentRepository.GetMany());

Comment addedComment = await commentRepository.AddAsync(new Comment{Body = "new comment", UserId = 1, PostId = 1});
Console.WriteLine($"new comment: Id: {addedComment.Id}, Body: {addedComment.Body}");

Comment fetchedComment = await commentRepository.GetSingleAsync(addedComment.Id);
fetchedComment.Body = "updated comment";

Comment updatedComment = await commentRepository.GetSingleAsync(addedComment.Id);
Console.WriteLine($"comment updated: {updatedComment.Body}");

await commentRepository.DeleteAsync(addedComment.Id);
Console.WriteLine($"comment deleted: {addedComment.Id}. Remaining comments:");
PrintComments(commentRepository.GetMany());

Console.WriteLine();
Console.WriteLine("trying to fetch the deleted user throws, as expected:");
try
{
    await userRepository.GetSingleAsync(addedUser.Id);
}
catch (InvalidOperationException e)
{
    Console.WriteLine($"caught:  {e.Message}");
}

Console.WriteLine();
Console.WriteLine("all repository operations completed successfully.");
return;

static void PrintUsers(IQueryable<User> users)
{
    foreach (User user in users)
    {
        Console.WriteLine($"Id: {user.Id}, UserName: {user.UserName}");
    }
}

static void PrintPosts(IQueryable<Post> posts)
{
    foreach (Post post in posts)
    {
        Console.WriteLine($"Id: {post.Id}, Title: {post.Title}, UserId: {post.UserId}");
    }
}

static void PrintComments(IQueryable<Comment> comments)
{
    foreach (Comment comment in comments)
    {
        Console.WriteLine($"Id: {comment.Id}, Body: {comment.Body}, PostId: {comment.PostId}");
    }
}