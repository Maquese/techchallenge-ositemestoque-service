
using Application.Models.Responses;
using Domain.Exceptions;
using Application.Interfaces;

namespace Application.UseCases.OrdensServico;

public class ConsutaStatusOSHandler
{
    private readonly OrdemServicoRepository _ordemServicoRepository;

    public ConsutaStatusOSHandler(OrdemServicoRepository ordemServicoRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
    }

    public async Task<BaseResponse> Handle(int ordemServicoId)
    {
        var ordemServico = await _ordemServicoRepository.ObterPorId(ordemServicoId);
        if (ordemServico == null)
        {
            throw new DomainException("Ordem de serviço não encontrada.");
        }

        return new BaseResponse
        {
            Success = true,
            Message = "Status da ordem de serviço obtido com sucesso.",
            Data = new
            {
                OrdemServicoId = ordemServico.Id,
                Status = ordemServico.Status.ToString()
            }
        };
    }
}
