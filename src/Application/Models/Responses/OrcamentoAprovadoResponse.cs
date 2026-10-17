namespace Application.Models.Responses;

public class OrcamentoAprovadoResponse
{
     public bool success { get; set; }
    public string message { get; set; }
    public Status data {get;set;} 
    public class Status
    {
        public bool orcamentoAprovado { get; set; }
    }
}

