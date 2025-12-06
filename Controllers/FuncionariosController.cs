using Microsoft.AspNetCore.Mvc;
using ApiEmpresas.Models;
using ApiEmpresas.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace ApiEmpresas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuncionariosController : ControllerBase
    {
        private readonly IRepository<Funcionario> _repository;
        private readonly ILogger<FuncionariosController> _logger;

        public FuncionariosController(
            IRepository<Funcionario> repository, 
            ILogger<FuncionariosController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // GET: api/Funcionarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Funcionario>>> Get()
        {
            try
            {
                _logger.LogInformation("Iniciando consulta de todos os funcionários");
                var funcionarios = await _repository.GetAllAsync();
                
                _logger.LogInformation($"Retornados {funcionarios?.Count() ?? 0} funcionários");
                return Ok(funcionarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar funcionários");
                return StatusCode(500, new 
                { 
                    message = "Erro interno ao buscar funcionários",
                    detalhes = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // GET: api/Funcionarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Funcionario>> Get(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning($"ID inválido recebido: {id}");
                    return BadRequest(new { message = "ID deve ser maior que zero" });
                }

                _logger.LogInformation($"Consultando funcionário ID: {id}");
                var funcionario = await _repository.GetAsync(id);
                
                if (funcionario == null)
                {
                    _logger.LogWarning($"Funcionário ID: {id} não encontrado");
                    return NotFound(new 
                    { 
                        message = $"Funcionário com ID {id} não encontrado",
                        sugestao = "Verifique se o ID está correto"
                    });
                }

                return Ok(funcionario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao consultar funcionário ID: {id}");
                return StatusCode(500, new 
                { 
                    message = $"Erro ao buscar funcionário {id}",
                    detalhes = ex.Message
                });
            }
        }

        // POST: api/Funcionarios
        [HttpPost]
        public async Task<ActionResult<Funcionario>> Post([FromBody] Funcionario funcionario)
        {
            try
            {
                _logger.LogInformation("Recebida requisição POST para criar funcionário");
                
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido: {@Errors}", 
                        ModelState.Values.SelectMany(v => v.Errors));
                    
                    return BadRequest(new 
                    { 
                        message = "Dados inválidos",
                        erros = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                    });
                }

                // Validações adicionais específicas para seu modelo
                if (funcionario.Salario < 0)
                {
                    return BadRequest(new 
                    { 
                        message = "Salário não pode ser negativo",
                        campo = "salario",
                        valorRecebido = funcionario.Salario
                    });
                }

                if (funcionario.EmpresaId <= 0)
                {
                    return BadRequest(new 
                    { 
                        message = "EmpresaId deve ser maior que zero",
                        campo = "empresaId",
                        valorRecebido = funcionario.EmpresaId
                    });
                }

                // Evitar referência cíclica no JSON
                funcionario.Empresa = null;
                
                _logger.LogInformation($"Criando funcionário: {funcionario.Nome}, Cargo: {funcionario.Cargo}, Salário: {funcionario.Salario:C}");
                var novoFuncionario = await _repository.AddAsync(funcionario);
                
                _logger.LogInformation($"Funcionário criado com ID: {novoFuncionario.Id}");
                
                return CreatedAtAction(
                    nameof(Get), 
                    new { id = novoFuncionario.Id }, 
                    new 
                    {
                        message = "Funcionário criado com sucesso",
                        funcionario = new 
                        {
                            id = novoFuncionario.Id,
                            nome = novoFuncionario.Nome,
                            cargo = novoFuncionario.Cargo,
                            salario = novoFuncionario.Salario,
                            empresaId = novoFuncionario.EmpresaId
                        },
                        location = $"/api/funcionarios/{novoFuncionario.Id}"
                    });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao criar funcionário");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operação inválida ao criar funcionário");
                return Conflict(new 
                { 
                    message = "Conflito ao criar funcionário",
                    detalhes = ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao criar funcionário");
                return StatusCode(500, new 
                { 
                    message = "Erro interno ao criar funcionário",
                    detalhes = ex.Message
                });
            }
        }

        // PUT: api/Funcionarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Funcionario funcionario)
        {
            try
            {
                _logger.LogInformation($"Recebida requisição PUT para funcionário ID: {id}");
                
                if (id != funcionario.Id)
                {
                    _logger.LogWarning($"ID mismatch: URL={id}, Body={funcionario.Id}");
                    return BadRequest(new 
                    { 
                        message = "ID da URL não corresponde ao ID do objeto enviado.",
                        idUrl = id,
                        idBody = funcionario.Id
                    });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState inválido no PUT");
                    var erros = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);
                    
                    return BadRequest(new 
                    { 
                        message = "Dados inválidos para atualização",
                        erros = erros
                    });
                }

                // Validação de salário
                if (funcionario.Salario < 0)
                {
                    return BadRequest(new 
                    { 
                        message = "Salário não pode ser negativo",
                        campo = "salario",
                        valorRecebido = funcionario.Salario
                    });
                }

                // Carregar do banco primeiro 
                var existente = await _repository.GetAsync(id);
                if (existente == null)
                {
                    _logger.LogWarning($"Tentativa de atualizar funcionário inexistente ID: {id}");
                    return NotFound(new 
                    { 
                        message = $"Funcionário com ID {id} não encontrado para atualização" 
                    });
                }

                // Atualiza manualmente os campos conforme SEU MODELO
                existente.Nome = funcionario.Nome;
                existente.Cargo = funcionario.Cargo;
                existente.Salario = funcionario.Salario;
                existente.EmpresaId = funcionario.EmpresaId;
                
                // Manter referência nula para evitar problemas de serialização
                existente.Empresa = null;

                _logger.LogInformation($"Atualizando funcionário ID: {id} - {funcionario.Nome}");
                await _repository.UpdateAsync(existente);
                
                _logger.LogInformation($"Funcionário ID: {id} atualizado com sucesso");
                
                return Ok(new 
                { 
                    message = "Funcionário atualizado com sucesso",
                    funcionario = new 
                    {
                        id = existente.Id,
                        nome = existente.Nome,
                        cargo = existente.Cargo,
                        salario = existente.Salario,
                        empresaId = existente.EmpresaId
                    }
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Funcionário não encontrado para atualização ID: {id}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar funcionário ID: {id}");
                return StatusCode(500, new 
                { 
                    message = "Erro interno ao atualizar funcionário",
                    detalhes = ex.Message
                });
            }
        }

        // DELETE: api/Funcionarios/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "ID inválido" });
                }

                _logger.LogInformation($"Recebida requisição DELETE para funcionário ID: {id}");
                
                // Verificar se funcionário existe antes de deletar
                var funcionario = await _repository.GetAsync(id);
                if (funcionario == null)
                {
                    _logger.LogWarning($"Tentativa de excluir funcionário inexistente ID: {id}");
                    return NotFound(new 
                    { 
                        message = $"Funcionário com ID {id} não encontrado para exclusão",
                        timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogInformation($"Excluindo funcionário ID: {id} - Nome: {funcionario.Nome}, Cargo: {funcionario.Cargo}");
                var deletado = await _repository.DeleteAsync(id);

                if (!deletado)
                {
                    _logger.LogError($"Falha ao excluir funcionário ID: {id} - repositório retornou false");
                    return StatusCode(500, new 
                    { 
                        message = "Falha ao excluir funcionário no banco de dados" 
                    });
                }

                _logger.LogInformation($"Funcionário ID: {id} excluído com sucesso");
                
                return Ok(new 
                { 
                    message = $"Funcionário '{funcionario.Nome}' excluído com sucesso",
                    detalhes = new 
                    {
                        id = id,
                        nome = funcionario.Nome,
                        cargo = funcionario.Cargo,
                        empresaId = funcionario.EmpresaId
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao excluir funcionário ID: {id}");
                return StatusCode(500, new 
                { 
                    message = "Erro interno ao excluir funcionário",
                    detalhes = ex.Message
                });
            }
        }

        // GET: api/Funcionarios/empresa/{empresaId}
        [HttpGet("empresa/{empresaId}")]
        public async Task<ActionResult<IEnumerable<Funcionario>>> GetByEmpresa(int empresaId)
        {
            try
            {
                if (empresaId <= 0)
                {
                    return BadRequest(new { message = "ID da empresa inválido" });
                }

                _logger.LogInformation($"Consultando funcionários da empresa ID: {empresaId}");
                
                // Esta funcionalidade depende de seu repositório
                // Se não tiver, pode implementar ou remover este endpoint
                var todosFuncionarios = await _repository.GetAllAsync();
                var funcionariosDaEmpresa = todosFuncionarios
                    .Where(f => f.EmpresaId == empresaId)
                    .ToList();
                
                _logger.LogInformation($"Encontrados {funcionariosDaEmpresa.Count} funcionários na empresa {empresaId}");
                
                return Ok(new 
                {
                    empresaId = empresaId,
                    totalFuncionarios = funcionariosDaEmpresa.Count,
                    funcionarios = funcionariosDaEmpresa
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao consultar funcionários da empresa {empresaId}");
                return StatusCode(500, new 
                { 
                    message = "Erro ao buscar funcionários da empresa",
                    detalhes = ex.Message
                });
            }
        }
    }
}