using System.Net;
using System.Reflection;
using System.Text;
using Application.Interfaces;
using Application.Models.Requests;
using Application.Services;
using Application.UseCases.OrdensServico;
using Domain.Aggregates;
using Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.Tests;

public class AbrirOrdemServicoSagaServiceTests
{
    [Fact]
    public async Task Handle_FalhaAoValidarVeiculo_DeveCompensarVeiculoEClienteEmOrdemReversa()
    {
        var chamadas = new List<string>();
        using var sagaHttpClient = TestSupport.CreateHttpClient((request, _) =>
        {
            chamadas.Add($"{request.Method.Method} {request.RequestUri!.PathAndQuery}");
            var response = request.RequestUri!.AbsolutePath switch
            {
                "/api/Cliente/CriarCliente" => JsonResponse("""{"success":true,"data":9}"""),
                "/api/Cliente/AdicionarVeiculo" => JsonResponse("""{"success":true,"data":25}"""),
                _ => new HttpResponseMessage(HttpStatusCode.OK)
            };

            return Task.FromResult(response);
        });
        using var vehicleLookupHttpClient = TestSupport.CreateHttpClient((_, _) =>
            Task.FromResult(JsonResponse("""{"data":{"existe":false}}""")));

        var configuration = TestSupport.CreateConfiguration("ClienteService", "http://cliente/");
        configuration.Setup(x => x["ClienteService"]).Returns("http://cliente/");
        var ordemRepository = new Mock<OrdemServicoRepository>();
        var servicoRepository = new Mock<ServicoRepository>();
        var adicionarOrdemServicoHandler = new AdicionarOrdemServicoHandler(
            servicoRepository.Object,
            ordemRepository.Object,
            vehicleLookupHttpClient,
            configuration.Object,
            NullLogger<AdicionarOrdemServicoHandler>.Instance);
        var service = new AbrirOrdemServicoSagaService(
            configuration.Object,
            sagaHttpClient,
            adicionarOrdemServicoHandler,
            ordemRepository.Object,
            NullLogger<AbrirOrdemServicoSagaService>.Instance);

        await Assert.ThrowsAsync<DomainException>(() =>
            service.Handle(new AberturaOSRequest()));

        Assert.Equal(
            new[]
            {
                "POST /api/Cliente/CriarCliente",
                "POST /api/Cliente/AdicionarVeiculo",
                "DELETE /api/Cliente/InativarVeiculo?id=25",
                "DELETE /api/Cliente/InativarCliente?id=9"
            },
            chamadas);
        ordemRepository.Verify(x => x.Adicionar(It.IsAny<Domain.Aggregates.OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task Handle_FalhaAoConsultarOrdem_DeveExecutarTodasAsCompensacoesEmOrdemReversa()
    {
        var chamadas = new List<string>();
        using var sagaHttpClient = TestSupport.CreateHttpClient((request, _) =>
        {
            chamadas.Add($"{request.Method.Method} {request.RequestUri!.PathAndQuery}");
            var response = request.RequestUri.AbsolutePath switch
            {
                "/api/Cliente/CriarCliente" => JsonResponse("""{"success":true,"data":9}"""),
                "/api/Cliente/AdicionarVeiculo" => JsonResponse("""{"success":true,"data":25}"""),
                _ => new HttpResponseMessage(HttpStatusCode.OK)
            };

            return Task.FromResult(response);
        });
        using var vehicleLookupHttpClient = TestSupport.CreateHttpClient((_, _) =>
            Task.FromResult(JsonResponse("""{"data":{"existe":true}}""")));

        var configuration = TestSupport.CreateConfiguration("ClienteService", "http://cliente/");
        configuration.Setup(x => x["ClienteService"]).Returns("http://cliente/");
        var ordemRepository = new Mock<OrdemServicoRepository>();
        ordemRepository
            .Setup(x => x.Adicionar(It.IsAny<OrdemServico>()))
            .Callback<OrdemServico>(ordem => TestSupport.SetProperty(ordem, "Id", 42))
            .Returns(Task.CompletedTask);
        ordemRepository
            .Setup(x => x.ObterPorId(42))
            .ReturnsAsync((OrdemServico)null!);
        var adicionarOrdemServicoHandler = new AdicionarOrdemServicoHandler(
            new Mock<ServicoRepository>().Object,
            ordemRepository.Object,
            vehicleLookupHttpClient,
            configuration.Object,
            NullLogger<AdicionarOrdemServicoHandler>.Instance);
        var service = new AbrirOrdemServicoSagaService(
            configuration.Object,
            sagaHttpClient,
            adicionarOrdemServicoHandler,
            ordemRepository.Object,
            NullLogger<AbrirOrdemServicoSagaService>.Instance);

        var exception = await Assert.ThrowsAsync<AggregateException>(() =>
            service.Handle(new AberturaOSRequest()));

        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.Contains("não foi encontrada para calcular", exception.InnerExceptions[0].Message);
        Assert.Contains("não foi encontrada para compensação", exception.InnerExceptions[1].InnerException!.Message);
        Assert.Equal(
            new[]
            {
                "POST /api/Cliente/CriarCliente",
                "POST /api/Cliente/AdicionarVeiculo",
                "DELETE /api/Cliente/InativarVeiculo?id=25",
                "DELETE /api/Cliente/InativarCliente?id=9"
            },
            chamadas);
        ordemRepository.Verify(x => x.ObterPorId(42), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ComSucesso_DeveCriarOrdemEOrcamento()
    {
        var chamadas = new List<string>();
        var orcamentoId = Guid.NewGuid();
        using var sagaHttpClient = TestSupport.CreateHttpClient((request, _) =>
        {
            chamadas.Add($"{request.Method.Method} {request.RequestUri!.PathAndQuery}");
            var response = request.RequestUri.AbsolutePath switch
            {
                "/api/Cliente/CriarCliente" => JsonResponse("""{"success":true,"data":"9"}"""),
                "/api/Cliente/AdicionarVeiculo" => JsonResponse("""{"success":true,"data":25}"""),
                "/api/Orcamento/CriarOrcamento" =>
                    JsonResponse($$"""{"success":true,"data":"{{orcamentoId}}"}"""),
                _ => throw new InvalidOperationException("Endpoint inesperado.")
            };

            return Task.FromResult(response);
        });
        using var vehicleLookupHttpClient = TestSupport.CreateHttpClient((_, _) =>
            Task.FromResult(JsonResponse("""{"data":{"existe":true}}""")));

        var configuration = TestSupport.CreateConfiguration("ClienteService", "http://cliente/");
        configuration.Setup(x => x["ClienteService"]).Returns("http://cliente/");
        configuration.Setup(x => x["OrcamentoService"]).Returns("http://orcamento/");

        var ordemRepository = new Mock<OrdemServicoRepository>();
        OrdemServico? ordemPersistida = null;
        ordemRepository
            .Setup(x => x.Adicionar(It.IsAny<OrdemServico>()))
            .Callback<OrdemServico>(ordem =>
            {
                TestSupport.SetProperty(ordem, "Id", 42);
                ordemPersistida = ordem;
            })
            .Returns(Task.CompletedTask);
        ordemRepository
            .Setup(x => x.ObterPorId(42))
            .Returns(() => Task.FromResult(ordemPersistida!));

        var servico = TestSupport.CreateServico(valor: 125m);
        var servicoRepository = new Mock<ServicoRepository>();
        servicoRepository
            .Setup(x => x.ListarPorIds(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Domain.Entidades.Servico> { servico });
        var adicionarOrdemServicoHandler = new AdicionarOrdemServicoHandler(
            servicoRepository.Object,
            ordemRepository.Object,
            vehicleLookupHttpClient,
            configuration.Object,
            NullLogger<AdicionarOrdemServicoHandler>.Instance);
        var service = new AbrirOrdemServicoSagaService(
            configuration.Object,
            sagaHttpClient,
            adicionarOrdemServicoHandler,
            ordemRepository.Object,
            NullLogger<AbrirOrdemServicoSagaService>.Instance);

        var response = await service.Handle(new AberturaOSRequest
        {
            ServicosIds = new List<int> { 3 }
        });

        Assert.True(response.Success);
        Assert.Equal(42, response.Data);
        Assert.Equal(
            new[]
            {
                "POST /api/Cliente/CriarCliente",
                "POST /api/Cliente/AdicionarVeiculo",
                "POST /api/Orcamento/CriarOrcamento"
            },
            chamadas);
        ordemRepository.Verify(x => x.ObterPorId(42), Times.Once);
        ordemRepository.Verify(x => x.Remover(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("\"identificador-invalido\"")]
    [InlineData("2147483648")]
    public async Task Handle_ComIdentificadorDeClienteInvalido_DeveFalhar(string data)
    {
        using var sagaHttpClient = TestSupport.CreateHttpClient((_, _) =>
            Task.FromResult(JsonResponse($$"""{"success":true,"data":{{data}}}""")));
        using var vehicleLookupHttpClient = TestSupport.CreateHttpClient((_, _) =>
            Task.FromResult(JsonResponse("""{"data":{"existe":true}}""")));

        var configuration = TestSupport.CreateConfiguration("ClienteService", "http://cliente/");
        configuration.Setup(x => x["ClienteService"]).Returns("http://cliente/");
        var ordemRepository = new Mock<OrdemServicoRepository>();
        var adicionarOrdemServicoHandler = new AdicionarOrdemServicoHandler(
            new Mock<ServicoRepository>().Object,
            ordemRepository.Object,
            vehicleLookupHttpClient,
            configuration.Object,
            NullLogger<AdicionarOrdemServicoHandler>.Instance);
        var service = new AbrirOrdemServicoSagaService(
            configuration.Object,
            sagaHttpClient,
            adicionarOrdemServicoHandler,
            ordemRepository.Object,
            NullLogger<AbrirOrdemServicoSagaService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Handle(new AberturaOSRequest()));

        ordemRepository.Verify(x => x.Adicionar(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ComUrlDeServicoInvalida_DeveFalharAntesDaChamadaHttp()
    {
        var chamadaHttp = false;
        using var sagaHttpClient = TestSupport.CreateHttpClient((_, _) =>
        {
            chamadaHttp = true;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });
        using var vehicleLookupHttpClient = TestSupport.CreateHttpClient((_, _) =>
            Task.FromResult(JsonResponse("""{"data":{"existe":true}}""")));

        var configuration = TestSupport.CreateConfiguration("ClienteService", "url-invalida");
        configuration.Setup(x => x["ClienteService"]).Returns("url-invalida");
        var ordemRepository = new Mock<OrdemServicoRepository>();
        var adicionarOrdemServicoHandler = new AdicionarOrdemServicoHandler(
            new Mock<ServicoRepository>().Object,
            ordemRepository.Object,
            vehicleLookupHttpClient,
            configuration.Object,
            NullLogger<AdicionarOrdemServicoHandler>.Instance);
        var service = new AbrirOrdemServicoSagaService(
            configuration.Object,
            sagaHttpClient,
            adicionarOrdemServicoHandler,
            ordemRepository.Object,
            NullLogger<AbrirOrdemServicoSagaService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.Handle(new AberturaOSRequest()));

        Assert.False(chamadaHttp);
    }

    [Fact]
    public async Task PostWithoutResponseAsync_ComSucesso_DeveEnviarPostEValidarResposta()
    {
        HttpMethod? metodoEnviado = null;
        string? endpointEnviado = null;
        string? corpoEnviado = null;
        using var httpClient = TestSupport.CreateHttpClient(async (request, _) =>
        {
            metodoEnviado = request.Method;
            endpointEnviado = request.RequestUri!.AbsoluteUri;
            corpoEnviado = await request.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.NoContent);
        });

        var configuration = TestSupport.CreateConfiguration(
            "OrcamentoService",
            "http://orcamento/");
        configuration.Setup(x => x["OrcamentoService"]).Returns("http://orcamento/");
        var ordemRepository = new Mock<OrdemServicoRepository>();
        var service = new AbrirOrdemServicoSagaService(
            configuration.Object,
            httpClient,
            new AdicionarOrdemServicoHandler(
                new Mock<ServicoRepository>().Object,
                ordemRepository.Object,
                TestSupport.CreateHttpClient((_, _) =>
                    Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))),
                configuration.Object,
                NullLogger<AdicionarOrdemServicoHandler>.Instance),
            ordemRepository.Object,
            NullLogger<AbrirOrdemServicoSagaService>.Instance);

        var method = typeof(AbrirOrdemServicoSagaService).GetMethod(
            "PostWithoutResponseAsync",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        var request = new { OrdemServicoId = 42 };
        var operation = method
            .MakeGenericMethod(request.GetType())
            .Invoke(service, new object?[]
            {
                "OrcamentoService",
                "api/Orcamento/NegarOrcamento",
                request,
                CancellationToken.None
            }) as Task;

        Assert.NotNull(operation);
        await operation;

        Assert.Equal(HttpMethod.Post, metodoEnviado);
        Assert.Equal(
            "http://orcamento/api/Orcamento/NegarOrcamento",
            endpointEnviado);
        Assert.Equal("""{"OrdemServicoId":42}""", corpoEnviado);
    }

    [Fact]
    public void ReadIntId_ComLong_DeveAceitarSomenteValoresDentroDoLimiteDeInt()
    {
        var readIntId = typeof(AbrirOrdemServicoSagaService).GetMethod(
            "ReadIntId",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(readIntId);
        Assert.Equal(42, readIntId.Invoke(null, new object?[] { 42L, "teste" }));

        var exception = Assert.Throws<TargetInvocationException>(() =>
            readIntId.Invoke(null, new object?[] { (long)int.MaxValue + 1, "teste" }));
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
}
