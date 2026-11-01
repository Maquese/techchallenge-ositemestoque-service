
using Application.Models.Requests;
using Application.Models.Responses;
using Domain.Exceptions;
using Application.Interfaces;

namespace Application.UseCases.OrdensServico;

public class AtualizarServicoHandler
{
    private readonly ServicoRepository _servicoRepository;

    public AtualizarServicoHandler(ServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }
    
    public async Task<BaseResponse> Handle(UpdateServicoRequest servicoModel)
    {
        var servico = await _servicoRepository.ObterPorId(servicoModel.Id);
        if (servico == null)
        {
            throw new DomainException("Serviço não encontrado");
        }
        if(!servico.EstaAtivo())
            throw new DomainException("Servico inativo");

        servico.Atualizar(servicoModel.Descricao, servicoModel.Valor, servicoModel.TempoEstimado);

        await _servicoRepository.Atualizar(servico);

        return new BaseResponse
        {
            Message = "Atualizado com sucesso",
            Success = true,
            Data = servico.Id
        };
    }

}
