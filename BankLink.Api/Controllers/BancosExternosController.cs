using BankLink.Api.Data;
using BankLink.Api.Domain;
using BankLink.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BancosExternosController : ControllerBase
{
    private readonly BankLinkDbContext _db;
    public BancosExternosController(BankLinkDbContext db) => _db = db;

    private static string BuildUrl(string baseUrl, string endpoint)
        => $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

    // GET: api/BancosExternos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExternalBankResponseDto>>> GetAll()
    {
        var list = await _db.ExternalBanks
            .Select(b => new ExternalBankResponseDto(
                b.Id, 
                b.Name, 
                b.Code, 
                b.BaseUrl, 
                b.TransferEndpoint,
                b.ValidationEndpoint,
                b.IsActive,
                b.CreatedAt,
                BuildUrl(b.BaseUrl, b.TransferEndpoint)))
            .ToListAsync();

        return Ok(list);
    }

    // GET: api/BancosExternos/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExternalBankResponseDto>> GetById(int id)
    {
        var b = await _db.ExternalBanks.FindAsync(id);
        if (b is null) return NotFound();

        var res = new ExternalBankResponseDto(
            b.Id, 
            b.Name, 
            b.Code, 
            b.BaseUrl, 
            b.TransferEndpoint,
            b.ValidationEndpoint,
            b.IsActive,
            b.CreatedAt,
            BuildUrl(b.BaseUrl, b.TransferEndpoint));

        return Ok(res);
    }

    // POST: api/BancosExternos
    [HttpPost]
    public async Task<ActionResult<ExternalBankResponseDto>> Create([FromBody] ExternalBankCreateDto dto)
    {
        var normalizedCode = dto.Code.Trim().ToUpperInvariant();
        
        if (await _db.ExternalBanks.AnyAsync(x => x.Code == normalizedCode))
            return Conflict(new { message = "Código de banco ya existe" });

        var bank = new ExternalBank
        {
            Name = dto.Name.Trim(),
            Code = normalizedCode,
            BaseUrl = dto.BaseUrl.Trim(),
            TransferEndpoint = dto.TransferEndpoint.Trim(),
            ValidationEndpoint = dto.ValidationEndpoint?.Trim(),
            ApiKey = dto.ApiKey.Trim()
        };

        _db.ExternalBanks.Add(bank);
        await _db.SaveChangesAsync();

        var res = new ExternalBankResponseDto(
            bank.Id, 
            bank.Name, 
            bank.Code, 
            bank.BaseUrl, 
            bank.TransferEndpoint,
            bank.ValidationEndpoint,
            bank.IsActive,
            bank.CreatedAt,
            BuildUrl(bank.BaseUrl, bank.TransferEndpoint));

        return CreatedAtAction(nameof(GetById), new { id = bank.Id }, res);
    }

    // PUT: api/BancosExternos/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ExternalBankUpdateDto dto)
    {
        var bank = await _db.ExternalBanks.FindAsync(id);
        if (bank is null) return NotFound();

        var normalizedCode = dto.Code.Trim().ToUpperInvariant();

        if (!string.Equals(bank.Code, normalizedCode, StringComparison.OrdinalIgnoreCase) &&
            await _db.ExternalBanks.AnyAsync(x => x.Code == normalizedCode))
            return Conflict(new { message = "Código de banco ya existe" });

        bank.Name = dto.Name.Trim();
        bank.Code = normalizedCode;
        bank.BaseUrl = dto.BaseUrl.Trim();
        bank.TransferEndpoint = dto.TransferEndpoint.Trim();
        bank.ValidationEndpoint = dto.ValidationEndpoint?.Trim();
        bank.ApiKey = dto.ApiKey.Trim();
        bank.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/BancosExternos/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var bank = await _db.ExternalBanks.FindAsync(id);
        if (bank is null) return NotFound();

        _db.ExternalBanks.Remove(bank);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
