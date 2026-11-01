namespace Application.Models.Responses;

public class ListaDeVeiculosResponse : BaseResponse
{
    public IList<int> Data { get; set; }
}