
namespace Application.Models.Requests;

public class DiagnosticoFinalizadoRequest
{
    public int Id { get; set; }
    public List<AddItensOrdemServicoRequest> ItensEstoque { get; set; }
}
