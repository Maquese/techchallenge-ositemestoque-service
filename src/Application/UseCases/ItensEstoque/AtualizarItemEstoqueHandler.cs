using Application.Interfaces;
using Application.Models.Requests;
using Application.Models.Responses;
using Domain.Exceptions;

namespace Application.UseCases.ItensEstoque;

public class AtualizarItemEstoqueHandler
{
    private readonly ItemEstoqueRepository _itemEstoqueRepository;

    public AtualizarItemEstoqueHandler(ItemEstoqueRepository itemEstoqueRepository)
    {
        _itemEstoqueRepository = itemEstoqueRepository;
    }

     public async Task<BaseResponse> Handle(UpdateItemEstoqueRequest itemEstoqueModel)
    {
        var itemExistente = await _itemEstoqueRepository.ObterPorId(itemEstoqueModel.Id);
        if (itemExistente == null)
        {
            throw new DomainException("Item de estoque não encontrado para atualização.");
        }

        if(!itemExistente.EstaAtivo())
            throw new DomainException("Item inativo");

        itemExistente.Atualizar(
            itemEstoqueModel.Nome,
            itemEstoqueModel.Descricao,
            itemEstoqueModel.Valor,
            itemEstoqueModel.Tipo,
            itemEstoqueModel.UnidadeMedida,
            itemEstoqueModel.DataValidade
        );
        await _itemEstoqueRepository.Atualizar(itemExistente);

        return new BaseResponse
        {
            Message = "Atualizado com sucesso",
            Success = true
        };
    }
}
