using InvoiceLite.Api.Models;

namespace InvoiceLite.Api.Dtos;

public record ClientDto(int Id, string Name, string Email, string? Company, string? Address, DateTime CreatedAt);

public record SaveClientRequest(string Name, string Email, string? Company, string? Address);

public static class ClientMappings
{
    public static ClientDto ToDto(this Client c) =>
        new(c.Id, c.Name, c.Email, c.Company, c.Address, c.CreatedAt);
}
