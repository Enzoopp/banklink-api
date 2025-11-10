namespace BankLink.Api.Dtos;

public record ExternalTransferRequest(
    string OriginBankCode,
    string OriginAccountNumber,
    string DestinationAccountNumber,
    decimal Amount,
    string? Concept,
    string IdempotencyKey
);