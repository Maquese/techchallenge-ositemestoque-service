namespace Application.Models.Responses;

public class OrcamentoAprovadoResponse : BaseResponse
{
    public Status Data {get;set;} 
    public class Status
    {
        public bool OrcamentoAprovado { get; set; }
    }
}

