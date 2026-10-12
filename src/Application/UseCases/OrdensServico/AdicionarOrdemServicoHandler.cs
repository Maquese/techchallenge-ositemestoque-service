
using Application.Models.Requests;
using Application.Models.Responses;
using Domain.Aggregates;
using Domain.Entidades;
using Domain.Exceptions;
using Application.Interfaces;
//using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.UseCases.OrdensServico;

public class AdicionarOrdemServicoHandler
{
    //private readonly VeiculoRepository _veiculoRepository;
    private readonly ServicoRepository _servicoRepository;
    private readonly OrdemServicoRepository _ordemServicoRepository;
    //private readonly ILogger<AdicionarOrdemServicoHandler> _logger;

    public AdicionarOrdemServicoHandler(ServicoRepository servicoRepository, 
                                        OrdemServicoRepository ordemServicoRepository)
    {
       
        _servicoRepository = servicoRepository;
        _ordemServicoRepository = ordemServicoRepository;
        
    }
    
    public async Task<BaseResponse> Handle(AddOrdemServicoRequest ordemServico)
    {
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

        // if (veiculo == null)
        // {
        //     _logger.LogWarning(
        //         "Falha ao criar ordem de serviço: veículo não encontrado. VehicleId: {VehicleId}",
        //         ordemServico.VeiculoId);

        //     throw new DomainException(
        //         $"Veículo com ID {ordemServico.VeiculoId} não encontrado.");
        // }

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
