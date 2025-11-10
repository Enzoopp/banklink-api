using System.ComponentModel.DataAnnotations;

namespace BankLink.Api.Dtos;

public record ClientCreateDto(
    [Required, StringLength(80)] string Nombre,
    [Required, StringLength(80)] string Apellido,
    [Required, StringLength(20)] string Dni,
    string? Direccion,
    string? Telefono,
    [Required, EmailAddress] string Email);

public record ClientUpdateDto(
    [Required, StringLength(80)] string Nombre,
    [Required, StringLength(80)] string Apellido,
    string? Direccion,
    string? Telefono,
    [Required, EmailAddress] string Email);

public record ClientResponseDto(
    int Id, string Nombre, string Apellido, string Dni,
    string? Direccion, string? Telefono, string Email);
