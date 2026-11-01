using Domain.Exceptions;
using Domain.VOs;

namespace Domain.Tests;

public class ValueObjectTests
{
    [Theory]
    [InlineData("11987654321")]
    [InlineData("(11) 98765-4321")]
    public void CelularVO_ComOnzeDigitos_DeveAceitarNumero(string numero)
    {
        var celular = new CelularVO(numero);

        Assert.Equal(numero, celular.Numero);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("1198765432")]
    [InlineData("119876543210")]
    public void CelularVO_ComNumeroInvalido_DeveLancarDomainException(string numero)
    {
        Assert.Throws<DomainException>(() => new CelularVO(numero));
    }

    [Theory]
    [InlineData("529.982.247-25", "52998224725", "CPF")]
    [InlineData("04.252.011/0001-10", "04252011000110", "CNPJ")]
    public void DocumentoVO_ComDocumentoValido_DeveNormalizarNumero(
        string entrada,
        string numeroEsperado,
        string tipoEsperado)
    {
        var documento = new DocumentoVO(entrada);

        Assert.Equal(numeroEsperado, documento.Numero);
        Assert.Equal(tipoEsperado, documento.Tipo);
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("529.982.247-24")]
    [InlineData("11.111.111/1111-11")]
    [InlineData("04.252.011/0001-19")]
    [InlineData("123")]
    public void DocumentoVO_ComDocumentoInvalido_DeveLancarDomainException(string numero)
    {
        Assert.Throws<DomainException>(() => new DocumentoVO(numero));
    }

    [Fact]
    public void EmailVO_ComEnderecoValido_DevePreservarEndereco()
    {
        var email = new EmailVO("contato@example.com");

        Assert.Equal("contato@example.com", email.Endereco);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nao-e-email")]
    [InlineData("Nome <contato@example.com>")]
    public void EmailVO_ComEnderecoInvalido_DeveLancarDomainException(string endereco)
    {
        Assert.Throws<DomainException>(() => new EmailVO(endereco));
    }

    [Theory]
    [InlineData("abc-1234", "ABC-1234")]
    [InlineData("abc1d23", "ABC1D23")]
    [InlineData(" ABC-1234 ", "ABC-1234")]
    public void PlacaVO_ComFormatoValido_DeveNormalizar(string entrada, string esperado)
    {
        var placa = new PlacaVO(entrada);

        Assert.Equal(esperado, placa.Valor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AB-1234")]
    [InlineData("ABC1234")]
    [InlineData("ABC1D2")]
    public void PlacaVO_ComFormatoInvalido_DeveLancarDomainException(string entrada)
    {
        Assert.Throws<DomainException>(() => new PlacaVO(entrada));
    }

    [Fact]
    public void PlacaVO_Comparacao_DeveUsarValorNormalizado()
    {
        var placa = new PlacaVO("abc-1234");
        var mesmaPlaca = new PlacaVO("ABC-1234");
        var outraPlaca = new PlacaVO("XYZ-9876");

        Assert.True(placa.Equals(mesmaPlaca));
        Assert.Equal(placa.GetHashCode(), mesmaPlaca.GetHashCode());
        Assert.False(placa.Equals(outraPlaca));
        Assert.False(placa.Equals(null));
        Assert.False(placa.Equals("ABC-1234"));
    }
}
