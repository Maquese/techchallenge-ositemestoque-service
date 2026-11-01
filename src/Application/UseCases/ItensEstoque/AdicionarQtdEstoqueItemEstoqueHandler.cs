using Application.Interfaces;
using Application.Models.Requests;
using Application.Models.Responses;
using Domain.Exceptions;

namespace Application.UseCases.ItensEstoque;

public class AdicionarQtdEstoqueItemEstoqueHandler
{
    private readonly ItemEstoqueRepository _itemEstoqueRepository;

    public AdicionarQtdEstoqueItemEstoqueHandler(ItemEstoqueRepository itemEstoqueRepository)
    {
        _itemEstoqueRepository = itemEstoqueRepository;
    }

    public async Task<BaseResponse> Handle(AddQuantidadeItemEstoqueRequest adicionarQuantidadeModel)
    {
        var itemExistente = await _itemEstoqueRepository.ObterPorId(adicionarQuantidadeModel.Id);
        if (itemExistente == null)
        {
            throw new DomainException("Item de estoque não encontrado para adicionar quantidade.");
        }

        if(!itemExistente.EstaAtivo())
            throw new DomainException("Item inativo");

        itemExistente.AdicionarQuantidadeEstoque(adicionarQuantidadeModel.Quantidade);
        await _itemEstoqueRepository.Atualizar(itemExistente);

        return new BaseResponse
        {
            Message = "Quantidade ajustada com sucesso",
            Success = true, 
            Data = itemExistente.Id
        };
    }
}
