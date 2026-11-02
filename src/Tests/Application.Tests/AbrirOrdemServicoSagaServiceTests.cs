using System.Net;
using System.Text;
using Application.Interfaces;
using Application.Models.Requests;
using Application.Services;
using Application.UseCases.OrdensServico;
using Domain.Exceptions;
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
            configuration.Object);
        var service = new AbrirOrdemServicoSagaService(
            configuration.Object,
            sagaHttpClient,
            adicionarOrdemServicoHandler,
            ordemRepository.Object);

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

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
}
