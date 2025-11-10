using System.ComponentModel.DataAnnotations;

namespace BankLink.Api.Dtos
{
    public record RegisterDto(
        [Required, EmailAddress] string Email,
        [Required, StringLength(80)] string FullName,
        [Required, MinLength(6)] string Password);

    public record LoginDto(
        [Required, EmailAddress] string Email,
        [Required] string Password);

    public record AuthResponseDto(string AccessToken, DateTime ExpiresAtUtc);
}
