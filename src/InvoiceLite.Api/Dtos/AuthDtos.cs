namespace InvoiceLite.Api.Dtos;

public record RegisterRequest(string Email, string Password, string? DisplayName);

public record UserDto(int Id, string Email, string? DisplayName);
