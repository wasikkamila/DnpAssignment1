using Entities;
using RepositoryContracts;
using InMemoryRepositories;
using CLI;

IUserRepository userRepository = new UserInMemoryRepository();
IPostRepository postRepository = new PostInMemoryRepository();
ICommentRepository commentRepository = new CommentInMemoryRepository();

CliApp app = new CliApp(userRepository, postRepository, commentRepository);
await app.RunAsync();