using Application.Interfaces;
using Application.Models.Responses;
using Domain.Exceptions;

namespace Application.UseCases.OrdensServico;

public class StatusAtualOSClienteHandler
{
    private readonly OrdemServicoRepository _ordemServicoRepository;
    //private readonly ClienteRepository _clienteRepository;

    public StatusAtualOSClienteHandler(OrdemServicoRepository ordemServicoRepository) //, ClienteRepository clienteRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
        //_clienteRepository = clienteRepository;
    }

    public async Task<BaseResponse> Handle(int clienteId)
    {
        // var cliente = await _clienteRepository.ObterPorId(clienteId);
        // if (cliente == null)        {
        //     throw new DomainException($"Cliente com ID {clienteId} não encontrado.");
        // }
        //var ordensServico = await _ordemServicoRepository.ListarOrdensServicoPorCliente("cliente.Veiculos.Select(v => v.Id).ToList()");

        return new BaseResponse
        {
            Success = true,
            Message = "Listado com sucesso",
            // Data = ordensServico.Select(os => new StatusOSsClienteResponse
            // {
            //     Id = os.Id,
            //     Status = os.Status,
            //     DataCriacao = os.DataAbertura,
            //     PlacaVeiculo = os.Veiculo.Placa.Valor
            // }).ToList()
        };
    }
}
