namespace ResQueue.Dtos;

public record UpdateUserDto(
    string? FullName,
    UserSettingsDto Settings = null!
);