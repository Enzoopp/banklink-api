using BankLink.Api.Dtos;

namespace BankLink.Api.Services;

public interface IExternalBankService
{
    Task<ExternalTransferResponse> SendTransferAsync(
        string bankCode,
        ExternalTransferRequest request,
        CancellationToken ct = default);
    
    Task<bool> ValidateAccountAsync(
        string bankCode,
        string accountNumber,
        CancellationToken ct = default);
}