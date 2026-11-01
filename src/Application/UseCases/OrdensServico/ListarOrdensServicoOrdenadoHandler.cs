
using Application.Models.Responses;
using Application.Interfaces;
using static Application.Models.Responses.OrdensServicoOrdeandoResponse;
namespace Application.UseCases.OrdensServico;

public class ListarOrdensServicoOrdenadoHandler
{
    private readonly OrdemServicoRepository _ordemServicoRepository;

    public ListarOrdensServicoOrdenadoHandler(OrdemServicoRepository ordemServicoRepository)
    {
        _ordemServicoRepository = ordemServicoRepository;
    }

    public async Task<BaseResponse> Handle()
    {
        var ordensServico = await _ordemServicoRepository.ListarAtivos();
        var ordensEmExecucao = ordensServico.Where(o => o.Status == "Em execução").OrderBy(x => x.DataAbertura).ToList();
        var ordensAguardandoAprovacao = ordensServico.Where(o => o.Status == "Aguardando aprovação").OrderBy(x => x.DataAbertura).ToList();
        var onrdensEmDiagnostico = ordensServico.Where(o => o.Status == "Em diagnóstico").OrderBy(x => x.DataAbertura).ToList();
        var ordensAprovadas = ordensServico.Where(o => o.Status == "Recebida").OrderBy(x => x.DataAbertura).ToList();

        var data = new List<OrdensServicoOrdeandoResponse> { new OrdensServicoOrdeandoResponse {
            Status = "Em execução",
            quantidade = ordensEmExecucao.Count.ToString(),
            OrdemServico = ordensEmExecucao.Select(o => new OrdemServicoListResponse {ID = o.Id, Data = o.DataAbertura}).ToList()
        },
        new OrdensServicoOrdeandoResponse {
            Status = "Aguardando aprovação",
            quantidade = ordensAguardandoAprovacao.Count.ToString(),
            OrdemServico = ordensAguardandoAprovacao.Select(o => new OrdemServicoListResponse {ID = o.Id, Data = o.DataAbertura}).ToList()
        },
        new OrdensServicoOrdeandoResponse {
            Status = "Em diagnóstico",
            quantidade = onrdensEmDiagnostico.Count.ToString(),
            OrdemServico = onrdensEmDiagnostico.Select(o => new OrdemServicoListResponse {ID = o.Id, Data = o.DataAbertura}).ToList()
        },
        new OrdensServicoOrdeandoResponse {
            Status = "Recebida",
            quantidade = ordensAprovadas.Count.ToString(),
            OrdemServico = ordensAprovadas.Select(o => new OrdemServicoListResponse {ID = o.Id, Data = o.DataAbertura}).ToList()
        }};


        return new BaseResponse
        {
            Success = true,
            Message = "Ordens de serviço listadas com sucesso.",
            Data = data
        };
    }
}
