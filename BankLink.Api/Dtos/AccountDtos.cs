using System.ComponentModel.DataAnnotations;
using BankLink.Api.Domain;

namespace BankLink.Api.Dtos;

public record AccountCreateDto(
    [Required, StringLength(30)] string AccountNumber,
    [Required] AccountType Type,
    [Required] int ClientId,
    decimal? InitialBalance
);

public record AccountUpdateStatusDto([Required] AccountStatus Status);

public record AccountResponseDto(
    int Id, string AccountNumber, AccountType Type,
    decimal Balance, int ClientId, AccountStatus Status);

public record MoneyOperationDto([Required, Range(0.01, double.MaxValue)] decimal Monto, string? Descripcion);
