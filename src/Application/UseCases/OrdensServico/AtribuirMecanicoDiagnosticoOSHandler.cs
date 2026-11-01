using Application.Interfaces;
using Application.Models.Requests;
using Application.Models.Responses;
using Domain.Exceptions;
//using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.UseCases.OrdensServico;

public class AtribuirMecanicoDiagnosticoOSHandler
{
    private readonly OrdemServicoRepository _ordemServicoRepository;
    //private readonly ILogger<AtribuirMecanicoDiagnosticoOSHandler> _logger;
    public AtribuirMecanicoDiagnosticoOSHandler(OrdemServicoRepository ordemServicoRepository)
                                                //ILogger<AtribuirMecanicoDiagnosticoOSHandler> logger)
    {
        _ordemServicoRepository = ordemServicoRepository;
        //_logger = logger;
    }

    public async Task<BaseResponse> Handle(AtribuiMecanicoRequest atribuiEmDiagnostico)
    {        
        var ordemServico = await _ordemServicoRepository.ObterPorId(atribuiEmDiagnostico.OrdemServicoId);
        if (ordemServico == null)
        {
            throw new DomainException($"Ordem de serviço com ID {atribuiEmDiagnostico.OrdemServicoId} não encontrada.");
        }

        // using var scope = _logger.BeginScope(
        //     "correlation_id={CorrelationId} event_type={EventType} operation={Operation} order_id={OrderId} vehicle_id={VehicleId} status={Status}",
        //     Activity.Current?.TraceId.ToString() ?? "n/a",
        //     "order_processing",
        //     "start_order_diagnosis",
        //     ordemServico.Id,
        //     ordemServico.VeiculoId,
        //     ordemServico.Status);

        if(ordemServico.Status != "Recebida")
        {
            throw new DomainException($"Ordem de serviço com ID {ordemServico.Id} não está no status 'Recebida' para atribuição de mecânico ao diagnóstico.");
        }

        ordemServico.OSEmDiagnostico(atribuiEmDiagnostico.MecanicoAtribuido);
        await _ordemServicoRepository.Atualizar(ordemServico);

        // _logger.LogInformation(
        //     "Mecânico atribuído ao diagnóstico {@Assignment}",
        //     new
        //     {
        //         Mechanic = atribuiEmDiagnostico.MecanicoAtribuido,
        //         OrderId = ordemServico.Id,
        //         VehicleId = ordemServico.VeiculoId,
        //         Status = ordemServico.Status,
        //         Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        //     });

        return new BaseResponse
        {
            Success = true,
            Message = $"Mecânico '{atribuiEmDiagnostico.MecanicoAtribuido}' atribuído à ordem de serviço ID {atribuiEmDiagnostico.OrdemServicoId}.",
            Data = atribuiEmDiagnostico.OrdemServicoId
        };
    } 
}


