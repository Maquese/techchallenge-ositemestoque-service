using Application.Models.Requests;
using Application.Models.Responses;
using Application.Services;

namespace Application.Controllers;

public class OrdemServicoAppController
{
    private readonly AbrirOrdemServicoSagaService _abrirOrdemServicoSagaService;

    public OrdemServicoAppController(AbrirOrdemServicoSagaService abrirOrdemServicoSagaService)
    {
        _abrirOrdemServicoSagaService = abrirOrdemServicoSagaService;
    }

    public Task<BaseResponse> AbrirOrdemServico(
        AberturaOSRequest request,
        CancellationToken cancellationToken = default)
    {
        return _abrirOrdemServicoSagaService.Handle(request, cancellationToken);
    }
}
