namespace BankLink.Api.Domain;
public class Movement {
  public int Id { get; set; }
  public int AccountId { get; set; }
  public MovementType Type { get; set; }
  public decimal Amount { get; set; }
  public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
  public string? Description { get; set; }
  public string? ExternalReference { get; set; }
  public Account? Account { get; set; }
  public string? IdempotencyKey { get; set; }
  public string? ExternalRef { get; set; }
}
