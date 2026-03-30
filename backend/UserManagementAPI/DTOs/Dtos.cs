namespace UserManagementAPI.DTOs;

public record CreateUserDto(string Username, string Password);

public record LoginDto(string Username, string Password, string Application);

public record AssignApplicationDto(int ApplicationId);

public record UserResponseDto(
    int Id,
    string Username,
    bool IsActive,
    DateTime CreatedAt,
    List<string> Applications
);

public record ApplicationResponseDto(int Id, string Name, string Description);

public record LoginResponseDto(string Token, string Username, List<string> Applications);
