using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.UseCases.OrdensServico;
using Application.Models.Requests;
//using Application.Controllers;

namespace AutoReparaAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    // [Authorize]
    public class OrdemServicoController : ControllerBase
    {
        public OrdemServicoController()
        {
            
        }

        [HttpPost]
        public async Task<IActionResult> CriarOrdemServico([FromServices]AdicionarOrdemServicoHandler adicionarOrdemServicoHandler, [FromBody] AddOrdemServicoRequest ordemServicoModel)
        {
            var id = await adicionarOrdemServicoHandler.Handle(ordemServicoModel);
            return Ok(id);
        }       

        [HttpPost]
        public async Task<IActionResult> AtribuirMecanicoEmDiagnostico([FromServices]AtribuirMecanicoDiagnosticoOSHandler atribuirMecanicoEmDiagnosticoHandler, [FromBody] AtribuiMecanicoRequest atribuiEmDiagnostico)
        {
            return Ok(await atribuirMecanicoEmDiagnosticoHandler.Handle(atribuiEmDiagnostico));
        }


        [HttpPost]
        public async Task<IActionResult> DiagnosticoFInalizado([FromServices]FinalizarDiagnosticoOSHandler diagnosticoFinalizadoHandler, [FromBody] DiagnosticoFinalizadoRequest diagnosticoFinalizadoModel)
        {
            return Ok(await diagnosticoFinalizadoHandler.Handle(diagnosticoFinalizadoModel));
        }


        [HttpPost]
        public async Task<IActionResult> AtribuirMecanicoEmExecucao([FromServices]AtribuirMecanicoExecucaoOSHandler atribuirMecanicoExecucaoOSHandler, [FromBody] AtribuiMecanicoRequest atribuiEmExecucao)
        {            
            return Ok(await atribuirMecanicoExecucaoOSHandler.Handle(atribuiEmExecucao));
        }

        [HttpPost]
        public async Task<IActionResult> FinalizarOrdemServico([FromServices]FinalizarOrdemServicoHandler finalizarOrdemServicoHandler, [FromBody] int ordemServicoId)
        {
            return Ok( await finalizarOrdemServicoHandler.Handle(ordemServicoId));
        }


        ///esse endpoint n é mais possivel dessa forma
        [HttpGet]
        public async Task<IActionResult> StatusAtualOrdensServicoCliente([FromServices]StatusAtualOSClienteHandler statusAtualOrdensServicoClienteHandler, [FromQuery] int clienteId)
        {
            return Ok(await statusAtualOrdensServicoClienteHandler.Handle(clienteId));
        }
        
        [HttpGet]
        public async Task<IActionResult> TempoMedioExecucaoMinutos([FromServices]TempoMedioExecucaoOSHandler tempoMedioExecucaoOSHandler)
        {
            var tempoMedio = await tempoMedioExecucaoOSHandler.Handle();
            return Ok(tempoMedio);
        }

        [HttpGet]
        public async Task<IActionResult> ListarOrdensServicoPorStatus([FromServices]ListarOrdensServicoOrdenadoHandler listarOrdensServicoPorStatusHandler)
        {
            var ordensServico = await listarOrdensServicoPorStatusHandler.Handle();
            return Ok(ordensServico);
        }

        
        // [HttpPost]
        // public  async Task<IActionResult> AberturaOS([FromServices]OrdemServicoAppController ordemServicoAppController,[FromBody] AberturaOSRequest request)
        // {
        //     var response = await ordemServicoAppController.AbrirOrdemServico(request);
        //     return Ok(response);
        // }
        

        [HttpGet]
        public async Task<IActionResult> ConsultaStatusOrdemServico([FromServices]ConsutaStatusOSHandler consultaStatusOSHandler, [FromQuery]int ordemServicoId)
        {
            var status = await consultaStatusOSHandler.Handle(ordemServicoId);
            return Ok(status);
        }

        


        #region Serviços 
        [HttpPost]
        public async Task<IActionResult> AdicionarServico([FromServices]AdicionarServicoHandler adicionarServicoOSHandler, [FromBody] AddServicoRequest servicoModel)
        {
            var id = await adicionarServicoOSHandler.Handle(servicoModel);
            return Ok(id);
        }

        [HttpGet]
        public async Task<IActionResult> BuscarServico([FromServices]BuscarServicoHandler buscarServicoHandler, [FromQuery]int id)
        {
            var servico = await buscarServicoHandler.Handle(id);
            return Ok(servico);
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarServico([FromServices]AtualizarServicoHandler atualizarServicoHandler, [FromBody] UpdateServicoRequest servicoModel)
        {
            
            return Ok(await atualizarServicoHandler.Handle(servicoModel));
        }

        [HttpDelete]
        public async Task<IActionResult> InativarServico([FromServices]InativarServicoHandler inativarServicoHandler, [FromQuery]int id)
        {
           
            return Ok( await inativarServicoHandler.Handle(id));
        }

        [HttpGet]
        public async Task<IActionResult> ListarServicosAtivos([FromServices]ListarServicosAtivosHandler listarServicosAtivosHandler)
        {
            var servicos = await listarServicosAtivosHandler.Handle();
            return Ok(servicos);
        }

        #endregion
    }
}
