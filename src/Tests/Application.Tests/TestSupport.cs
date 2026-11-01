using Domain.Aggregates;
using Domain.Entidades;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Application.Tests;

internal static class TestSupport
{
    public static ItemEstoque CreateItem(decimal valor = 10m) =>
        new("Peça", "Filtro", "Filtro de óleo", valor, "Unidade");

    public static Servico CreateServico(string descricao = "Revisão", decimal valor = 25m) =>
        new(descricao, valor, 30);

    public static Mock<IConfiguration> CreateConfiguration(string sectionName, string value)
    {
        var section = new Mock<IConfigurationSection>();
        section.SetupGet(x => x.Value).Returns(value);

        var configuration = new Mock<IConfiguration>();
        configuration.Setup(x => x.GetSection(sectionName)).Returns(section.Object);
        return configuration;
    }

    public static HttpClient CreateHttpClient(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) =>
        new(new StubHttpMessageHandler(sendAsync));

    public static void SetProperty<TTarget, TValue>(TTarget target, string propertyName, TValue value)
        where TTarget : class =>
        typeof(TTarget).GetProperty(propertyName)!.SetValue(target, value);
}

internal sealed class StubHttpMessageHandler(
    Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) =>
        sendAsync(request, cancellationToken);
}
