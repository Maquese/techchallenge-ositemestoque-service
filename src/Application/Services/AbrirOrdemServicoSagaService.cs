using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Application.Models.Requests;
using Application.Models.Responses;
using Application.UseCases.OrdensServico;
using Application.Interfaces;

namespace Application.Services;

public class AbrirOrdemServicoSagaService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly AdicionarOrdemServicoHandler _adicionarOrdemServicoHandler;
    private readonly OrdemServicoRepository _ordemServicoRepository;

    public AbrirOrdemServicoSagaService(
        IConfiguration configuration,
        HttpClient httpClient,
        AdicionarOrdemServicoHandler adicionarOrdemServicoHandler,
        OrdemServicoRepository ordemServicoRepository)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _adicionarOrdemServicoHandler = adicionarOrdemServicoHandler;
        _ordemServicoRepository = ordemServicoRepository;
    }

    public async Task<BaseResponse> Handle(
        AberturaOSRequest request,
        CancellationToken cancellationToken = default)
    {
        var compensations = new List<(string Name, Func<Task> Execute)>();

        try
        {
            var clienteId = await PostForIntIdAsync(
                "ClienteService",
                "api/Cliente/CriarCliente",
                new ClienteCreateRequest
                {
                    Documento = request.Documento,
                    Nome = request.Nome,
                    Email = request.Email,
                    Celular = request.Celular
                },
                cancellationToken);
            compensations.Add(("inativar cliente", () => DeleteAsync(
                "ClienteService",
                $"api/Cliente/InativarCliente?id={clienteId}",
                CancellationToken.None)));

            var veiculoId = await PostForIntIdAsync(
                "ClienteService",
                "api/Cliente/AdicionarVeiculo",
                new VeiculoCreateRequest
                {
                    Placa = request.Placa,
                    Modelo = request.Modelo,
                    Marca = request.Marca,
                    Ano = request.Ano,
                    ClienteId = clienteId
                },
                cancellationToken);
            compensations.Add(("inativar veículo", () => DeleteAsync(
                "ClienteService",
                $"api/Cliente/InativarVeiculo?id={veiculoId}",
                CancellationToken.None)));

            var ordemServico = await _adicionarOrdemServicoHandler.Handle(new AddOrdemServicoRequest
            {
                ServicosIds = request.ServicosIds,
                VeiculoId = veiculoId
            });
            var ordemServicoId = ReadIntId(ordemServico.Data, "ordem de serviço");
            compensations.Add(("remover ordem de serviço", async () =>
            {
                var entidade = await _ordemServicoRepository.ObterPorId(ordemServicoId);
                if (entidade is null)
                {
                    throw new InvalidOperationException(
                        $"A ordem de serviço {ordemServicoId} não foi encontrada para compensação.");
                }

                await _ordemServicoRepository.Remover(entidade);
            }));

            var entidadeOrdemServico = await _ordemServicoRepository.ObterPorId(ordemServicoId);
            if (entidadeOrdemServico is null)
            {
                throw new InvalidOperationException(
                    $"A ordem de serviço {ordemServicoId} não foi encontrada para calcular o orçamento.");
            }

            var valorTotal = entidadeOrdemServico.CalcularValorOrdemServico();
            if (!valorTotal.HasValue)
            {
                throw new InvalidOperationException(
                    $"Não foi possível calcular o valor da ordem de serviço {ordemServicoId}.");
            }

            var orcamentoId = await PostForGuidIdAsync(
                "OrcamentoService",
                "api/Orcamento/CriarOrcamento",
                new AddOrcamentoRequest
                {
                    OrdemServicoId = ordemServicoId,
                    ValorTotal = valorTotal.Value,
                    Observacao = $"Orçamento inicial da ordem de serviço {ordemServicoId}."
                },
                cancellationToken);
            compensations.Add(("negar orçamento", () => PostWithoutResponseAsync(
                "OrcamentoService",
                "api/Orcamento/NegarOrcamento",
                orcamentoId,
                CancellationToken.None)));

            return ordemServico;
        }
        catch (Exception operationException)
        {
            var compensationExceptions = new List<Exception>();
            for (var index = compensations.Count - 1; index >= 0; index--)
            {
                var compensation = compensations[index];
                try
                {
                    await compensation.Execute();
                }
                catch (Exception compensationException)
                {
                    compensationExceptions.Add(new InvalidOperationException(
                        $"A compensação '{compensation.Name}' falhou.",
                        compensationException));
                }
            }

            if (compensationExceptions.Count > 0)
            {
                throw new AggregateException(
                    "A abertura da ordem de serviço falhou e uma ou mais compensações também falharam.",
                    new[] { operationException }.Concat(compensationExceptions));
            }

            throw;
        }
    }

    private async Task<int> PostForIntIdAsync<TRequest>(
        string serviceSetting,
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var response = await PostForResponseAsync(serviceSetting, endpoint, request, cancellationToken);
        return ReadIntId(response.Data, endpoint);
    }

    private async Task<Guid> PostForGuidIdAsync<TRequest>(
        string serviceSetting,
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var response = await PostForResponseAsync(serviceSetting, endpoint, request, cancellationToken);
        if (response.Data.ValueKind == JsonValueKind.String &&
            response.Data.TryGetGuid(out var id))
        {
            return id;
        }

        throw new InvalidOperationException(
            $"O serviço '{serviceSetting}' retornou um identificador inválido para '{endpoint}'.");
    }

    private async Task<RemoteBaseResponse> PostForResponseAsync<TRequest>(
        string serviceSetting,
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            GetEndpoint(serviceSetting, endpoint),
            request,
            JsonOptions,
            cancellationToken);
        return await ReadResponseAsync(response, serviceSetting, endpoint, cancellationToken);
    }

    private async Task PostWithoutResponseAsync<TRequest>(
        string serviceSetting,
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            GetEndpoint(serviceSetting, endpoint),
            request,
            JsonOptions,
            cancellationToken);
        await EnsureSuccessAsync(response, serviceSetting, endpoint, cancellationToken);
    }

    private async Task DeleteAsync(
        string serviceSetting,
        string endpoint,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.DeleteAsync(
            GetEndpoint(serviceSetting, endpoint),
            cancellationToken);
        await EnsureSuccessAsync(response, serviceSetting, endpoint, cancellationToken);
    }

    private async Task<RemoteBaseResponse> ReadResponseAsync(
        HttpResponseMessage response,
        string serviceSetting,
        string endpoint,
        CancellationToken cancellationToken)
    {
        await EnsureSuccessAsync(response, serviceSetting, endpoint, cancellationToken);
        var body = await response.Content.ReadFromJsonAsync<RemoteBaseResponse>(
            JsonOptions,
            cancellationToken);
        if (body is null || !body.Success)
        {
            throw new InvalidOperationException(
                $"O serviço '{serviceSetting}' rejeitou a chamada '{endpoint}': " +
                $"{body?.Message ?? "resposta vazia"}.");
        }

        return body;
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string serviceSetting,
        string endpoint,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"A chamada '{endpoint}' no serviço '{serviceSetting}' falhou " +
            $"com status {(int)response.StatusCode}: {body}",
            null,
            response.StatusCode);
    }

    private Uri GetEndpoint(string serviceSetting, string endpoint)
    {
        var baseAddress = _configuration[serviceSetting];
        if (!Uri.TryCreate(baseAddress, UriKind.Absolute, out var serviceUri))
        {
            throw new InvalidOperationException(
                $"A URL do serviço '{serviceSetting}' não está configurada corretamente.");
        }

        return new Uri(serviceUri, endpoint);
    }

    private static int ReadIntId(object? data, string source)
    {
        var value = data switch
        {
            int integer => integer,
            long longInteger when longInteger is >= int.MinValue and <= int.MaxValue =>
                (int)longInteger,
            JsonElement json when json.ValueKind == JsonValueKind.Number &&
                                  json.TryGetInt32(out var integer) => integer,
            JsonElement json when json.ValueKind == JsonValueKind.String &&
                                  int.TryParse(
                                      json.GetString(),
                                      NumberStyles.Integer,
                                      CultureInfo.InvariantCulture,
                                      out var integer) => integer,
            _ => 0
        };

        if (value <= 0)
        {
            throw new InvalidOperationException(
                $"O serviço '{source}' retornou um identificador inválido.");
        }

        return value;
    }

    private sealed class RemoteBaseResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public JsonElement Data { get; set; }
    }

    private sealed class ClienteCreateRequest
    {
        public string Documento { get; init; } = string.Empty;
        public string Nome { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Celular { get; init; } = string.Empty;
    }

    private sealed class VeiculoCreateRequest
    {
        public string Placa { get; init; } = string.Empty;
        public string Modelo { get; init; } = string.Empty;
        public string Marca { get; init; } = string.Empty;
        public int Ano { get; init; }
        public int ClienteId { get; init; }
    }
}
