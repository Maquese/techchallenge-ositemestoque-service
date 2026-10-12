
// using Application.Models.Requests;
// using Application.Models.Responses;
// using Application.UseCases.OrdensServico;
// using Domain.Exceptions;
// using Application.Interfaces;
// using Domain.Aggregates;

// namespace Application.Controllers;

// public class OrdemServicoAppController
// {
//     private readonly OrdemServicoRepository _ordemSerivicoRepository;
//     private readonly VeiculoRepository _veiculoRepository;
//     private readonly ClienteRepository _clienteRepository;
//     private readonly AdicionarClienteHandler _adicionarClienteHandler;
//     private readonly AdicionarVeiculoClienteHandler _adicionarVeiculoClienteHandler;
//     private readonly AdicionarOrdemServicoHandler _adicionarOrdemServicoHandler;

//     public OrdemServicoAppController(AdicionarClienteHandler adicionarClienteHandler, AdicionarVeiculoClienteHandler adicionarVeiculoClienteHandler,
//                                      AdicionarOrdemServicoHandler adicionarOrdemServicoHandler ,OrdemServicoRepository ordemServicoRepository,
//                                      VeiculoRepository veiculoRepository, ClienteRepository clienteRepository)
//     {
//         _ordemSerivicoRepository = ordemServicoRepository;
//         _veiculoRepository = veiculoRepository;
//         _clienteRepository = clienteRepository;
//         _adicionarClienteHandler = adicionarClienteHandler;
//         _adicionarVeiculoClienteHandler = adicionarVeiculoClienteHandler;
//         _adicionarOrdemServicoHandler = adicionarOrdemServicoHandler;
//     }

//     public async Task<BaseResponse> AbrirOrdemServico(AberturaOSRequest request)
//     {
//         int clienteId = 0;
//         int veiculoId = 0;
//         int ordemServicoId = 0;
//         try{
//             var cliente = await _adicionarClienteHandler.Handle(new AddClienteRequest
//             {
//                 Documento = request.Documento,
//                 Nome = request.Nome,
//                 Email = request.Email,
//                 Celular = request.Celular
//             });
//             clienteId = (int)cliente.Data;
//             if((int)cliente.Data <= 0)
//             {
//                 throw new DomainException("Erro ao criar cliente.");
//             }

//             var dataVeiculo = await _adicionarVeiculoClienteHandler.Handle(new AddVeiculoRequest
//             {
//                 Placa = request.Placa,
//                 Modelo = request.Modelo,
//                 Marca = request.Marca,
//                 Ano = request.Ano,
//                 ClienteId = (int)cliente.Data
//             });

//             veiculoId = (int)dataVeiculo.Data;

//             if((int)dataVeiculo.Data <= 0)
//             {
//                 throw new DomainException("Erro ao criar veículo.");
//             } 

//             var dataOrdemServico = await _adicionarOrdemServicoHandler.Handle(new AddOrdemServicoRequest
//             {
//                 ServicosIds = request.ServicosIds,
//                 VeiculoId = (int)dataVeiculo.Data   
//             });       
//              ordemServicoId = (int)dataOrdemServico.Data;

//             if((int)dataOrdemServico.Data <= 0)
//             {
//                 throw new DomainException("Erro ao criar ordem de serviço.");
//             }

//             return dataOrdemServico;
//         }catch(Exception ex)
//         {
//             RemoverCasoHajaErro(clienteId,veiculoId,ordemServicoId);
//             throw ex;
//         }
//     }

//     private async void RemoverCasoHajaErro(int clienteId, int veiculoId, int ordemServicoId)
//     {
//         if(ordemServicoId != 0)
//           await _ordemSerivicoRepository.Remover(await _ordemSerivicoRepository.ObterPorId(ordemServicoId));
        
//         if(veiculoId != 0)
//           await _veiculoRepository.Remover(await _veiculoRepository.ObterPorId(veiculoId));

//         if(clienteId != 0)
//           await _clienteRepository.Remover(await _clienteRepository.ObterPorId(clienteId));
//     }
// }
