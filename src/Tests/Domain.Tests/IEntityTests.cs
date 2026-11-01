using Xunit;
using Domain.Entidades;

namespace Domain.Tests
{
    // Classe fake para poder testar IEntity
    public class FakeEntity : IEntity
    {
        public FakeEntity()
        {
            Ativo = true; // inicializa como ativo
        }
    }

    public class IEntityTests
    {
        [Fact]
        public void Construtor_DeveInicializarComoAtivo()
        {
            // Act
            var entidade = new FakeEntity();

            // Assert
            Assert.True(entidade.Ativo);
        }

        [Fact]
        public void Inativar_DeveDefinirAtivoComoFalse()
        {
            // Arrange
            var entidade = new FakeEntity();

            // Act
            entidade.Inativar();

            // Assert
            Assert.False(entidade.Ativo);
            Assert.False(entidade.EstaAtivo());
        }
    }
}
