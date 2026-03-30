namespace UserManagementAPI.Models;

public class UserApplication
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ApplicationId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Application Application { get; set; } = null!;
}
