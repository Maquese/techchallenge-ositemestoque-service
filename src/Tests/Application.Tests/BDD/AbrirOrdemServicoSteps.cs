using System.Net;
using System.Text;
using Application.Interfaces;
using Application.Models.Requests;
using Application.Services;
using Application.Tests;
using Application.UseCases.OrdensServico;
using Domain.Aggregates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Reqnroll;

namespace Application.Tests.BDD;

[Binding]
public sealed class AbrirOrdemServicoSteps
{
    private readonly List<string> _chamadas = [];
    private bool _clienteServiceRespondeComSucesso;
    private bool _veiculoCriadoComSucesso;
    private bool _ordemServicoCriadaComSucesso;
    private bool _orcamentoServiceRetornaErro;
    private Exception? _exception;
    private Mock<OrdemServicoRepository>? _ordemRepository;

    [Given("que o ClienteService responde com sucesso")]
    public void DadoQueOClienteServiceRespondeComSucesso()
    {
        _clienteServiceRespondeComSucesso = true;
    }

    [Given("que o Veiculo é criado com sucesso")]
    public void DadoQueOVeiculoECriadoComSucesso()
    {
        _veiculoCriadoComSucesso = true;
    }

    [Given("que a Ordem de Serviço é criada com sucesso")]
    public void DadoQueAOrdemDeServicoECriadaComSucesso()
    {
        _ordemServicoCriadaComSucesso = true;
    }

    [Given("que o OrcamentoService retorna erro")]
    public void DadoQueOOrcamentoServiceRetornaErro()
    {
        _orcamentoServiceRetornaErro = true;
    }

    [When("uma Ordem de Serviço é aberta")]
    public async Task QuandoUmaOrdemDeServicoEAberta()
    {
        Assert.True(_clienteServiceRespondeComSucesso);
        Assert.True(_veiculoCriadoComSucesso);
        Assert.True(_ordemServicoCriadaComSucesso);
        Assert.True(_orcamentoServiceRetornaErro);

        var configuration = TestSupport.CreateConfiguration(
            "ClienteService",
            "http://cliente/");
        configuration.Setup(x => x["ClienteService"]).Returns("http://cliente/");
        configuration.Setup(x => x["OrcamentoService"]).Returns("http://orcamento/");

        using var sagaHttpClient = TestSupport.CreateHttpClient((request, _) =>
        {
            var endpoint = $"{request.Method.Method} {request.RequestUri!.PathAndQuery}";
            _chamadas.Add(endpoint);

            var response = request.RequestUri.AbsolutePath switch
            {
                "/api/Cliente/CriarCliente" =>
                    JsonResponse("""{"success":true,"data":9}"""),
                "/api/Cliente/AdicionarVeiculo" =>
                    JsonResponse("""{"success":true,"data":25}"""),
                "/api/Orcamento/CriarOrcamento" =>
                    new HttpResponseMessage(HttpStatusCode.InternalServerError),
                "/api/Cliente/InativarVeiculo" or "/api/Cliente/InativarCliente" =>
                    new HttpResponseMessage(HttpStatusCode.NoContent),
                _ => throw new InvalidOperationException($"Endpoint inesperado: {endpoint}.")
            };

            return Task.FromResult(response);
        });

        using var vehicleLookupHttpClient = TestSupport.CreateHttpClient((_, _) =>
            Task.FromResult(JsonResponse("""{"data":{"existe":true}}""")));

        _ordemRepository = new Mock<OrdemServicoRepository>();
        OrdemServico? ordemPersistida = null;
        _ordemRepository
            .Setup(x => x.Adicionar(It.IsAny<OrdemServico>()))
            .Callback<OrdemServico>(ordem =>
            {
                TestSupport.SetProperty(ordem, "Id", 42);
                ordemPersistida = ordem;
            })
            .Returns(Task.CompletedTask);
        _ordemRepository
            .Setup(x => x.ObterPorId(42))
            .Returns(() => Task.FromResult(ordemPersistida!));
        _ordemRepository
            .Setup(x => x.Remover(It.IsAny<OrdemServico>()))
            .Returns(Task.CompletedTask);

        var servicoRepository = new Mock<ServicoRepository>();
        servicoRepository
            .Setup(x => x.ListarPorIds(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Domain.Entidades.Servico>
            {
                TestSupport.CreateServico(valor: 125m)
            });

        var adicionarOrdemServicoHandler = new AdicionarOrdemServicoHandler(
            servicoRepository.Object,
            _ordemRepository.Object,
            vehicleLookupHttpClient,
            configuration.Object,
            NullLogger<AdicionarOrdemServicoHandler>.Instance);
        var service = new AbrirOrdemServicoSagaService(
            configuration.Object,
            sagaHttpClient,
            adicionarOrdemServicoHandler,
            _ordemRepository.Object,
            NullLogger<AbrirOrdemServicoSagaService>.Instance);

        _exception = await Record.ExceptionAsync(() =>
            service.Handle(new AberturaOSRequest { ServicosIds = [3] }));
    }

    [Then("as compensações devem ser executadas")]
    public void EntaoAsCompensacoesDevemSerExecutadas()
    {
        Assert.IsType<HttpRequestException>(_exception);
        Assert.Equal(
            new[]
            {
                "POST /api/Cliente/CriarCliente",
                "POST /api/Cliente/AdicionarVeiculo",
                "POST /api/Orcamento/CriarOrcamento",
                "DELETE /api/Cliente/InativarVeiculo?id=25",
                "DELETE /api/Cliente/InativarCliente?id=9"
            },
            _chamadas);

        Assert.NotNull(_ordemRepository);
        _ordemRepository.Verify(x => x.Remover(It.IsAny<OrdemServico>()), Times.Once);
    }

    private static HttpResponseMessage JsonResponse(string json) =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
}
