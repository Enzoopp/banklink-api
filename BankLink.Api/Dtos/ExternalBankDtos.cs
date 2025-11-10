using System.ComponentModel.DataAnnotations;

namespace BankLink.Api.Dtos;

public record ExternalBankCreateDto(
    [Required, StringLength(80)] string Name,
    [Required, StringLength(30)] string Code,
    [Required, Url] string BaseUrl,
    [Required, StringLength(120)] string TransferEndpoint,
    [StringLength(120)] string? ValidationEndpoint, // NUEVO: opcional
    [Required, StringLength(120)] string ApiKey
);

public record ExternalBankUpdateDto(
    [Required, StringLength(80)] string Name,
    [Required, StringLength(30)] string Code,
    [Required, Url] string BaseUrl,
    [Required, StringLength(120)] string TransferEndpoint,
    [StringLength(120)] string? ValidationEndpoint, // NUEVO: opcional
    [Required, StringLength(120)] string ApiKey,
    bool IsActive // NUEVO
);

public record ExternalBankResponseDto(
    int Id,
    string Name,
    string Code,
    string BaseUrl,
    string TransferEndpoint,
    string? ValidationEndpoint, // NUEVO
    bool IsActive, // NUEVO
    DateTime CreatedAt, // NUEVO
    string TransferUrl
);
