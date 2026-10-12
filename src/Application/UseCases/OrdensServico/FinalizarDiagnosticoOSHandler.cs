
using Application.Models.Requests;
using Domain.Aggregates;
using Domain.Entidades;
using Domain.Exceptions;
using Application.Interfaces;
using Application.Models.Responses;
//using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.UseCases.OrdensServico;

public class FinalizarDiagnosticoOSHandler
{
    private readonly OrdemServicoRepository _ordemServicoRepository;
    private readonly ServicoRepository _servicoRepository;
    private readonly ItemEstoqueRepository _itensEstoqueRepository;
    //private readonly OrcamentoRepository _orcamentoRepository;
    //private readonly ILogger<FinalizarDiagnosticoOSHandler> _logger;

    public FinalizarDiagnosticoOSHandler(OrdemServicoRepository ordemServicoRepository, 
                                         ServicoRepository servicoRepository, 
                                         ItemEstoqueRepository itensEstoqueRepository) 
                                         //OrcamentoRepository orcamentoRepository,
                                         //ILogger<FinalizarDiagnosticoOSHandler> logger)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _servicoRepository = servicoRepository;
        _itensEstoqueRepository = itensEstoqueRepository;
        //_orcamentoRepository = orcamentoRepository; 
        //_logger = logger;
    }

      public async Task<BaseResponse> Handle(DiagnosticoFinalizadoRequest diagnosticoFinalizadoModel)
    {
        var ordemServico = await _ordemServicoRepository.ObterPorId(diagnosticoFinalizadoModel.Id);
        if (ordemServico == null)
        {
            throw new DomainException($"Ordem de serviço para o veículo ID {diagnosticoFinalizadoModel.Id} não encontrada.");
        }

        //using var scope = _logger.BeginScope(
        //    "correlation_id={CorrelationId} event_type={EventType} operation={Operation} order_id={OrderId} vehicle_id={VehicleId} status={Status}",
        //    Activity.Current?.TraceId.ToString() ?? "n/a",
        //    "order_processing",
        //    "finalize_order_diagnosis",
        //    ordemServico.Id,
        //    ordemServico.VeiculoId,
        //    ordemServico.Status);

        if(ordemServico.Status != "Em diagnóstico")
        {
            throw new DomainException($"Ordem de serviço com ID {ordemServico.Id} não está no status 'Em diagnóstico' para finalização do diagnóstico.");
        }

        var itensEstoque = diagnosticoFinalizadoModel.ItensEstoque.Select(item => new OrdemServicoItemEstoque
        (diagnosticoFinalizadoModel.Id, item.id, item.quantidade)).ToList();

        var itensEstoqueBase = await _itensEstoqueRepository.ListarPorIds(itensEstoque.Select(x => x.ItemEstoqueId).ToList());

        if(itensEstoqueBase.Count != itensEstoque.Count)
        {
            throw new DomainException($"Um ou mais itens de estoque informados não estão mais disponíveis.");
        }

        ordemServico.OSDiagnosticada(itensEstoque);
        //_logger.LogInformation(
        //    "Diagnóstico finalizado {@Diagnosis}",
        //    new
        //    {
        //        OrderId = ordemServico.Id,
        //        VehicleId = ordemServico.VeiculoId,
        //        Status = ordemServico.Status,
        //        Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        //    });
        // var orcamento = new Orcamento(
        //     diagnosticoFinalizadoModel.Id,
        //     await CalcularValorTotalOrcamento(
        //         diagnosticoFinalizadoModel.ItensEstoque,
        //         ordemServico.Servicos?.Select(x => x.Id).ToList() ?? new List<int>()),
        //     "obs");
        //aqui tem que fazer um achamada para o servico de ORCAMENTO
        await _ordemServicoRepository.Atualizar(ordemServico);
        //await _orcamentoRepository.Adicionar(orcamento);
        // _logger.LogInformation(
        //     "Orçamento criado {@Budget}",
        //     new
        //     {
        //         TempoGasto = (DateTime.Now - ordemServico.DataAbertura).TotalMilliseconds,
        //         OrderId = ordemServico.Id,
        //         VehicleId = ordemServico.VeiculoId,
        //         Status = ordemServico.Status,
        //         BudgetId = orcamento.Id
        //     });
        return new BaseResponse
        {
            Success = true,
            Message = $"Ordem de servico id:{ordemServico.Id} status aguardando aprovação, orçamento id:{"orcamento.Id"} cridado",
            Data = "orcamento.Id"
        };
    }

    private async Task<decimal> CalcularValorTotalOrcamento(List<AddItensOrdemServicoRequest> itensEstoque, List<int> servicosIds)
    {
        var servicos = await _servicoRepository.ListarPorIds(servicosIds);

        var itens = await _itensEstoqueRepository.ListarPorIds(itensEstoque.Select(i => i.id).ToList());

        return servicos.Sum(s => s.Valor) + itens.Sum(i => i.Valor * itensEstoque.First(e => e.id == i.Id).quantidade);
    }
}
