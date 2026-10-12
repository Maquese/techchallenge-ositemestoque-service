namespace Application.Models.Requests;

public class AddServicoRequest
{
    public string Descricao { get; set; }
    public decimal Valor { get; set; }
    public int TempoEstimado { get; set; }
    
}