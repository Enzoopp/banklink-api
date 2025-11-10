using BankLink.Api.Data;
using BankLink.Api.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace BankLink.Api.Services;

public class ExternalBankService : IExternalBankService
{
    private readonly BankLinkDbContext _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<ExternalBankService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExternalBankService(
        BankLinkDbContext db,
        IHttpClientFactory httpFactory,
        ILogger<ExternalBankService> logger)
    {
        _db = db;
        _httpFactory = httpFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<ExternalTransferResponse> SendTransferAsync(
        string bankCode,
        ExternalTransferRequest request,
        CancellationToken ct = default)
    {
        var bank = await _db.ExternalBanks
            .FirstOrDefaultAsync(b => b.Code == bankCode, ct);

        if (bank == null)
            throw new InvalidOperationException($"Banco {bankCode} no encontrado");

        var url = $"{bank.BaseUrl.TrimEnd('/')}/{bank.TransferEndpoint.TrimStart('/')}";
        var client = _httpFactory.CreateClient("BankLinkClient");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Headers.Add("X-Api-Key", bank.ApiKey);
        httpRequest.Headers.Add("Idempotency-Key", request.IdempotencyKey);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request, _jsonOptions),
            Encoding.UTF8,
            "application/json");

        _logger.LogInformation(
            "Enviando transferencia a {BankCode} - URL: {Url}",
            bankCode, url);

        try
        {
            var response = await client.SendAsync(httpRequest, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Error al enviar transferencia a {BankCode}. Status: {Status}, Body: {Body}",
                    bankCode, response.StatusCode, responseBody);

                throw new HttpRequestException(
                    $"Banco {bankCode} rechazó la transferencia: {response.StatusCode}");
            }

            var result = JsonSerializer.Deserialize<ExternalTransferResponse>(
                responseBody, _jsonOptions);

            _logger.LogInformation(
                "Transferencia a {BankCode} exitosa. TransactionId: {TransactionId}",
                bankCode, result?.TransactionId);

            return result!;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error de red al contactar {BankCode}", bankCode);
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout al contactar {BankCode}", bankCode);
            throw new TimeoutException($"Timeout al contactar banco {bankCode}", ex);
        }
    }

    public async Task<bool> ValidateAccountAsync(
        string bankCode,
        string accountNumber,
        CancellationToken ct = default)
    {
        var bank = await _db.ExternalBanks
            .FirstOrDefaultAsync(b => b.Code == bankCode, ct);

        if (bank == null)
            return false;

        if (!string.IsNullOrEmpty(bank.ValidationEndpoint))
        {
            var url = $"{bank.BaseUrl.TrimEnd('/')}/{bank.ValidationEndpoint.TrimStart('/')}";
            var client = _httpFactory.CreateClient("BankLinkClient");

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{url}/{accountNumber}");
            httpRequest.Headers.Add("X-Api-Key", bank.ApiKey);

            try
            {
                var response = await client.SendAsync(httpRequest, ct);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validando cuenta en {BankCode}", bankCode);
                return false;
            }
        }

        return true;
    }
}