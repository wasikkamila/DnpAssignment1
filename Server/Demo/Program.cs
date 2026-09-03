using Entities;
using InMemoryRepositories;
using RepositoryContracts;

Console.WriteLine("USERS:");
IUserRepository userRepository = new UserInMemoryRepository();