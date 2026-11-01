using System;
using Xunit;
using Domain.Aggregates;

namespace Domain.Tests
{
    public class ItemEstoqueTests
    {
        [Fact]
        public void Construtor_DeveInicializarCorretamente()
        {
            // Arrange
            string tipo = "Peça";
            string nome = "Filtro de óleo";
            string descricao = "Filtro para motor";
            decimal valor = 50m;
            string unidadeMedida = "Unidade";

            // Act
            var item = new ItemEstoque(tipo, nome, descricao, valor, unidadeMedida);

            // Assert
            Assert.Equal(tipo, item.Tipo);
            Assert.Equal(nome, item.Nome);
            Assert.Equal(descricao, item.Descricao);
            Assert.Equal(valor, item.Valor);
            Assert.Equal(unidadeMedida, item.UnidadeMedida);
            Assert.True(item.Ativo);
            Assert.Equal(0, item.QuantidadeEmEstoque);
            Assert.True(item.DataCadastro <= DateTime.UtcNow);
        }

        [Theory]
        [InlineData("", "desc", 10, "Peça", "Unidade")]
        [InlineData("Peça", "", 10, "Peça", "Unidade")]
        [InlineData("Peça", "desc", -1, "Peça", "Unidade")]
        [InlineData("Peça", "desc", 10, "Outro", "Unidade")]
        [InlineData("Peça", "desc", 10, "Peça", "")]
        public void Construtor_ParametrosInvalidos_DeveLancarExcecao(
            string nome, string descricao, decimal valor, string tipo, string unidadeMedida)
        {
            Assert.Throws<ArgumentException>(() =>
                new ItemEstoque(tipo, nome, descricao, valor, unidadeMedida));
        }

        [Fact]
        public void Atualizar_DeveAlterarValoresCorretamente()
        {
            var item = new ItemEstoque("Peça", "Filtro", "Filtro motor", 50m, "Unidade");

            item.Atualizar("Pneu", "Pneu aro 15", 200m, "Insumo", "Unidade");

            Assert.Equal("Pneu", item.Nome);
            Assert.Equal("Pneu aro 15", item.Descricao);
            Assert.Equal(200m, item.Valor);
            Assert.Equal("Insumo", item.Tipo);
            Assert.Equal("Unidade", item.UnidadeMedida);
        }

        [Fact]
        public void Construtor_ComValidade_DeveArmazenarData()
        {
            var validade = new DateTime(2027, 12, 31);

            var item = new ItemEstoque("Peça", "Filtro", "Filtro motor", 50m, "Unidade", validade);

            Assert.Equal(validade, item.Datavalidade);
        }

        [Theory]
        [InlineData("", "descrição", 10, "Peça", "Unidade")]
        [InlineData("Filtro", "", 10, "Peça", "Unidade")]
        [InlineData("Filtro", "descrição", -1, "Peça", "Unidade")]
        [InlineData("Filtro", "descrição", 10, "Outro", "Unidade")]
        [InlineData("Filtro", "descrição", 10, "Peça", "")]
        public void Atualizar_ComDadosInvalidos_DeveLancarArgumentException(
            string nome, string descricao, decimal valor, string tipo, string unidadeMedida)
        {
            var item = new ItemEstoque("Peça", "Filtro", "Filtro motor", 50m, "Unidade");

            Assert.Throws<ArgumentException>(() =>
                item.Atualizar(nome, descricao, valor, tipo, unidadeMedida));
        }

        [Fact]
        public void Inativar_DeveDefinirAtivoComoFalse()
        {
            var item = new ItemEstoque("Peça", "Filtro", "Filtro motor", 50m, "Unidade");

            item.Inativar();

            Assert.False(item.Ativo);
        }

        [Fact]
        public void Ativar_DeveDefinirAtivoComoTrue()
        {
            var item = new ItemEstoque("Peça", "Filtro", "Filtro motor", 50m, "Unidade");
            item.Inativar();

            item.Ativar();

            Assert.True(item.Ativo);
        }

        [Fact]
        public void AdicionarQuantidadeEstoque_DeveIncrementarCorretamente()
        {
            var item = new ItemEstoque("Peça", "Filtro", "Filtro motor", 50m, "Unidade");

            item.AdicionarQuantidadeEstoque(10);

            Assert.Equal(10, item.QuantidadeEmEstoque);
        }

        [Fact]
        public void AdicionarQuantidadeEstoque_QuantidadeNegativa_DeveLancarExcecao()
        {
            var item = new ItemEstoque("Peça", "Filtro", "Filtro motor", 50m, "Unidade");

            Assert.Throws<ArgumentException>(() => item.AdicionarQuantidadeEstoque(-5));
        }

        [Fact]
        public void DeduzirQuantidadeEstoque_DeveReduzirCorretamente()
        {
            var item = new ItemEstoque("Peça", "Filtro", "Filtro motor", 50m, "Unidade");
            item.AdicionarQuantidadeEstoque(10);

            item.DeduzirQuantidadeEstoque(5);

            Assert.Equal(5, item.QuantidadeEmEstoque);
        }

        [Fact]
        public void DeduzirQuantidadeEstoque_QuantidadeNegativa_DeveLancarExcecao()
        {
            var item = new ItemEstoque("Peça", "Filtro", "Filtro motor", 50m, "Unidade");

            Assert.Throws<ArgumentException>(() => item.DeduzirQuantidadeEstoque(-3));
        }

        [Fact]
        public void DeduzirQuantidadeEstoque_QuantidadeMaiorQueEstoque_DeveLancarExcecao()
        {
            var item = new ItemEstoque("Peça", "Filtro", "Filtro motor", 50m, "Unidade");
            item.AdicionarQuantidadeEstoque(5);

            Assert.Throws<InvalidOperationException>(() => item.DeduzirQuantidadeEstoque(10));
        }
    }
}
