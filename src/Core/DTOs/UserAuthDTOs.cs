namespace MathStorm.Core.DTOs;

public class UserAuthRequestDto
{
    public string Username { get; set; } = string.Empty;
}

public class UserAuthResponseDto
{
    public bool IsAuthenticated { get; set; }
    public bool IsNewUser { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

public class UserProfileDto
{
    public string Username { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = true;
}