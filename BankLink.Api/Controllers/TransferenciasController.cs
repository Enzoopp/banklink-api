using BankLink.Api.Data;
using BankLink.Api.Domain;
using BankLink.Api.Dtos;
using BankLink.Api.Services;
using BankLink.Api.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankLink.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransferenciasController : ControllerBase
{
    private readonly BankLinkDbContext _db;
    private readonly IExternalBankService _externalBankService;
    private readonly ILogger<TransferenciasController> _logger;

    public TransferenciasController(
        BankLinkDbContext db,
        IExternalBankService externalBankService,
        ILogger<TransferenciasController> logger)
    {
        _db = db;
        _externalBankService = externalBankService;
        _logger = logger;
    }

    // ============= OUTBOUND (enviar a otro banco) =============
    [HttpPost("enviar")]
    public async Task<ActionResult<TransferResultDto>> Enviar(
        [FromBody] TransferSendDto dto,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Iniciando transferencia desde cuenta {AccountId} por monto {Amount} a {BankCode}:{AccountNumber}",
            dto.OriginAccountId, dto.Amount, dto.DestinationBankCode, dto.DestinationAccountNumber);

        // Validar idempotencia
        var idemPrevio = await _db.Movements
            .Include(m => m.Account)
            .Where(m =>
                m.AccountId == dto.OriginAccountId &&
                m.IdempotencyKey == dto.IdempotencyKey &&
                m.Type == MovementType.TransferenciaEnviada)
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync(ct);

        if (idemPrevio is not null)
        {
            _logger.LogInformation(
                "Transferencia duplicada detectada con IdempotencyKey {Key}",
                dto.IdempotencyKey);
            return Ok(new TransferResultDto(idemPrevio.Account!.Balance, idemPrevio.Id));
        }

        // Validar cuenta origen
        var account = await _db.Accounts.FindAsync(new object?[] { dto.OriginAccountId }, ct);
        if (account is null || account.Status != AccountStatus.Activa)
        {
            _logger.LogWarning("Cuenta origen {AccountId} no encontrada o inactiva", dto.OriginAccountId);
            return NotFound(new { message = "Cuenta origen inexistente o inactiva" });
        }

        // Validar saldo suficiente
        if (account.Balance < dto.Amount)
        {
            _logger.LogWarning(
                "Saldo insuficiente en cuenta {AccountId}. Balance: {Balance}, Requerido: {Amount}",
                dto.OriginAccountId, account.Balance, dto.Amount);
            return UnprocessableEntity(new { message = "Saldo insuficiente" });
        }

        // Validar que el banco externo exista y esté activo
        var extBank = await _db.ExternalBanks
            .FirstOrDefaultAsync(b => b.Code == dto.DestinationBankCode && b.IsActive, ct);

        if (extBank is null)
        {
            _logger.LogWarning("Banco destino {BankCode} no registrado o inactivo", dto.DestinationBankCode);
            return NotFound(new { message = "Banco destino no registrado o inactivo" });
        }

        // Iniciar transacción
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            // 1. Debitar localmente
            account.Balance -= dto.Amount;
            
            var mov = new Movement
            {
                AccountId = account.Id,
                Type = MovementType.TransferenciaEnviada,
                Amount = dto.Amount,
                Description = string.IsNullOrWhiteSpace(dto.Concept)
                    ? "Transferencia Enviada"
                    : dto.Concept!,
                IdempotencyKey = dto.IdempotencyKey,
                ExternalRef = $"{extBank.Code}:{dto.DestinationAccountNumber}"
            };
            
            _db.Movements.Add(mov);
            await _db.SaveChangesAsync(ct);

            // 2. Preparar request para banco externo
            var externalRequest = new ExternalTransferRequest(
                OriginBankCode: "BANKLINK",
                OriginAccountNumber: account.AccountNumber,
                DestinationAccountNumber: dto.DestinationAccountNumber,
                Amount: dto.Amount,
                Concept: dto.Concept,
                IdempotencyKey: dto.IdempotencyKey
            );

            _logger.LogInformation(
                "Enviando solicitud al banco externo {BankCode}",
                extBank.Code);

            // 3. Enviar a banco externo usando el servicio
            var externalResponse = await _externalBankService.SendTransferAsync(
                extBank.Code,
                externalRequest,
                ct);

            if (!externalResponse.Success)
            {
                _logger.LogError(
                    "Banco externo {BankCode} rechazó transferencia: {Message}",
                    extBank.Code, externalResponse.Message);
                
                await tx.RollbackAsync(ct);
                return BadRequest(new { message = externalResponse.Message ?? "Banco externo rechazó la transferencia" });
            }

            // 4. Actualizar referencia externa con el ID de transacción del banco externo
            mov.ExternalRef = $"{extBank.Code}:{externalResponse.TransactionId}";
            await _db.SaveChangesAsync(ct);

            // 5. Confirmar transacción
            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "Transferencia exitosa. MovementId: {MovementId}, ExternalTxId: {ExternalTxId}, Nuevo balance: {Balance}",
                mov.Id, externalResponse.TransactionId, account.Balance);

            return Ok(new TransferResultDto(account.Balance, mov.Id));
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de red al contactar banco {BankCode}", extBank.Code);
            await tx.RollbackAsync(ct);
            return StatusCode(503, new { message = "Error de red al contactar banco externo" });
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout al contactar banco {BankCode}", extBank.Code);
            await tx.RollbackAsync(ct);
            return StatusCode(504, new { message = "Timeout al contactar banco externo" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado procesando transferencia");
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // ============= INBOUND (recibir desde otro banco) =============
    [HttpPost("recibir")]
    public async Task<ActionResult<TransferResultDto>> Recibir(
        [FromHeader(Name = "X-Api-Key")] string apiKey,
        [FromBody] TransferReceiveDto dto,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Recibiendo transferencia desde {BankCode}:{AccountNumber} por monto {Amount}",
            dto.OriginBankCode, dto.OriginAccountNumber, dto.Amount);

        // Validar banco origen + API key
        var origin = await _db.ExternalBanks
            .FirstOrDefaultAsync(b => b.Code == dto.OriginBankCode && b.IsActive, ct);
        
        if (origin is null)
        {
            _logger.LogWarning("Banco origen {BankCode} no registrado o inactivo", dto.OriginBankCode);
            return NotFound(new { message = "Banco origen no registrado o inactivo" });
        }

        if (!string.Equals(origin.ApiKey, apiKey, StringComparison.Ordinal))
        {
            _logger.LogWarning(
                "API key inválida para banco {BankCode}",
                dto.OriginBankCode);
            return Unauthorized(new { message = "API key inválida" });
        }

        // Validar cuenta destino
        var dest = await _db.Accounts
            .FirstOrDefaultAsync(a => a.AccountNumber == dto.DestinationAccountNumber, ct);
        
        if (dest is null || dest.Status != AccountStatus.Activa)
        {
            _logger.LogWarning(
                "Cuenta destino {AccountNumber} no encontrada o inactiva",
                dto.DestinationAccountNumber);
            return NotFound(new { message = "Cuenta destino inexistente o inactiva" });
        }

        // Validar idempotencia
        var idemPrevio = await _db.Movements
            .Include(m => m.Account)
            .Where(m =>
                m.AccountId == dest.Id &&
                m.IdempotencyKey == dto.IdempotencyKey &&
                m.Type == MovementType.TransferenciaRecibida)
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync(ct);

        if (idemPrevio is not null)
        {
            _logger.LogInformation(
                "Transferencia entrante duplicada detectada con IdempotencyKey {Key}",
                dto.IdempotencyKey);
            return Ok(new TransferResultDto(idemPrevio.Account!.Balance, idemPrevio.Id));
        }

        // Iniciar transacción
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            // Acreditar en cuenta destino
            dest.Balance += dto.Amount;
            
            var mov = new Movement
            {
                AccountId = dest.Id,
                Type = MovementType.TransferenciaRecibida,
                Amount = dto.Amount,
                Description = string.IsNullOrWhiteSpace(dto.Concept)
                    ? "Transferencia Recibida"
                    : dto.Concept!,
                IdempotencyKey = dto.IdempotencyKey,
                ExternalRef = $"{dto.OriginBankCode}:{dto.OriginAccountNumber}"
            };
            
            _db.Movements.Add(mov);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            _logger.LogInformation(
                "Transferencia recibida exitosamente. MovementId: {MovementId}, Nuevo balance: {Balance}",
                mov.Id, dest.Balance);

            return Created($"/api/Cuentas/{dest.Id}/movimientos",
                new TransferResultDto(dest.Balance, mov.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando transferencia entrante");
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
