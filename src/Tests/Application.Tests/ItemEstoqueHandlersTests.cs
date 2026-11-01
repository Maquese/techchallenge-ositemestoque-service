using Application.Interfaces;
using Application.Models.Requests;
using Application.Models.Responses;
using Application.UseCases.ItensEstoque;
using Domain.Aggregates;
using Domain.Exceptions;
using Moq;

namespace Application.Tests;

public class ItemEstoqueHandlersTests
{
    [Fact]
    public async Task AdicionarItemEstoque_DevePersistirItemEConfirmarSucesso()
    {
        var repository = new Mock<ItemEstoqueRepository>();
        repository.Setup(x => x.Adicionar(It.IsAny<ItemEstoque>())).Returns(Task.CompletedTask);
        var handler = new AdicionarItemEstoqueHandler(repository.Object);

        var response = await handler.Handle(new AddItemEstoqueRequest
        {
            Tipo = "Peça",
            Nome = "Filtro",
            Descricao = "Filtro de óleo",
            Valor = 42m,
            UnidadeMedida = "Unidade"
        });

        Assert.True(response.Success);
        Assert.Equal("Item de estoque adicionado com sucesso", response.Message);
        repository.Verify(x => x.Adicionar(It.Is<ItemEstoque>(item =>
            item.Nome == "Filtro" && item.Valor == 42m)), Times.Once);
    }

