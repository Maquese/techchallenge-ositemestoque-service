
using Application.Models.Requests;
using Domain.Exceptions;
using Application.Interfaces;
using Application.Models.Responses;
//using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.UseCases.OrdensServico;

public class FinalizarOrdemServicoHandler
{
    private readonly OrdemServicoRepository _ordemServicoRepository;
    private readonly ItemEstoqueRepository _itensEstoqueRepository;
    //private readonly ILogger<FinalizarOrdemServicoHandler> _logger;

    public FinalizarOrdemServicoHandler(OrdemServicoRepository ordemServicoRepository, 
                                        ItemEstoqueRepository itensEstoqueRepository)
                                        //ILogger<FinalizarOrdemServicoHandler> logger)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _itensEstoqueRepository = itensEstoqueRepository;
       // _logger = logger;
    }
    
    public async Task<BaseResponse> Handle(int ordemServicoId)
    {
        var ordemServico = await _ordemServicoRepository.ObterPorId(ordemServicoId);
        if (ordemServico == null)
        {
            throw new DomainException($"Ordem de serviço com ID {ordemServicoId} não encontrada.");
        }

        if (ordemServico.Status != "Em execução")
        {
            throw new DomainException($"Ordem de serviço com ID {ordemServico.Id} não está no status 'Em execução' para finalização.");
        }
        // using var scope = _logger.BeginScope(
        //     "correlation_id={CorrelationId} event_type={EventType} operation={Operation} order_id={OrderId} vehicle_id={VehicleId} status={Status}",
        //     Activity.Current?.TraceId.ToString() ?? "n/a",
        //     "order_processing",
        //     "finalize_order_execution",
        //     ordemServico.Id,
        //     ordemServico.VeiculoId,
        //     ordemServico.Status);
        await DeduzirItensEstoque(ordemServico.OrdemServicoItensEstoque?.Select(i => new AddItensOrdemServicoRequest { id = i.ItemEstoqueId, quantidade = i.Quantidade }).ToList() ?? new List<AddItensOrdemServicoRequest>());

        ordemServico.FinalizarOrdemServico();
        await _ordemServicoRepository.Atualizar(ordemServico);
        // _logger.LogInformation(
        //     "Ordem de serviço finalizada com sucesso {@Order}",
        //     new
        //     {
        //         TempoGasto = ordemServico.TempoMedioExecucao(),
        //         OrderId = ordemServico.Id,
        //         VehicleId = ordemServico.VeiculoId,
        //         Status = ordemServico.Status,
        //         Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        //     });
        return new BaseResponse{
            Message = $"Ordem de serviço ID {ordemServicoId} finalizada com sucesso.",
            Success = true,
            Data = ordemServico.Id
        };
    }

    private async Task DeduzirItensEstoque(List<AddItensOrdemServicoRequest> itensEstoque)
    {   
        foreach (var item in itensEstoque)
        {
            var itemEstoque = await _itensEstoqueRepository.ObterPorId(item.id);
            if (itemEstoque == null)
            {
                throw new DomainException($"Item de estoque com ID {item.id} não encontrado.");
            }

            itemEstoque.DeduzirQuantidadeEstoque(item.quantidade);
            await _itensEstoqueRepository.Atualizar(itemEstoque);
        }
    }
}
