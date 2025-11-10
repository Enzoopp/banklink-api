namespace BankLink.Api.Domain;
public class Client {
  public int Id { get; set; }
  public string Nombre { get; set; } = default!;
  public string Apellido { get; set; } = default!;
  public string Dni { get; set; } = default!;
  public string? Direccion { get; set; }
  public string? Telefono { get; set; }
  public string Email { get; set; } = default!;
  public List<Account> Accounts { get; set; } = new();
}
