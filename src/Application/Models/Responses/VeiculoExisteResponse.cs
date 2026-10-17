namespace Application.Models.Responses;

public class VeiculoExisteResponse : BaseResponse
{
    public    VeiculoExiste Data {get;set;} 
    public class VeiculoExiste
    {
        public bool Existe { get; set; }
    }
}

