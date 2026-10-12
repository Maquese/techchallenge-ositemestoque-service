using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.UseCases.ItensEstoque;
using Application.Models.Requests;
using Application.UseCases.ItensEstoque;

namespace AutoReparaAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    // [Authorize]
    public class ItemEstoqueController : ControllerBase
    {
        public ItemEstoqueController()
        {
            
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarItemEstoque([FromServices]AdicionarItemEstoqueHandler itemEstoqueAppService, [FromBody] AddItemEstoqueRequest itemEstoqueModel)
        {
            return Ok(await itemEstoqueAppService.Handle(itemEstoqueModel));
        }

        [HttpGet]
        public async Task<IActionResult> ListarItensEstoque([FromServices]ListarItemEstoqueHandler itemEstoqueAppService)
        {
            return Ok(await itemEstoqueAppService.Handle());
        }

         [HttpGet]
        public async Task<IActionResult> ObterItemEstoque([FromServices]ObterItemEstoqueHandler itemEstoqueAppService,[FromQuery] int id)
        {
            return Ok(await itemEstoqueAppService.Handle(id));
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarItemEstoque([FromServices]AtualizarItemEstoqueHandler itemEstoqueAppService, [FromBody] UpdateItemEstoqueRequest itemEstoqueModel)
        {
            return Ok( await itemEstoqueAppService.Handle(itemEstoqueModel));
        }

        [HttpDelete]
        public async Task<IActionResult> InativarItemEstoque([FromServices]InativarItemEstoqueHandler itemEstoqueAppService, [FromQuery] int id)
        {
            return Ok(await itemEstoqueAppService.Handle(id));
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarQuantidadeEstoque([FromServices]AdicionarQtdEstoqueItemEstoqueHandler itemEstoqueAppService, [FromBody] AddQuantidadeItemEstoqueRequest adicionarQuantidadeModel)
        {
            
            return Ok(await itemEstoqueAppService.Handle(adicionarQuantidadeModel));
        }
    }
}
