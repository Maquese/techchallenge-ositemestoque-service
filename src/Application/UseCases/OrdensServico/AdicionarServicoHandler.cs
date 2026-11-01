using Application.Interfaces;
using Application.Models.Requests;
using Application.Models.Responses;
using Domain.Entidades;

namespace Application.UseCases.OrdensServico;

public class AdicionarServicoHandler
{
    private readonly ServicoRepository _servicoRepository;

    public AdicionarServicoHandler(ServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository; 
    }
     public async Task<BaseResponse> Handle(AddServicoRequest servico)
    {
        var servicoEntity = new Servico
        (
            descricao: servico.Descricao,
            valor: servico.Valor,
            tempoEstimado: servico.TempoEstimado
        );
        
        await _servicoRepository.Adicionar(servicoEntity);
        return new BaseResponse
        {
            Message = "Adicionado com sucesso",
            Data = servicoEntity.Id,
            Success = true
        };
    }
}
