using System;
using BankLink.Api.Domain; // <- para MovementType

namespace BankLink.Api.Dtos
{
    public class MovementResponseDto
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public MovementType Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Description { get; set; }
    }
}
