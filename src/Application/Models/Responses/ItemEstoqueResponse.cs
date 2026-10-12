namespace Application.Models.Responses;

public class ItemEstoqueResponse
{
    public int Id {get;set;}
    public string Nome { get; set; }
    public string Descricao { get;  set; }
    public decimal Valor { get;  set; }
    public int Quantidade {get;set;}
}