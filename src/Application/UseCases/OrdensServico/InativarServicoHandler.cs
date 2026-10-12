using Application.Interfaces;
using Application.Models.Responses;
using Domain.Exceptions;

namespace Application.UseCases.OrdensServico;

public class InativarServicoHandler
{
    private readonly ServicoRepository _servicoRepository;

    public InativarServicoHandler(ServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    public async Task<BaseResponse> Handle(int id)
    {
        var servico = await _servicoRepository.ObterPorId(id);
        if (servico == null)
        {
            throw new DomainException("Serviço não encontrado");
        }

        await _servicoRepository.Inativar(servico);
        return new BaseResponse
        {
            Message = "Inativado com sucesso",
            Success = true,
            Data = servico.Id
        };
    }
}
