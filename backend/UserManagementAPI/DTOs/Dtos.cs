namespace UserManagementAPI.DTOs;

public record CreateUserDto(string Username, string Password);

public record RegisterAdminDto(string Username, string Password, string SecretKey);

public record LoginDto(string Username, string Password, string Application);

public record AssignApplicationDto(int ApplicationId);

public record UserResponseDto(
    int Id,
    string Username,
    bool IsActive,
    string Role,
    DateTime CreatedAt,
    List<string> Applications
);

public record ApplicationResponseDto(int Id, string Name, string Description);

public record LoginResponseDto(string Token, string Username, string Role, List<string> Applications);