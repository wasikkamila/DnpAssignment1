namespace ApiContracts;

public class CommentDto
{
    public required int Id { get; set; }
    public required string Body { get; set; }
    public required int UserId { get; set; }
    public required string UserName { get; set; }
    public required int PostId { get; set; }
}