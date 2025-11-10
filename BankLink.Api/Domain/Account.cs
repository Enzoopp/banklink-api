namespace BankLink.Api.Domain;
public class Account {
  public int Id { get; set; }
  public string AccountNumber { get; set; } = default!;
  public AccountType Type { get; set; }
  public decimal Balance { get; set; }
  public int ClientId { get; set; }
  public AccountStatus Status { get; set; } = AccountStatus.Activa;
  public Client? Client { get; set; }
  public List<Movement> Movements { get; set; } = new();
}
