namespace Application.Models.Requests;

public class AddItemEstoqueRequest
{
    public string Tipo { get;  set; }
    public string Nome { get; set; }
    public string Descricao { get;  set; }
    public decimal Valor { get;  set; }
    public DateTime? DataValidade { get;  set; }
    public string UnidadeMedida { get; set; }
}