    [Fact]
    public async Task AdicionarItemEstoque_ComDadosInvalidos_DevePropagarExcecaoDeDominio()
    {
        var repository = new Mock<ItemEstoqueRepository>();
        var handler = new AdicionarItemEstoqueHandler(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(new AddItemEstoqueRequest
        {
            Tipo = "Invalido",
            Nome = "Filtro",
            Descricao = "Filtro de óleo",
            Valor = 42m,
            UnidadeMedida = "Unidade"
        }));

        repository.Verify(x => x.Adicionar(It.IsAny<ItemEstoque>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarQuantidade_ItemNaoEncontrado_DeveLancarDomainException()
    {
        var repository = new Mock<ItemEstoqueRepository>();
        repository.Setup(x => x.ObterPorId(3)).ReturnsAsync((ItemEstoque)null!);
        var handler = new AdicionarQtdEstoqueItemEstoqueHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new AddQuantidadeItemEstoqueRequest { Id = 3, Quantidade = 2 }));

        repository.Verify(x => x.Atualizar(It.IsAny<ItemEstoque>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarQuantidade_ItemInativo_DeveLancarDomainException()
    {
        var item = TestSupport.CreateItem();
        item.Inativar();
        var repository = new Mock<ItemEstoqueRepository>();
        repository.Setup(x => x.ObterPorId(3)).ReturnsAsync(item);
        var handler = new AdicionarQtdEstoqueItemEstoqueHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new AddQuantidadeItemEstoqueRequest { Id = 3, Quantidade = 2 }));

        repository.Verify(x => x.Atualizar(It.IsAny<ItemEstoque>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarQuantidade_ItemAtivo_DeveIncrementarEAtualizar()
    {
        var item = TestSupport.CreateItem();
        var repository = new Mock<ItemEstoqueRepository>();
        repository.Setup(x => x.ObterPorId(3)).ReturnsAsync(item);
        repository.Setup(x => x.Atualizar(item)).Returns(Task.CompletedTask);
        var handler = new AdicionarQtdEstoqueItemEstoqueHandler(repository.Object);

        var response = await handler.Handle(new AddQuantidadeItemEstoqueRequest { Id = 3, Quantidade = 4 });

        Assert.True(response.Success);
        Assert.Equal("Quantidade ajustada com sucesso", response.Message);
        Assert.Equal(4, item.QuantidadeEmEstoque);
        repository.Verify(x => x.Atualizar(item), Times.Once);
    }

    [Fact]
    public async Task AtualizarItem_ItemNaoEncontrado_DeveLancarDomainException()
    {
        var repository = new Mock<ItemEstoqueRepository>();
        repository.Setup(x => x.ObterPorId(4)).ReturnsAsync((ItemEstoque)null!);
        var handler = new AtualizarItemEstoqueHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new UpdateItemEstoqueRequest { Id = 4 }));

        repository.Verify(x => x.Atualizar(It.IsAny<ItemEstoque>()), Times.Never);
    }

    [Fact]
    public async Task AtualizarItem_ItemInativo_DeveLancarDomainException()
    {
        var item = TestSupport.CreateItem();
        item.Inativar();
        var repository = new Mock<ItemEstoqueRepository>();
        repository.Setup(x => x.ObterPorId(4)).ReturnsAsync(item);
        var handler = new AtualizarItemEstoqueHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.Handle(new UpdateItemEstoqueRequest { Id = 4 }));

        repository.Verify(x => x.Atualizar(It.IsAny<ItemEstoque>()), Times.Never);
    }

    [Fact]
    public async Task AtualizarItem_ItemAtivo_DeveAlterarDadosEPersistir()
    {
        var item = TestSupport.CreateItem();
        var repository = new Mock<ItemEstoqueRepository>();
        repository.Setup(x => x.ObterPorId(4)).ReturnsAsync(item);
        repository.Setup(x => x.Atualizar(item)).Returns(Task.CompletedTask);
        var handler = new AtualizarItemEstoqueHandler(repository.Object);

        var response = await handler.Handle(new UpdateItemEstoqueRequest
        {
            Id = 4,
            Nome = "Pastilha",
            Descricao = "Pastilha de freio",
            Valor = 80m,
            Tipo = "Insumo",
            UnidadeMedida = "Caixa"
        });

        Assert.True(response.Success);
        Assert.Equal("Atualizado com sucesso", response.Message);
        Assert.Equal("Pastilha", item.Nome);
        Assert.Equal(80m, item.Valor);
        repository.Verify(x => x.Atualizar(item), Times.Once);
    }

    [Fact]
    public async Task InativarItem_ItemNaoEncontrado_DeveLancarDomainException()
    {
        var repository = new Mock<ItemEstoqueRepository>();
        repository.Setup(x => x.ObterPorId(5)).ReturnsAsync((ItemEstoque)null!);
        var handler = new InativarItemEstoqueHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(5));

        repository.Verify(x => x.Atualizar(It.IsAny<ItemEstoque>()), Times.Never);
    }

    [Fact]
    public async Task InativarItem_ItemEncontrado_DeveInativarEPersistir()
    {
        var item = TestSupport.CreateItem();
        var repository = new Mock<ItemEstoqueRepository>();
        repository.Setup(x => x.ObterPorId(5)).ReturnsAsync(item);
        repository.Setup(x => x.Atualizar(item)).Returns(Task.CompletedTask);
        var handler = new InativarItemEstoqueHandler(repository.Object);

        var response = await handler.Handle(5);

        Assert.True(response.Success);
        Assert.False(item.EstaAtivo());
        repository.Verify(x => x.Atualizar(item), Times.Once);
    }

    [Fact]
    public async Task ListarItens_DeveMapearItensAtivos()
    {
        var repository = new Mock<ItemEstoqueRepository>();
        repository.Setup(x => x.ListarAtivos()).ReturnsAsync(new List<ItemEstoque>
        {
            TestSupport.CreateItem(17m)
        });
        var handler = new ListarItemEstoqueHandler(repository.Object);

        var response = await handler.Handle();

        Assert.True(response.Success);
        var items = Assert.IsAssignableFrom<IEnumerable<ItemEstoqueResponse>>(response.Data);
        var item = Assert.Single(items);
        Assert.Equal("Filtro", item.Nome);
        Assert.Equal(17m, item.Valor);
    }

    [Fact]
    public async Task ObterItem_ItemNaoEncontrado_DeveLancarDomainException()
    {
        var repository = new Mock<ItemEstoqueRepository>();
        repository.Setup(x => x.ObterPorId(6)).ReturnsAsync((ItemEstoque)null!);
        var handler = new ObterItemEstoqueHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(6));
    }

    [Fact]
    public async Task ObterItem_ItemInativo_DeveLancarDomainException()
    {
        var item = TestSupport.CreateItem();
        item.Inativar();
        var repository = new Mock<ItemEstoqueRepository>();
        repository.Setup(x => x.ObterPorId(6)).ReturnsAsync(item);
        var handler = new ObterItemEstoqueHandler(repository.Object);

        await Assert.ThrowsAsync<DomainException>(() => handler.Handle(6));
    }

    [Fact]
    public async Task ObterItem_ItemAtivo_DeveRetornarDados()
    {
        var item = TestSupport.CreateItem(23m);
        item.AdicionarQuantidadeEstoque(7);
        var repository = new Mock<ItemEstoqueRepository>();
        repository.Setup(x => x.ObterPorId(6)).ReturnsAsync(item);
        var handler = new ObterItemEstoqueHandler(repository.Object);

        var response = await handler.Handle(6);

        Assert.True(response.Success);
        var result = Assert.IsType<ItemEstoqueResponse>(response.Data);
        Assert.Equal("Filtro", result.Nome);
        Assert.Equal(23m, result.Valor);
        Assert.Equal(7, result.Quantidade);
    }
}
