using Application.Interfaces;
using Application.Models.Responses;
using Domain.Exceptions;

namespace Application.UseCases.ItensEstoque;
public class ObterItemEstoqueHandler
{
    private readonly ItemEstoqueRepository _itemEstoqueRepository;

    public ObterItemEstoqueHandler(ItemEstoqueRepository itemEstoqueRepository)
    {
        _itemEstoqueRepository = itemEstoqueRepository;
    }    
    public async Task<BaseResponse> Handle(int id)
    {
        var item = await _itemEstoqueRepository.ObterPorId(id);
        if (item == null)
            throw new DomainException ("Não encontrado");
        if(!item.EstaAtivo())
            throw new  DomainException("Item inativo");

        return new BaseResponse
        {
            Message = "Item estoque encontrado",
            Success = true, 
            Data = new ItemEstoqueResponse
            {
                Id = item.Id,
                Nome = item.Nome,
                Descricao = item.Descricao,
                Valor = item.Valor,
                Quantidade = item.QuantidadeEmEstoque
            }
        };
    }

}
