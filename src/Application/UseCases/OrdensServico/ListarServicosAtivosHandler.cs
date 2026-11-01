using Application.Interfaces;
using Application.Models.Responses;

namespace Application.UseCases.OrdensServico;

public class ListarServicosAtivosHandler
{
    private readonly ServicoRepository _servicoRepository;

    public ListarServicosAtivosHandler(ServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }
    public async Task<BaseResponse> Handle()
    {
        var servicos = await _servicoRepository.ListarAtivos();
        return new BaseResponse
        {
          Message = "Ok",
          Success = true,
          Data =  servicos.Select(s => new ServicoResponse
            {
                Id = s.Id,
                Descricao = s.Descricao,
                Valor = s.Valor,
                TempoEstimado = s.TempoEstimado
            }).ToList()
        };
    }
}
