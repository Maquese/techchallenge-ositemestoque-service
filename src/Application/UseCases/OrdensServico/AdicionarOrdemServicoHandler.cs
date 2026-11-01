
using Application.Models.Requests;
using Application.Models.Responses;
using Domain.Aggregates;
using Domain.Entidades;
using Domain.Exceptions;
using Application.Interfaces;
//using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Formats.Asn1;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace Application.UseCases.OrdensServico;

public class AdicionarOrdemServicoHandler
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    //private readonly VeiculoRepository _veiculoRepository;
    private readonly ServicoRepository _servicoRepository;
    private readonly OrdemServicoRepository _ordemServicoRepository;
    //private readonly ILogger<AdicionarOrdemServicoHandler> _logger;

    public AdicionarOrdemServicoHandler(ServicoRepository servicoRepository, 
                                        OrdemServicoRepository ordemServicoRepository,
                                         HttpClient httpClient, IConfiguration configuration)
    {
        _configuration = configuration;
       _httpClient = httpClient;
        _servicoRepository = servicoRepository;
        _ordemServicoRepository = ordemServicoRepository;
        
    }
    
    public async Task<BaseResponse> Handle(AddOrdemServicoRequest ordemServico)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // Ignora maiúsculas/minúsculas
        };

        var data = await _httpClient.GetFromJsonAsync<VeiculoExisteResponse>($"{_configuration.GetSection("ClienteService").Value}api/Cliente/BuscarVeiculoPorId?id={ordemServico.VeiculoId}",jsonOptions);
        
        // using var scope = _logger.BeginScope(
        //     "correlation_id={CorrelationId} event_type={EventType} operation={Operation} vehicle_id={VehicleId} status={Status}",
        //     Activity.Current?.TraceId.ToString() ?? "n/a",
        //     "order_processing",
        //     "create_order",
        //     ordemServico.VeiculoId,
        //     "pending");

        // _logger.LogInformation(
        //     "Ordem de serviço em criação {@OrderCreateContext}",
        //     new
        //     {
        //         VehicleId = ordemServico.VeiculoId,
        //         Operation = "create_order"
        //     });

        // var veiculo = await _veiculoRepository.ObterPorId(ordemServico.VeiculoId);

        if (!data.Data.Existe)
        {
            // _logger.LogWarning(
            //     "Falha ao criar ordem de serviço: veículo não encontrado. VehicleId: {VehicleId}",
            //     ordemServico.VeiculoId);

            throw new DomainException(
                $"Veículo com ID {ordemServico.VeiculoId} não encotnrado.");
        }

        var ordemServicoEntity = new OrdemServico(
            ordemServico.VeiculoId,
            ordemServico.ServicosIds != null
                ? await _servicoRepository.ListarPorIds(ordemServico.ServicosIds)
                : new List<Servico>());

        await _ordemServicoRepository.Adicionar(ordemServicoEntity);

        // _logger.LogInformation(
        //     "Ordem de serviço criada com sucesso {@Order}",
        //     new
        //     {
        //         OrderId = ordemServicoEntity.Id,
        //         VehicleId = ordemServicoEntity.VeiculoId,
        //         Status = ordemServicoEntity.Status,
        //         Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        //     });

        return new BaseResponse
        {
            Success = true,
            Message = "Ordem de serviço adicionada com sucesso.",
            Data = ordemServicoEntity.Id
        };
    }
}
