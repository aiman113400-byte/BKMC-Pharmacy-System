namespace BKMC.Pharmacy.Application.DTOs.Admin;

public class UserListDto
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int Role { get; set; }
}

public class UpdateUserDto
{
    public string Username { get; set; } = string.Empty;

    public int Role { get; set; }
}

public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}