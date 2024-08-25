namespace Resqueue.Dtos;

public record UpdateUserDto(
    string? FullName,
    UserConfigDto Config = null!
);