using Application.Interfaces;
using Application.Models.Responses;
using Domain.Exceptions;

namespace Application.UseCases.OrdensServico;

public class BuscarServicoHandler
{
    private readonly ServicoRepository _servicoRepository;

    public BuscarServicoHandler(ServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    public async Task<BaseResponse> Handle(int id)
    {
        var servico = await _servicoRepository.ObterPorId(id);
        if (servico == null)
            throw new DomainException("Servico não encontrado");
        if(!servico.EstaAtivo())
            throw new DomainException("Servico inativo");

        return new BaseResponse
        {
           Message = "Buscado com sucesso",
           Success = true,
           Data = new ServicoResponse
            {
                Id = servico.Id,
                Descricao = servico.Descricao,
                Valor = servico.Valor,
                TempoEstimado = servico.TempoEstimado
            }
        };
    }
}
