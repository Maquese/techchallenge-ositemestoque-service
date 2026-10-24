using System.Net.Http.Json;
using System.Text.Json;
using Application.Interfaces;
using Application.Models.Responses;
using Domain.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Application.UseCases.OrdensServico;

public class StatusAtualOSClienteHandler
{
    private readonly IConfiguration _configuration;
    private readonly OrdemServicoRepository _ordemServicoRepository;
    private readonly HttpClient _httpClient;

    //private readonly ClienteRepository _clienteRepository;

    public StatusAtualOSClienteHandler(OrdemServicoRepository ordemServicoRepository,
                                       HttpClient httpClient, IConfiguration configuration) //, ClienteRepository clienteRepository)
    {
        _configuration = configuration;
        _ordemServicoRepository = ordemServicoRepository;
        _httpClient = httpClient;
        //_clienteRepository = clienteRepository;
    }

    public async Task<BaseResponse> Handle(int clienteId)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // Ignora maiúsculas/minúsculas
        };

        var veiculos = await _httpClient.GetFromJsonAsync<ListaDeVeiculosResponse>($"{_configuration.GetSection("ClienteService").Value}api/Cliente/BuscarVeiculosPorCliente?idcliente={clienteId}",jsonOptions);
        
        // var cliente = await _clienteRepository.ObterPorId(clienteId);
        // if (cliente == null)        {
        //     throw new DomainException($"Cliente com ID {clienteId} não encontrado.");
        // }
        var ordensServico = await _ordemServicoRepository.ListarOrdensServicoPorCliente(veiculos.Data.ToList());

        return new BaseResponse
        {
            Success = true,
            Message = "Listado com sucesso",
            Data = ordensServico.Select(os => new StatusOSsClienteResponse
            {
                Id = os.Id,
                Status = os.Status,
                DataCriacao = os.DataAbertura,
                IdVeiculo = os.VeiculoId
            }).ToList()
        };
    }
}
