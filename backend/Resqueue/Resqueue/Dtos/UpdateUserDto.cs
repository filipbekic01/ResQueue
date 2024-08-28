namespace Resqueue.Dtos;

public record UpdateUserDto(
    string? FullName,
    UserSettingsDto Settings = null!
);