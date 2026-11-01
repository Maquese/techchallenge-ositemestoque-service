using System;
using System.Collections.Generic;
using Xunit;
using Domain.Aggregates;
using Domain.Entidades;

namespace Domain.Tests
{
    public class OrdemServicoTests
    {
        [Fact]
        public void Construtor_DeveInicializarCorretamente()
        {
            // Arrange
            int veiculoId = 1;

            // Act
            var ordem = new OrdemServico(veiculoId);

            // Assert
            Assert.Equal(veiculoId, ordem.VeiculoId);
            Assert.Equal("Recebida", ordem.Status);
            Assert.NotNull(ordem.Servicos);
            Assert.NotNull(ordem.OrdemServicoItensEstoque);
            Assert.True(ordem.Ativo);
            Assert.True(ordem.DataAbertura <= DateTime.Now);
            Assert.Null(ordem.DataFechamento);
        }

        [Fact]
        public void OSEmDiagnostico_DeveDefinirStatusEDadosCorretos()
        {
            var ordem = new OrdemServico(1);

            ordem.OSEmDiagnostico("Carlos");

            Assert.Equal("Em diagnóstico", ordem.Status);
            Assert.Equal("Carlos", ordem.MecanicoAtribuido);
        }

        [Fact]
        public void OSDiagnosticada_DeveDefinirItensEStatusCorretos()
        {
            var ordem = new OrdemServico(1);
            var itens = new List<OrdemServicoItemEstoque>
            {
                new OrdemServicoItemEstoque(1, 1, 2)
            };

            ordem.OSDiagnosticada(itens);

            Assert.Equal("Aguardando aprovação", ordem.Status);
            Assert.Equal(itens, ordem.OrdemServicoItensEstoque);
            Assert.Null(ordem.MecanicoAtribuido);
        }

        [Fact]
        public void OSAprovada_DeveDefinirStatusComoAprovada()
        {
            var ordem = new OrdemServico(1);

            ordem.OSAprovada();

            Assert.Equal("Aprovada", ordem.Status);
        }

        [Fact]
        public void OSNegada_DeveDefinirStatusComoNegada()
        {
            var ordem = new OrdemServico(1);

            ordem.OSNegada();

            Assert.Equal("Negada", ordem.Status);
        }

        [Fact]
        public void EmExecucao_DeveDefinirStatusEDadosCorretos()
        {
            var ordem = new OrdemServico(1);

            ordem.EmExecucao("João");

            Assert.Equal("Em execução", ordem.Status);
            Assert.Equal("João", ordem.MecanicoAtribuido);
            Assert.NotNull(ordem.DataInicioExecucao);
        }

        [Fact]
        public void FinalizarOrdemServico_DeveDefinirStatusEDadosCorretos()
        {
            var ordem = new OrdemServico(1);

            ordem.FinalizarOrdemServico();

            Assert.Equal("Finalizada", ordem.Status);
            Assert.NotNull(ordem.DataFimExecucao);
        }

        [Fact]
        public void OrdemServicoEntregue_DeveDefinirStatusEDadosCorretos()
        {
            var ordem = new OrdemServico(1);

            ordem.OrdemServicoEntregue();

            Assert.Equal("Entregue", ordem.Status);
            Assert.NotNull(ordem.DataFechamento);
        }

        [Fact]
        public void TempoMedioExecucao_SemDatas_DeveLancarExcecao()
        {
            var ordem = new OrdemServico(1);

            var exception = Assert.Throws<InvalidOperationException>(() => ordem.TempoMedioExecucao());

            Assert.Equal("Datas de início e fim da execução não estão definidas.", exception.Message);
        }

        [Fact]
        public void TempoMedioExecucao_ComInicioEFim_DeveRetornarMilissegundos()
        {
            var ordem = new OrdemServico(1);
            ordem.EmExecucao("Carlos");

            ordem.FinalizarOrdemServico();

            Assert.True(ordem.TempoMedioExecucao() >= 0);
        }

        [Fact]
        public void CalcularValorOrdemServico_DeveSomarServicosEItensDeEstoque()
        {
            var itemEstoque = new ItemEstoque("Peça", "Filtro", "Filtro de óleo", 12.5m, "Unidade");
            var itemOrdem = new OrdemServicoItemEstoque(1, 1, 3);
            typeof(OrdemServicoItemEstoque)
                .GetProperty(nameof(OrdemServicoItemEstoque.ItemEstoque))!
                .SetValue(itemOrdem, itemEstoque);
            var servicos = new List<Servico>
            {
                new Servico("Troca de óleo", 35m, 30),
                new Servico("Inspeção", 10m, 15)
            };
            var ordem = new OrdemServico(1, servicos);
            ordem.OSDiagnosticada(new List<OrdemServicoItemEstoque> { itemOrdem });

            Assert.Equal(82.5m, ordem.CalcularValorOrdemServico()!.Value);
        }

        [Fact]
        public void CalcularValorOrdemServico_SemServicosEItens_DeveRetornarZero()
        {
            var ordem = new OrdemServico(1);

            Assert.Equal(0m, ordem.CalcularValorOrdemServico()!.Value);
        }
    }
}
