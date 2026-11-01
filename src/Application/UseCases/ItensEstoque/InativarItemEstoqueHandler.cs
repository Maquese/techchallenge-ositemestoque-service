using Application.Interfaces;
using Application.Models.Responses;
using Domain.Exceptions;

namespace Application.UseCases.ItensEstoque;

public class InativarItemEstoqueHandler
{
    private readonly ItemEstoqueRepository _itemEstoqueRepository;

    public InativarItemEstoqueHandler(ItemEstoqueRepository itemEstoqueRepository)
    {
        _itemEstoqueRepository = itemEstoqueRepository;
    }    
    public async Task<BaseResponse> Handle(int id)
    {
        var itemExistente = await _itemEstoqueRepository.ObterPorId(id);
        if (itemExistente == null)
        {
            throw new DomainException("Item de estoque não encontrado para inativação.");
        }

        itemExistente.Inativar();
        await _itemEstoqueRepository.Atualizar(itemExistente);
        return new BaseResponse
        {
            Message = "Item inativado com sucesso",
            Success = true,
            Data = itemExistente.Id
        };
    }

}
