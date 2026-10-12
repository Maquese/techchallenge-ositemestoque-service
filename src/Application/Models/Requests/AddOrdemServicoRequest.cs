namespace Application.Models.Requests;

public class AddOrdemServicoRequest
{
    public int VeiculoId { get; set; }
    public List<int> ServicosIds { get; set; }
}
