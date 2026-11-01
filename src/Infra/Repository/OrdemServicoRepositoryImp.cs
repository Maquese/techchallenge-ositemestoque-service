using Domain.Aggregates;
using Domain.Entidades;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repository;

public class OrdemServicoRepositoryImp : BaseRepositoryImp<OrdemServico>, OrdemServicoRepository
{
    public OrdemServicoRepositoryImp(EFContext context) : base(context)
    {
        
    }

    public Task<IEnumerable<OrdemServico>> ListarOrdensServicoPorCliente(List<int> veiculosIds)
    {
        return Task.FromResult(_context.Set<OrdemServico>()
            .Where(os => veiculosIds.Contains(os.VeiculoId))
            .Include(os => os.Servicos)
            .Include(os => os.OrdemServicoItensEstoque)
                .ThenInclude(osi => osi.ItemEstoque)
            //.Include(os => os.Orcamentos)
            .AsEnumerable());
    }

    public Task<List<OrdemServico>> ListarOrdensServicoPorStatus(IList<string> status)
    {
        return Task.FromResult(_context.Set<OrdemServico>()
            .Where(os => os.Status == "Entregue" || os.Status == "Finalizada")
            .ToList());
    }

    public override async Task<OrdemServico> ObterPorId(int id)
    {
        return await _context.Set<OrdemServico>()
            .Include(os => os.Servicos)
            .Include(os => os.OrdemServicoItensEstoque)
                .ThenInclude(osi => osi.ItemEstoque)
            //.Include(os => os.Orcamentos)
            .FirstOrDefaultAsync(os => os.Id == id);
    }
}
