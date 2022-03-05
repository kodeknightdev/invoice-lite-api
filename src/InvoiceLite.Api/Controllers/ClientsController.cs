using InvoiceLite.Api.Data;
using InvoiceLite.Api.Dtos;
using InvoiceLite.Api.Extensions;
using InvoiceLite.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoiceLite.Api.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ClientsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IEnumerable<ClientDto>> GetAll()
    {
        var userId = User.GetUserId();
        return await _db.Clients
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .Select(c => new ClientDto(c.Id, c.Name, c.Email, c.Company, c.Address, c.CreatedAt))
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientDto>> Get(int id)
    {
        var client = await FindClient(id);
        if (client == null) return NotFound();
        return client.ToDto();
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create(SaveClientRequest request)
    {
        var client = new Client { UserId = User.GetUserId() };
        Apply(client, request);
        _db.Clients.Add(client);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = client.Id }, client.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ClientDto>> Update(int id, SaveClientRequest request)
    {
        var client = await FindClient(id);
        if (client == null) return NotFound();

        Apply(client, request);
        await _db.SaveChangesAsync();
        return client.ToDto();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await FindClient(id);
        if (client == null) return NotFound();

        if (await _db.Invoices.AnyAsync(i => i.ClientId == id))
            return Conflict(new { message = "This client has invoices and cannot be deleted." });

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private Task<Client?> FindClient(int id)
    {
        var userId = User.GetUserId();
        return _db.Clients.SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    private static void Apply(Client client, SaveClientRequest request)
    {
        client.Name = request.Name.Trim();
        client.Email = request.Email.Trim();
        client.Company = string.IsNullOrWhiteSpace(request.Company) ? null : request.Company.Trim();
        client.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
    }
}
