using Application.Interfaces;
using Application.Models.Requests;
using Application.Models.Responses;
using Domain.Exceptions;
using Microsoft.Extensions.Configuration;

//using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;

namespace Application.UseCases.OrdensServico;

public class AtribuirMecanicoExecucaoOSHandler
{
    private readonly OrdemServicoRepository _ordemServicoRepository;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    //private readonly ILogger<AtribuirMecanicoExecucaoOSHandler> _logger;
    public AtribuirMecanicoExecucaoOSHandler(OrdemServicoRepository ordemServicoRepository ,
                                             HttpClient httpClient, IConfiguration configuration)
                                             //ILogger<AtribuirMecanicoExecucaoOSHandler> logger)
    {
        _ordemServicoRepository = ordemServicoRepository;
        _httpClient = httpClient;
        _configuration = configuration;
        //_logger = logger;
    }

    public async Task<BaseResponse> Handle(AtribuiMecanicoRequest atribuiEmReparo)
    {
        var ordemServico = await _ordemServicoRepository.ObterPorId(atribuiEmReparo.OrdemServicoId);
        if (ordemServico == null)
        {
            throw new DomainException($"Ordem de serviço com ID {atribuiEmReparo.OrdemServicoId} não encontrada.");
        }
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // Ignora maiúsculas/minúsculas
        };

        var statusAprovada = await _httpClient.GetFromJsonAsync<OrcamentoAprovadoResponse>($"{_configuration.GetSection("OrcamentoService").Value}api/Orcamento/OrcamentoAprovado?ordemservicoid={atribuiEmReparo.OrdemServicoId}",jsonOptions);
        
       if (!statusAprovada.Data.OrcamentoAprovado)
            throw new DomainException($"Ordem de serviço com ID {ordemServico.Id} não está no status 'Aprovada' para atribuição de mecânico à execução.");
        
        // using var scope = _logger.BeginScope(
        //     "correlation_id={CorrelationId} event_type={EventType} operation={Operation} order_id={OrderId} vehicle_id={VehicleId} status={Status}",
        //     Activity.Current?.TraceId.ToString() ?? "n/a",
        //     "order_processing",
        //     "start_order_execution",
        //     ordemServico.Id,
        //     ordemServico.VeiculoId,
        //     ordemServico.Status);
         ordemServico.EmExecucao(atribuiEmReparo.MecanicoAtribuido);
        await _ordemServicoRepository.Atualizar(ordemServico);
        // _logger.LogInformation(
        //     "Mecânico atribuído à execução {@Assignment}",
        //     new
        //     {
        //         Mechanic = atribuiEmReparo.MecanicoAtribuido,
        //         OrderId = ordemServico.Id,
        //         VehicleId = ordemServico.VeiculoId,
        //         Status = ordemServico.Status,
        //         Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        //     });
        return new BaseResponse
        {
            Success = true,
            Message = $"Mecânico '{atribuiEmReparo.MecanicoAtribuido}' atribuído à ordem de serviço ID {atribuiEmReparo.OrdemServicoId}.",
            Data = ordemServico.Id
        };
    }

}
