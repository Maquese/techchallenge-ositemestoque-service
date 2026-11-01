using Domain.Entidades;
using Domain.Exceptions;

namespace Domain.Tests;

public class ServicoTests
{
    [Fact]
    public void Construtor_DeveInicializarServicoAtivo()
    {
        var servico = new Servico("Troca de óleo", 125m, 45);

        Assert.Equal("Troca de óleo", servico.Descricao);
        Assert.Equal(125m, servico.Valor);
        Assert.Equal(45, servico.TempoEstimado);
        Assert.True(servico.EstaAtivo());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Construtor_ComValorNaoPositivo_DeveLancarDomainException(decimal valor)
    {
        var exception = Assert.Throws<DomainException>(() => new Servico("Reparo", valor, 30));

        Assert.Equal("Valor do serviço deve ser maior que zero.", exception.Message);
    }

    [Fact]
    public void Atualizar_DeveSubstituirDadosDoServico()
    {
        var servico = new Servico("Troca de óleo", 125m, 45);

        servico.Atualizar("Revisão", 250m, 90);

        Assert.Equal("Revisão", servico.Descricao);
        Assert.Equal(250m, servico.Valor);
        Assert.Equal(90, servico.TempoEstimado);
    }
}
