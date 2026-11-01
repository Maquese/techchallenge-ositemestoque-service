using System.Net;
using System.Net.Http.Json;
using System.Text;
using Application.Interfaces;
using Application.Models.Requests;
using Application.Models.Responses;
using Application.UseCases.OrdensServico;
using Domain.Aggregates;
using Domain.Entidades;
using Domain.Exceptions;
using Moq;

namespace Application.Tests;

public class OrdemServicoHandlersTests
{
    [Fact]
    public async Task AdicionarOrdemServico_VeiculoInexistente_DeveLancarDomainException()
    {
        var ordemRepository = new Mock<OrdemServicoRepository>();
        var servicoRepository = new Mock<ServicoRepository>();
        using var client = TestSupport.CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"data":{"existe":false}}""",
                    Encoding.UTF8,
                    "application/json")
            }));
        var handler = new AdicionarOrdemServicoHandler(
            servicoRepository.Object,
            ordemRepository.Object,
            client,
            TestSupport.CreateConfiguration("ClienteService", "http://cliente/").Object);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new AddOrdemServicoRequest { VeiculoId = 12 }));

        ordemRepository.Verify(x => x.Adicionar(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarOrdemServico_SemServicos_DeveCriarOrdem()
    {
        var ordemRepository = new Mock<OrdemServicoRepository>();
        ordemRepository.Setup(x => x.Adicionar(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
        var servicoRepository = new Mock<ServicoRepository>();
        using var client = CreateSuccessfulVehicleClient();
        var handler = new AdicionarOrdemServicoHandler(
            servicoRepository.Object,
            ordemRepository.Object,
            client,
            TestSupport.CreateConfiguration("ClienteService", "http://cliente/").Object);

        var response = await handler.Handle(new AddOrdemServicoRequest
        {
            VeiculoId = 12,
            ServicosIds = null!
        });

        Assert.True(response.Success);
        Assert.Equal("Ordem de serviço adicionada com sucesso.", response.Message);
        ordemRepository.Verify(x => x.Adicionar(It.Is<OrdemServico>(os =>
            os.VeiculoId == 12 && os.Status == "Recebida" && !os.Servicos!.Any())), Times.Once);
        servicoRepository.Verify(x => x.ListarPorIds(It.IsAny<List<int>>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarOrdemServico_ComServicos_DeveConsultarServicosECriarOrdem()
    {
        var ordemRepository = new Mock<OrdemServicoRepository>();
        ordemRepository.Setup(x => x.Adicionar(It.IsAny<OrdemServico>())).Returns(Task.CompletedTask);
        var servico = TestSupport.CreateServico();
        var servicoRepository = new Mock<ServicoRepository>();
        servicoRepository.Setup(x => x.ListarPorIds(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Servico> { servico });
        using var client = CreateSuccessfulVehicleClient();
        var handler = new AdicionarOrdemServicoHandler(
            servicoRepository.Object,
            ordemRepository.Object,
            client,
            TestSupport.CreateConfiguration("ClienteService", "http://cliente/").Object);

        await handler.Handle(new AddOrdemServicoRequest
        {
            VeiculoId = 12,
            ServicosIds = new List<int> { 2 }
        });

        servicoRepository.Verify(x => x.ListarPorIds(It.Is<List<int>>(ids => ids.Single() == 2)), Times.Once);
        ordemRepository.Verify(x => x.Adicionar(It.Is<OrdemServico>(os =>
            os.Servicos!.Single() == servico)), Times.Once);
    }

    [Fact]
    public async Task AdicionarServico_DevePersistirEConfirmarSucesso()
    {
        var repository = new Mock<ServicoRepository>();
        repository.Setup(x => x.Adicionar(It.IsAny<Servico>())).Returns(Task.CompletedTask);
        var handler = new AdicionarServicoHandler(repository.Object);

        var response = await handler.Handle(new AddServicoRequest
        {
            Descricao = "Revisão",
            Valor = 100m,
            TempoEstimado = 60
        });

        Assert.True(response.Success);
        Assert.Equal("Adicionado com sucesso", response.Message);
        repository.Verify(x => x.Adicionar(It.Is<Servico>(s =>
            s.Descricao == "Revisão" && s.Valor == 100m)), Times.Once);
    }

    [Fact]
    public async Task AdicionarServico_ComValorInvalido_DevePropagarExcecao()
    {
        var repository = new Mock<ServicoRepository>();
        var handler = new AdicionarServicoHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(new AddServicoRequest
        {
            Descricao = "Revisão",
            Valor = 0m,
            TempoEstimado = 60
        }));

        repository.Verify(x => x.Adicionar(It.IsAny<Servico>()), Times.Never);
    }

    [Fact]
    public async Task AtribuirMecanicoDiagnostico_OrdemNaoEncontrada_DeveLancarDomainException()
    {
        var repository = new Mock<OrdemServicoRepository>();
        repository.Setup(x => x.ObterPorId(7)).ReturnsAsync((OrdemServico)null!);
        var handler = new AtribuirMecanicoDiagnosticoOSHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new AtribuiMecanicoRequest { OrdemServicoId = 7, MecanicoAtribuido = "Ana" }));
    }

    [Fact]
    public async Task AtribuirMecanicoDiagnostico_StatusIncorreto_DeveLancarDomainException()
    {
        var ordem = new OrdemServico(1);
        ordem.OSAprovada();
        var repository = new Mock<OrdemServicoRepository>();
        repository.Setup(x => x.ObterPorId(7)).ReturnsAsync(ordem);
        var handler = new AtribuirMecanicoDiagnosticoOSHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new AtribuiMecanicoRequest { OrdemServicoId = 7, MecanicoAtribuido = "Ana" }));

        repository.Verify(x => x.Atualizar(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task AtribuirMecanicoDiagnostico_OrdemRecebida_DeveAtualizarStatus()
    {
        var ordem = new OrdemServico(1);
        var repository = new Mock<OrdemServicoRepository>();
        repository.Setup(x => x.ObterPorId(7)).ReturnsAsync(ordem);
        repository.Setup(x => x.Atualizar(ordem)).Returns(Task.CompletedTask);
        var handler = new AtribuirMecanicoDiagnosticoOSHandler(repository.Object);

        var response = await handler.Handle(
            new AtribuiMecanicoRequest { OrdemServicoId = 7, MecanicoAtribuido = "Ana" });

        Assert.True(response.Success);
        Assert.Equal("Em diagnóstico", ordem.Status);
        Assert.Equal("Ana", ordem.MecanicoAtribuido);
        repository.Verify(x => x.Atualizar(ordem), Times.Once);
    }

    [Fact]
    public async Task AtribuirMecanicoExecucao_OrdemNaoEncontrada_DeveLancarDomainException()
    {
        var repository = new Mock<OrdemServicoRepository>();
        repository.Setup(x => x.ObterPorId(8)).ReturnsAsync((OrdemServico)null!);
        using var client = TestSupport.CreateHttpClient((_, _) =>
            throw new InvalidOperationException("Não deveria chamar o serviço de orçamento."));
        var handler = new AtribuirMecanicoExecucaoOSHandler(
            repository.Object,
            client,
            TestSupport.CreateConfiguration("OrcamentoService", "http://orcamento/").Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new AtribuiMecanicoRequest { OrdemServicoId = 8, MecanicoAtribuido = "Ana" }));
    }

    [Fact]
    public async Task AtribuirMecanicoExecucao_OrcamentoNaoAprovado_DeveLancarDomainException()
    {
        var ordem = new OrdemServico(1);
        var repository = new Mock<OrdemServicoRepository>();
        repository.Setup(x => x.ObterPorId(8)).ReturnsAsync(ordem);
        using var client = TestSupport.CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"data":{"orcamentoAprovado":false}}""",
                    Encoding.UTF8,
                    "application/json")
            }));
        var handler = new AtribuirMecanicoExecucaoOSHandler(
            repository.Object,
            client,
            TestSupport.CreateConfiguration("OrcamentoService", "http://orcamento/").Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new AtribuiMecanicoRequest { OrdemServicoId = 8, MecanicoAtribuido = "Ana" }));

        repository.Verify(x => x.Atualizar(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task AtribuirMecanicoExecucao_OrcamentoAprovado_DeveIniciarExecucao()
    {
        var ordem = new OrdemServico(1);
        var repository = new Mock<OrdemServicoRepository>();
        repository.Setup(x => x.ObterPorId(8)).ReturnsAsync(ordem);
        repository.Setup(x => x.Atualizar(ordem)).Returns(Task.CompletedTask);
        using var client = TestSupport.CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"data":{"orcamentoAprovado":true}}""",
                    Encoding.UTF8,
                    "application/json")
            }));
        var handler = new AtribuirMecanicoExecucaoOSHandler(
            repository.Object,
            client,
            TestSupport.CreateConfiguration("OrcamentoService", "http://orcamento/").Object);

        var response = await handler.Handle(
            new AtribuiMecanicoRequest { OrdemServicoId = 8, MecanicoAtribuido = "Ana" });

        Assert.True(response.Success);
        Assert.Equal("Em execução", ordem.Status);
        Assert.Equal("Ana", ordem.MecanicoAtribuido);
        Assert.NotNull(ordem.DataInicioExecucao);
        repository.Verify(x => x.Atualizar(ordem), Times.Once);
    }

    [Fact]
    public async Task AtualizarServico_NaoEncontrado_DeveLancarDomainException()
    {
        var repository = new Mock<ServicoRepository>();
        repository.Setup(x => x.ObterPorId(9)).ReturnsAsync((Servico)null!);
        var handler = new AtualizarServicoHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new UpdateServicoRequest { Id = 9 }));
    }

    [Fact]
    public async Task AtualizarServico_Inativo_DeveLancarDomainException()
    {
        var servico = TestSupport.CreateServico();
        servico.Inativar();
        var repository = new Mock<ServicoRepository>();
        repository.Setup(x => x.ObterPorId(9)).ReturnsAsync(servico);
        var handler = new AtualizarServicoHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new UpdateServicoRequest { Id = 9 }));

        repository.Verify(x => x.Atualizar(It.IsAny<Servico>()), Times.Never);
    }

    [Fact]
    public async Task AtualizarServico_Ativo_DeveAlterarEPersistir()
    {
        var servico = TestSupport.CreateServico();
        var repository = new Mock<ServicoRepository>();
        repository.Setup(x => x.ObterPorId(9)).ReturnsAsync(servico);
        repository.Setup(x => x.Atualizar(servico)).Returns(Task.CompletedTask);
        var handler = new AtualizarServicoHandler(repository.Object);

        var response = await handler.Handle(new UpdateServicoRequest
        {
            Id = 9,
            Descricao = "Revisão completa",
            Valor = 120m,
            TempoEstimado = 90
        });

        Assert.True(response.Success);
        Assert.Equal("Revisão completa", servico.Descricao);
        Assert.Equal(120m, servico.Valor);
        repository.Verify(x => x.Atualizar(servico), Times.Once);
    }

    [Fact]
    public async Task BuscarServico_NaoEncontrado_DeveLancarDomainException()
    {
        var repository = new Mock<ServicoRepository>();
        repository.Setup(x => x.ObterPorId(10)).ReturnsAsync((Servico)null!);
        var handler = new BuscarServicoHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(10));
    }

    [Fact]
    public async Task BuscarServico_Inativo_DeveLancarDomainException()
    {
        var servico = TestSupport.CreateServico();
        servico.Inativar();
        var repository = new Mock<ServicoRepository>();
        repository.Setup(x => x.ObterPorId(10)).ReturnsAsync(servico);
        var handler = new BuscarServicoHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(10));
    }

    [Fact]
    public async Task BuscarServico_Ativo_DeveRetornarDados()
    {
        var servico = TestSupport.CreateServico("Troca", 70m);
        var repository = new Mock<ServicoRepository>();
        repository.Setup(x => x.ObterPorId(10)).ReturnsAsync(servico);
        var handler = new BuscarServicoHandler(repository.Object);

        var response = await handler.Handle(10);

        Assert.True(response.Success);
        var result = Assert.IsType<ServicoResponse>(response.Data);
        Assert.Equal("Troca", result.Descricao);
        Assert.Equal(70m, result.Valor);
    }

    [Fact]
    public async Task ConsultarStatusOS_NaoEncontrada_DeveLancarDomainException()
    {
        var repository = new Mock<OrdemServicoRepository>();
        repository.Setup(x => x.ObterPorId(11)).ReturnsAsync((OrdemServico)null!);
        var handler = new ConsutaStatusOSHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(11));
    }

    [Fact]
    public async Task ConsultarStatusOS_Encontrada_DeveRetornarStatus()
    {
        var ordem = new OrdemServico(1);
        var repository = new Mock<OrdemServicoRepository>();
        repository.Setup(x => x.ObterPorId(11)).ReturnsAsync(ordem);
        var handler = new ConsutaStatusOSHandler(repository.Object);

        var response = await handler.Handle(11);

        Assert.True(response.Success);
        Assert.Equal("Status da ordem de serviço obtido com sucesso.", response.Message);
        Assert.Equal("Recebida", response.Data!.GetType().GetProperty("Status")!.GetValue(response.Data));
    }

    [Fact]
    public async Task FinalizarDiagnostico_OrdemNaoEncontrada_DeveLancarDomainException()
    {
        var ordemRepository = new Mock<OrdemServicoRepository>();
        ordemRepository.Setup(x => x.ObterPorId(12)).ReturnsAsync((OrdemServico)null!);
        using var client = CreateNoRequestHttpClient();
        var handler = CreateFinalizarDiagnosticoHandler(
            ordemRepository,
            new Mock<ServicoRepository>(),
            new Mock<ItemEstoqueRepository>(),
            client);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(NewDiagnosticoRequest(12)));
    }

    [Fact]
    public async Task FinalizarDiagnostico_StatusIncorreto_DeveLancarDomainException()
    {
        var ordemRepository = new Mock<OrdemServicoRepository>();
        ordemRepository.Setup(x => x.ObterPorId(12)).ReturnsAsync(new OrdemServico(1));
        using var client = CreateNoRequestHttpClient();
        var handler = CreateFinalizarDiagnosticoHandler(
            ordemRepository,
            new Mock<ServicoRepository>(),
            new Mock<ItemEstoqueRepository>(),
            client);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(NewDiagnosticoRequest(12)));
    }

    [Fact]
    public async Task FinalizarDiagnostico_ItemIndisponivel_DeveLancarDomainException()
    {
        var ordem = new OrdemServico(1);
        ordem.OSEmDiagnostico("Ana");
        var ordemRepository = new Mock<OrdemServicoRepository>();
        ordemRepository.Setup(x => x.ObterPorId(12)).ReturnsAsync(ordem);
        var itemRepository = new Mock<ItemEstoqueRepository>();
        itemRepository.Setup(x => x.ListarPorIds(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<ItemEstoque>());
        using var client = CreateNoRequestHttpClient();
        var handler = CreateFinalizarDiagnosticoHandler(
            ordemRepository,
            new Mock<ServicoRepository>(),
            itemRepository,
            client);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(NewDiagnosticoRequest(12)));

        ordemRepository.Verify(x => x.Atualizar(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task FinalizarDiagnostico_ComItensEServicos_DeveCalcularOrcamentoEAtualizarOrdem()
    {
        var servico = TestSupport.CreateServico("Revisão", 30m);
        var ordem = new OrdemServico(1, new List<Servico> { servico });
        ordem.OSEmDiagnostico("Ana");
        var ordemRepository = new Mock<OrdemServicoRepository>();
        ordemRepository.Setup(x => x.ObterPorId(12)).ReturnsAsync(ordem);
        ordemRepository.Setup(x => x.Atualizar(ordem)).Returns(Task.CompletedTask);
        var servicoRepository = new Mock<ServicoRepository>();
        servicoRepository.Setup(x => x.ListarPorIds(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Servico> { servico });
        var item = TestSupport.CreateItem(10m);
        var itemRepository = new Mock<ItemEstoqueRepository>();
        itemRepository.SetupSequence(x => x.ListarPorIds(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<ItemEstoque> { item })
            .ReturnsAsync(new List<ItemEstoque> { item });
        AddOrcamentoRequest? postedOrcamento = null;
        using var client = TestSupport.CreateHttpClient(async (request, _) =>
        {
            postedOrcamento = await request.Content!.ReadFromJsonAsync<AddOrcamentoRequest>();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"success":true,"message":"criado","data":{"orcamentoId":5}}""",
                    Encoding.UTF8,
                    "application/json")
            };
        });
        var handler = CreateFinalizarDiagnosticoHandler(
            ordemRepository,
            servicoRepository,
            itemRepository,
            client);

        var response = await handler.Handle(new DiagnosticoFinalizadoRequest
        {
            Id = 12,
            Observacao = "Diagnóstico concluído",
            ItensEstoque = new List<AddItensOrdemServicoRequest>
            {
                new() { id = 0, quantidade = 2 }
            }
        });

        Assert.True(response.Success);
        Assert.Equal("Aguardando aprovação", ordem.Status);
        Assert.Single(ordem.OrdemServicoItensEstoque!);
        Assert.Equal(50m, postedOrcamento!.ValorTotal);
        Assert.Equal("Diagnóstico concluído", postedOrcamento.Observacao);
        ordemRepository.Verify(x => x.Atualizar(ordem), Times.Once);
    }

    [Fact]
    public async Task FinalizarOrdemServico_OrdemNaoEncontrada_DeveLancarDomainException()
    {
        var ordemRepository = new Mock<OrdemServicoRepository>();
        ordemRepository.Setup(x => x.ObterPorId(13)).ReturnsAsync((OrdemServico)null!);
        var handler = new FinalizarOrdemServicoHandler(
            ordemRepository.Object,
            new Mock<ItemEstoqueRepository>().Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(13));
    }

    [Fact]
    public async Task FinalizarOrdemServico_StatusIncorreto_DeveLancarDomainException()
    {
        var ordemRepository = new Mock<OrdemServicoRepository>();
        ordemRepository.Setup(x => x.ObterPorId(13)).ReturnsAsync(new OrdemServico(1));
        var handler = new FinalizarOrdemServicoHandler(
            ordemRepository.Object,
            new Mock<ItemEstoqueRepository>().Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(13));
    }

    [Fact]
    public async Task FinalizarOrdemServico_ComItens_DeveDeduzirEstoqueEFinalizar()
    {
        var ordem = new OrdemServico(1);
        ordem.OSEmDiagnostico("Ana");
        ordem.OSDiagnosticada(new List<OrdemServicoItemEstoque>
        {
            new(13, 14, 3)
        });
        ordem.EmExecucao("Ana");
        var ordemRepository = new Mock<OrdemServicoRepository>();
        ordemRepository.Setup(x => x.ObterPorId(13)).ReturnsAsync(ordem);
        ordemRepository.Setup(x => x.Atualizar(ordem)).Returns(Task.CompletedTask);
        var item = TestSupport.CreateItem();
        item.AdicionarQuantidadeEstoque(5);
        var itemRepository = new Mock<ItemEstoqueRepository>();
        itemRepository.Setup(x => x.ObterPorId(14)).ReturnsAsync(item);
        itemRepository.Setup(x => x.Atualizar(item)).Returns(Task.CompletedTask);
        var handler = new FinalizarOrdemServicoHandler(ordemRepository.Object, itemRepository.Object);

        var response = await handler.Handle(13);

        Assert.True(response.Success);
        Assert.Equal("Finalizada", ordem.Status);
        Assert.Equal(2, item.QuantidadeEmEstoque);
        itemRepository.Verify(x => x.Atualizar(item), Times.Once);
        ordemRepository.Verify(x => x.Atualizar(ordem), Times.Once);
    }

    [Fact]
    public async Task FinalizarOrdemServico_ItemNaoEncontrado_DeveLancarDomainException()
    {
        var ordem = new OrdemServico(1);
        ordem.OSDiagnosticada(new List<OrdemServicoItemEstoque> { new(13, 14, 3) });
        ordem.EmExecucao("Ana");
        var ordemRepository = new Mock<OrdemServicoRepository>();
        ordemRepository.Setup(x => x.ObterPorId(13)).ReturnsAsync(ordem);
        var itemRepository = new Mock<ItemEstoqueRepository>();
        itemRepository.Setup(x => x.ObterPorId(14)).ReturnsAsync((ItemEstoque)null!);
        var handler = new FinalizarOrdemServicoHandler(ordemRepository.Object, itemRepository.Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(13));

        ordemRepository.Verify(x => x.Atualizar(It.IsAny<OrdemServico>()), Times.Never);
    }

    [Fact]
    public async Task InativarServico_NaoEncontrado_DeveLancarDomainException()
    {
        var repository = new Mock<ServicoRepository>();
        repository.Setup(x => x.ObterPorId(15)).ReturnsAsync((Servico)null!);
        var handler = new InativarServicoHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(15));
    }

    [Fact]
    public async Task InativarServico_Encontrado_DeveAcionarRepositorio()
    {
        var servico = TestSupport.CreateServico();
        var repository = new Mock<ServicoRepository>();
        repository.Setup(x => x.ObterPorId(15)).ReturnsAsync(servico);
        repository.Setup(x => x.Inativar(servico)).Returns(Task.CompletedTask);
        var handler = new InativarServicoHandler(repository.Object);

        var response = await handler.Handle(15);

        Assert.True(response.Success);
        repository.Verify(x => x.Inativar(servico), Times.Once);
    }

    [Fact]
    public async Task ListarOrdensOrdenadas_DeveAgruparStatusEIgnorarOutrosStatus()
    {
        var ordens = new List<OrdemServico>();
        var execucao = new OrdemServico(1);
        execucao.EmExecucao("Ana");
        ordens.Add(execucao);
        var aguardando = new OrdemServico(2);
        aguardando.OSDiagnosticada(new List<OrdemServicoItemEstoque>());
        ordens.Add(aguardando);
        var diagnostico = new OrdemServico(3);
        diagnostico.OSEmDiagnostico("Bia");
        ordens.Add(diagnostico);
        ordens.Add(new OrdemServico(4));
        var negada = new OrdemServico(5);
        negada.OSNegada();
        ordens.Add(negada);
        var repository = new Mock<OrdemServicoRepository>();
        repository.Setup(x => x.ListarAtivos()).ReturnsAsync(ordens);
        var handler = new ListarOrdensServicoOrdenadoHandler(repository.Object);

        var response = await handler.Handle();

        Assert.True(response.Success);
        var grupos = Assert.IsAssignableFrom<IEnumerable<OrdensServicoOrdeandoResponse>>(response.Data).ToList();
        Assert.Equal(4, grupos.Count);
        Assert.Collection(grupos,
            group => { Assert.Equal("Em execução", group.Status); Assert.Equal("1", group.quantidade); },
            group => { Assert.Equal("Aguardando aprovação", group.Status); Assert.Equal("1", group.quantidade); },
            group => { Assert.Equal("Em diagnóstico", group.Status); Assert.Equal("1", group.quantidade); },
            group => { Assert.Equal("Recebida", group.Status); Assert.Equal("1", group.quantidade); });
    }

    [Fact]
    public async Task ListarServicosAtivos_DeveMapearServicos()
    {
        var repository = new Mock<ServicoRepository>();
        repository.Setup(x => x.ListarAtivos())
            .ReturnsAsync(new List<Servico> { TestSupport.CreateServico("Alinhamento", 45m) });
        var handler = new ListarServicosAtivosHandler(repository.Object);

        var response = await handler.Handle();

        Assert.True(response.Success);
        var servicos = Assert.IsAssignableFrom<IEnumerable<ServicoResponse>>(response.Data);
        var servico = Assert.Single(servicos);
        Assert.Equal("Alinhamento", servico.Descricao);
        Assert.Equal(45m, servico.Valor);
    }

    [Fact]
    public async Task StatusAtualCliente_DeveConsultarVeiculosEMapearOrdens()
    {
        var ordem = new OrdemServico(21);
        ordem.OSEmDiagnostico("Ana");
        var repository = new Mock<OrdemServicoRepository>();
        List<int>? veiculosConsultados = null;
        using var client = TestSupport.CreateHttpClient((_, _) =>
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"data":[21,22]}""",
                    Encoding.UTF8,
                    "application/json")
            });
        });
        repository.Setup(x => x.ListarOrdensServicoPorCliente(It.IsAny<List<int>>()))
            .Callback<List<int>>(ids => veiculosConsultados = ids)
            .ReturnsAsync(new List<OrdemServico> { ordem });
        var handler = new StatusAtualOSClienteHandler(
            repository.Object,
            client,
            TestSupport.CreateConfiguration("ClienteService", "http://cliente/").Object);

        var response = await handler.Handle(2);

        Assert.True(response.Success);
        Assert.Equal(new[] { 21, 22 }, veiculosConsultados!);
        var status = Assert.IsAssignableFrom<IEnumerable<StatusOSsClienteResponse>>(response.Data).Single();
        Assert.Equal("Em diagnóstico", status.Status);
        Assert.Equal(21, status.IdVeiculo);
        repository.Verify(x => x.ListarOrdensServicoPorCliente(It.IsAny<List<int>>()), Times.Once);
    }

    [Fact]
    public async Task TempoMedioExecucao_SemOrdensFinalizadas_DeveRetornarZero()
    {
        var repository = new Mock<OrdemServicoRepository>();
        repository.Setup(x => x.ListarOrdensServicoPorStatus(It.IsAny<IList<string>>()))
            .ReturnsAsync(new List<OrdemServico>());
        var handler = new TempoMedioExecucaoOSHandler(repository.Object);

        var result = await handler.Handle();

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task TempoMedioExecucao_ComOrdensFinalizadas_DeveRetornarMediaEmMinutos()
    {
        var ordemUm = new OrdemServico(1);
        var ordemDois = new OrdemServico(2);
        var inicio = DateTime.UtcNow;
        TestSupport.SetProperty(ordemUm, nameof(OrdemServico.DataInicioExecucao), inicio);
        TestSupport.SetProperty(ordemUm, nameof(OrdemServico.DataFimExecucao), inicio.AddMinutes(4));
        TestSupport.SetProperty(ordemDois, nameof(OrdemServico.DataInicioExecucao), inicio);
        TestSupport.SetProperty(ordemDois, nameof(OrdemServico.DataFimExecucao), inicio.AddMinutes(8));
        var repository = new Mock<OrdemServicoRepository>();
        repository.Setup(x => x.ListarOrdensServicoPorStatus(It.IsAny<IList<string>>()))
            .ReturnsAsync(new List<OrdemServico> { ordemUm, ordemDois });
        var handler = new TempoMedioExecucaoOSHandler(repository.Object);

        var result = await handler.Handle();

        Assert.Equal(6, result);
        repository.Verify(x => x.ListarOrdensServicoPorStatus(
            It.Is<IList<string>>(status => status.Contains("Finalizada") && status.Contains("Entregue"))), Times.Once);
    }

    private static HttpClient CreateSuccessfulVehicleClient() =>
        TestSupport.CreateHttpClient((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"data":{"existe":true}}""",
                    Encoding.UTF8,
                    "application/json")
            }));

    private static HttpClient CreateNoRequestHttpClient() =>
        TestSupport.CreateHttpClient((_, _) =>
            throw new InvalidOperationException("Não deveria chamar o serviço externo."));

    private static FinalizarDiagnosticoOSHandler CreateFinalizarDiagnosticoHandler(
        Mock<OrdemServicoRepository> ordemRepository,
        Mock<ServicoRepository> servicoRepository,
        Mock<ItemEstoqueRepository> itemRepository,
        HttpClient client) =>
        new(
            ordemRepository.Object,
            servicoRepository.Object,
            itemRepository.Object,
            client,
            TestSupport.CreateConfiguration("OrcamentoService", "http://orcamento/").Object);

    private static DiagnosticoFinalizadoRequest NewDiagnosticoRequest(int id) =>
        new()
        {
            Id = id,
            Observacao = "Diagnóstico",
            ItensEstoque = new List<AddItensOrdemServicoRequest>
            {
                new() { id = 0, quantidade = 2 }
            }
        };
}
