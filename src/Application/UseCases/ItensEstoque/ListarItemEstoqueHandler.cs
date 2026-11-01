using System.Security.Cryptography.X509Certificates;
using Application.Interfaces;
using Application.Models.Responses;
using Domain.Aggregates;

namespace Application.UseCases.ItensEstoque;

public class ListarItemEstoqueHandler
{
    private readonly ItemEstoqueRepository _itemEstoqueRepository;

    public ListarItemEstoqueHandler(ItemEstoqueRepository itemEstoqueRepository)
    {
        _itemEstoqueRepository = itemEstoqueRepository;
    }    
    public async Task<BaseResponse> Handle()
    {
        return new BaseResponse
        {
            Message = "Listado com sucesso",
            Success = true,
            Data  = (await _itemEstoqueRepository.ListarAtivos()).Select(x => new ItemEstoqueResponse { Id = x. Id, Nome =  x.Nome, Descricao =  x.Descricao, Valor =  x.Valor, Quantidade = x.QuantidadeEmEstoque})
        };
    }

}
