using System.ComponentModel.DataAnnotations;

namespace BankLink.Api.Dtos;

public record TransferSendDto(
    [Required] int OriginAccountId,
    [Required, StringLength(30)] string DestinationBankCode,
    [Required, StringLength(30)] string DestinationAccountNumber,
    [Range(0.01, double.MaxValue)] decimal Amount,
    string? Concept,
    [Required, StringLength(80)] string IdempotencyKey
);

public record TransferReceiveDto(
    [Required, StringLength(30)] string OriginBankCode,
    [Required, StringLength(30)] string OriginAccountNumber,
    [Required, StringLength(30)] string DestinationAccountNumber,
    [Range(0.01, double.MaxValue)] decimal Amount,
    string? Concept,
    [Required, StringLength(80)] string IdempotencyKey
);

public record TransferResultDto(decimal Balance, int MovementId);
