namespace Application.Models.Responses;

public class OrdensServicoOrdeandoResponse
{
    public string Status { get; set; }
    public string quantidade { get; set; }
    public List<OrdemServicoListResponse> OrdemServico { get; set; }

    public class OrdemServicoListResponse
    {
        public int ID  { get; set; }
        public DateTime Data  { get; set; }
    }
}
