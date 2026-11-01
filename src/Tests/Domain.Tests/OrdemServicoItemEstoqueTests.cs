using System;
using Xunit;
using Domain.Entidades;

namespace Domain.Tests
{
    public class OrdemServicoItemEstoqueTests
    {
        [Fact]
        public void Construtor_DeveInicializarCorretamente()
        {
            // Arrange
            int ordemServicoId = 1;
            int itemEstoqueId = 2;
            int quantidade = 10;

            // Act
            var entidade = new OrdemServicoItemEstoque(ordemServicoId, itemEstoqueId, quantidade);

            // Assert
            Assert.Equal(ordemServicoId, entidade.OrdemServicoId);
            Assert.Equal(itemEstoqueId, entidade.ItemEstoqueId);
            Assert.Equal(quantidade, entidade.Quantidade);
            Assert.True(entidade.Ativo);
            Assert.True(entidade.DataCadastro <= DateTime.UtcNow);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Construtor_QuantidadeInvalida_DeveLancarExcecao(int quantidadeInvalida)
        {
            Assert.Throws<ArgumentException>(() =>
                new OrdemServicoItemEstoque(1, 2, quantidadeInvalida));
        }

        [Fact]
        public void AtualizarQuantidade_DeveAlterarValor()
        {
            // Arrange
            var entidade = new OrdemServicoItemEstoque(1, 2, 10);

            // Act
            entidade.AtualizarQuantidade(20);

            // Assert
            Assert.Equal(20, entidade.Quantidade);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public void AtualizarQuantidade_QuantidadeInvalida_DeveLancarExcecao(int quantidadeInvalida)
        {
            // Arrange
            var entidade = new OrdemServicoItemEstoque(1, 2, 10);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                entidade.AtualizarQuantidade(quantidadeInvalida));
        }
    }
}
