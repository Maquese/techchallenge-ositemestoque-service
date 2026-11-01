namespace Application.Models.Requests;

public class AberturaOSRequest
{
    public string Documento { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Celular { get; set; }
    public string Placa { get; set; }
    public string Modelo { get; set; }
    public string Marca { get; set; }
    public int Ano { get; set; }
    public List<int> ServicosIds { get; set; }
    
}
