namespace Entities;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public List<Post> Posts { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();

    private User() { }  // for EFC

    public User(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }
}