using Application.Interfaces;
using Domain.Aggregates;
namespace Infra.Repository;


public class ItemEstoqueRepositoryImp : BaseRepositoryImp<ItemEstoque>, ItemEstoqueRepository
{
    public ItemEstoqueRepositoryImp(EFContext context) : base(context)
    {
    }
}