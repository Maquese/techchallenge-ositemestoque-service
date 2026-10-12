using Application.Interfaces;
using Application.Models.Requests;
using Application.Models.Responses;
using Domain.Aggregates;

namespace Application.UseCases.ItensEstoque;

public class AdicionarItemEstoqueHandler
{
    private readonly ItemEstoqueRepository _itemEstoqueRepository;

    public AdicionarItemEstoqueHandler(ItemEstoqueRepository itemEstoqueRepository)
    {
        _itemEstoqueRepository = itemEstoqueRepository;
    }

    public async Task<BaseResponse> Handle(AddItemEstoqueRequest itemEstoqueModel)
    {
        var novaPeca = new ItemEstoque
        (
            itemEstoqueModel.Tipo,
            itemEstoqueModel.Nome,
            itemEstoqueModel.Descricao,
            itemEstoqueModel.Valor,
            itemEstoqueModel.UnidadeMedida,
            itemEstoqueModel.DataValidade
        );
        await _itemEstoqueRepository.Adicionar(novaPeca);

        return new BaseResponse
        {
            Message = "Item de estoque adicionado com sucesso",
            Success = true,
            Data = novaPeca.Id
        };
    }

}
