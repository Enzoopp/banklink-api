namespace BankLink.Api.Dtos;

public record ExternalTransferResponse(
    bool Success,
    string TransactionId,
    decimal NewBalance,
    string? Message
);