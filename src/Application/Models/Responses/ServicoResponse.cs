namespace Application.Models.Responses;

public class ServicoResponse
{
    public int? Id { get; set; }
    public string Descricao { get; set; }
    public decimal Valor { get; set; }
    public int TempoEstimado { get; set; }
}
