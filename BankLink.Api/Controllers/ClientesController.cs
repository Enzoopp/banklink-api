using BankLink.Api.Data;
using BankLink.Api.Domain;
using BankLink.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly BankLinkDbContext _db;
    public ClientesController(BankLinkDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientResponseDto>>> GetAll()
    {
        var list = await _db.Clients
            .Select(c => new ClientResponseDto(c.Id, c.Nombre, c.Apellido, c.Dni, c.Direccion, c.Telefono, c.Email))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClientResponseDto>> GetById(int id)
    {
        var c = await _db.Clients.FindAsync(id);
        if (c is null) return NotFound();
        return Ok(new ClientResponseDto(c.Id, c.Nombre, c.Apellido, c.Dni, c.Direccion, c.Telefono, c.Email));
    }

    [HttpPost]
    public async Task<ActionResult<ClientResponseDto>> Create([FromBody] ClientCreateDto dto)
    {
        if (await _db.Clients.AnyAsync(x => x.Dni == dto.Dni))
            return Conflict(new { message = "DNI ya registrado" });

        var c = new Client
        {
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            Dni = dto.Dni.Trim(),
            Direccion = dto.Direccion?.Trim(),
            Telefono = dto.Telefono?.Trim(),
            Email = dto.Email.Trim()
        };
        _db.Clients.Add(c);
        await _db.SaveChangesAsync();

        var res = new ClientResponseDto(c.Id, c.Nombre, c.Apellido, c.Dni, c.Direccion, c.Telefono, c.Email);
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, res);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ClientUpdateDto dto)
    {
        var c = await _db.Clients.FindAsync(id);
        if (c is null) return NotFound();

        c.Nombre = dto.Nombre.Trim();
        c.Apellido = dto.Apellido.Trim();
        c.Direccion = dto.Direccion?.Trim();
        c.Telefono = dto.Telefono?.Trim();
        c.Email = dto.Email.Trim();

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _db.Clients.Include(x => x.Accounts).FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return NotFound();
        if (c.Accounts.Any())
            return BadRequest(new { message = "No se puede eliminar: tiene cuentas asociadas" });

        _db.Clients.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
