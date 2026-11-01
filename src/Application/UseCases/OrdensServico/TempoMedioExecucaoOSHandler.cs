using Application.Interfaces;

namespace Application.UseCases.OrdensServico;

public class TempoMedioExecucaoOSHandler
{
    private readonly OrdemServicoRepository _ordemServicoRepository;

    public TempoMedioExecucaoOSHandler(OrdemServicoRepository ordemServicoRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
    }
    
    public async Task<int> Handle()
    {
        var ordensFinalizadas = await _ordemServicoRepository.ListarOrdensServicoPorStatus(new List<string> { "Finalizada", "Entregue" });
        if (!ordensFinalizadas.Any())
        {
            return 0;
        }

        var tempoTotalExecucao = ordensFinalizadas.Sum(os => (os.DataFimExecucao.Value - os.DataInicioExecucao.Value).TotalMinutes);
        return (int)(tempoTotalExecucao / ordensFinalizadas.Count());
    }
}
