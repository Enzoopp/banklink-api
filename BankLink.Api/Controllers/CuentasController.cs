using BankLink.Api.Data;
using BankLink.Api.Domain;
using BankLink.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CuentasController : ControllerBase
{
    private readonly BankLinkDbContext _db;
    public CuentasController(BankLinkDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AccountResponseDto>>> GetAll()
    {
        var list = await _db.Accounts
            .Select(a => new AccountResponseDto(a.Id, a.AccountNumber, a.Type, a.Balance, a.ClientId, a.Status))
            .ToListAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountResponseDto>> GetById(int id)
    {
        var a = await _db.Accounts.FindAsync(id);
        if (a is null) return NotFound();
        return Ok(new AccountResponseDto(a.Id, a.AccountNumber, a.Type, a.Balance, a.ClientId, a.Status));
    }

    [HttpPost]
    public async Task<ActionResult<AccountResponseDto>> Create([FromBody] AccountCreateDto dto)
    {
        if (!await _db.Clients.AnyAsync(c => c.Id == dto.ClientId))
            return BadRequest(new { message = "Cliente no existe" });

        if (await _db.Accounts.AnyAsync(a => a.AccountNumber == dto.AccountNumber))
            return Conflict(new { message = "Número de cuenta ya existe" });

        var a = new Account
        {
            AccountNumber = dto.AccountNumber.Trim(),
            Type = dto.Type,
            ClientId = dto.ClientId,
            Balance = 0m,
            Status = AccountStatus.Activa
        };

        await using var tx = await _db.Database.BeginTransactionAsync();
        _db.Accounts.Add(a);
        await _db.SaveChangesAsync();

        if ((dto.InitialBalance ?? 0m) > 0m)
        {
            a.Balance += dto.InitialBalance!.Value;
            _db.Movements.Add(new Movement
            {
                AccountId = a.Id,
                Type = MovementType.Deposito,
                Amount = dto.InitialBalance!.Value,
                Description = "Depósito inicial"
            });
            await _db.SaveChangesAsync();
        }
        await tx.CommitAsync();

        var res = new AccountResponseDto(a.Id, a.AccountNumber, a.Type, a.Balance, a.ClientId, a.Status);
        return CreatedAtAction(nameof(GetById), new { id = a.Id }, res);
    }

    [HttpPut("{id:int}/estado")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] AccountUpdateStatusDto dto)
    {
        var a = await _db.Accounts.FindAsync(id);
        if (a is null) return NotFound();
        a.Status = dto.Status;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:int}/movimientos")]
    public async Task<IActionResult> GetMovimientos(int id)
    {
        var exists = await _db.Accounts.AnyAsync(x => x.Id == id);
        if (!exists) return NotFound();

        var movs = await _db.Movements
            .Where(m => m.AccountId == id)
            .OrderByDescending(m => m.OccurredAt)
            .Select(m => new {
                m.Id, m.Type, m.Amount, m.OccurredAt, m.Description, m.ExternalReference
            }).ToListAsync();

        return Ok(movs);
    }

    [HttpPost("{id:int}/depositos")]
    public async Task<IActionResult> Deposito(int id, [FromBody] MoneyOperationDto dto)
    {
        var a = await _db.Accounts.FindAsync(id);
        if (a is null) return NotFound();
        if (a.Status != AccountStatus.Activa) return BadRequest(new { message = "Cuenta inactiva" });

        await using var tx = await _db.Database.BeginTransactionAsync();
        a.Balance += dto.Monto;
        _db.Movements.Add(new Movement {
            AccountId = id, Type = MovementType.Deposito, Amount = dto.Monto, Description = dto.Descripcion
        });
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return CreatedAtAction(nameof(GetMovimientos), new { id }, new { saldo = a.Balance });
    }

    [HttpPost("{id:int}/retiros")]
    public async Task<IActionResult> Retiro(int id, [FromBody] MoneyOperationDto dto)
    {
        var a = await _db.Accounts.FindAsync(id);
        if (a is null) return NotFound();
        if (a.Status != AccountStatus.Activa) return BadRequest(new { message = "Cuenta inactiva" });
        if (a.Balance < dto.Monto) return UnprocessableEntity(new { message = "Saldo insuficiente" });

        await using var tx = await _db.Database.BeginTransactionAsync();
        a.Balance -= dto.Monto;
        _db.Movements.Add(new Movement {
            AccountId = id, Type = MovementType.Retiro, Amount = dto.Monto, Description = dto.Descripcion
        });
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return CreatedAtAction(nameof(GetMovimientos), new { id }, new { saldo = a.Balance });
    }
}
