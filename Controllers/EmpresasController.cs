using Microsoft.AspNetCore.Mvc;
using ApiEmpresas.Models;
using ApiEmpresas.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ApiEmpresas.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpresasController : ControllerBase
    {
        private readonly IRepository<Empresa> _repository;
        private readonly ILogger<EmpresasController> _logger;

        public EmpresasController(
            IRepository<Empresa> repository, 
            ILogger<EmpresasController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // GET: api/Empresas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Empresa>>> Get()
        {
            try
            {
                _logger.LogInformation("Iniciando consulta de todas as empresas");
                var empresas = await _repository.GetAllAsync();
                
                _logger.LogInformation($"Retornadas {empresas?.Count() ?? 0} empresas");
                return Ok(empresas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar empresas");
                return StatusCode(500, new 
                { 
                    message = "Erro interno ao buscar empresas",
                    detalhes = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        // GET: api/Empresas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Empresa>> Get(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning($"ID inválido recebido: {id}");
                    return BadRequest(new { message = "ID deve ser maior que zero" });
                }

                _logger.LogInformation($"Consultando empresa ID: {id}");
                var empresa = await _repository.GetAsync(id);
                
                if (empresa == null)
                {
                    _logger.LogWarning($"Empresa ID: {id} não encontrada");
                    return NotFound(new 
                    { 
                        message = $"Empresa com ID {id} não encontrada",
                        sugestao = "Verifique se o ID está correto"
                    });
                }

                return Ok(empresa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao consultar empresa ID: {id}");
                return StatusCode(500, new 
                { 
                    message = $"Erro ao buscar empresa {id}",
                    detalhes = ex.Message
                });
            }
        }

        // POST: api/Empresas
        [HttpPost]
        public async Task<ActionResult<Empresa>> Post([FromBody] Empresa empresa)
        {
            try
            {
                _logger.LogInformation("Recebida requisição POST para criar empresa");
                
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

                // Validação adicional do CNPJ
                if (!ValidarCNPJ(empresa.CNPJ))
                {
                    return BadRequest(new 
                    { 
                        message = "CNPJ inválido",
                        campo = "cnpj",
                        valorRecebido = empresa.CNPJ,
                        formatoEsperado = "14 dígitos numéricos (com ou sem máscara)"
                    });
                }

                // Validação de CNPJ único (deve ser implementada no repositório)
                // Se seu repositório não faz essa validação, pode adicionar aqui:
                /*
                var cnpjExistente = await _repository.GetAllAsync();
                if (cnpjExistente.Any(e => e.CNPJ == empresa.CNPJ))
                {
                    return Conflict(new 
                    { 
                        message = "CNPJ já cadastrado no sistema",
                        cnpj = empresa.CNPJ
                    });
                }
                */

                _logger.LogInformation($"Criando empresa: {empresa.Nome}, CNPJ: {FormatarCNPJ(empresa.CNPJ)}");
                var novaEmpresa = await _repository.AddAsync(empresa);
                
                _logger.LogInformation($"Empresa criada com ID: {novaEmpresa.Id}");
                
                return CreatedAtAction(
                    nameof(Get), 
                    new { id = novaEmpresa.Id }, 
                    new 
                    {
                        message = "Empresa criada com sucesso",
                        empresa = new 
                        {
                            id = novaEmpresa.Id,
                            nome = novaEmpresa.Nome,
                            cnpj = FormatarCNPJ(novaEmpresa.CNPJ),
                            totalFuncionarios = novaEmpresa.Funcionarios?.Count ?? 0
                        },
                        location = $"/api/empresas/{novaEmpresa.Id}"
                    });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Erro de validação ao criar empresa");
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operação inválida ao criar empresa");
                return Conflict(new 
                { 
                    message = "Conflito ao criar empresa",
                    detalhes = ex.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao criar empresa");
                return StatusCode(500, new 
                { 
                    message = "Erro interno ao criar empresa",
                    detalhes = ex.Message
                });
            }
        }

        // PUT: api/Empresas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Empresa empresa)
        {
            try
            {
                _logger.LogInformation($"Recebida requisição PUT para empresa ID: {id}");
                
                if (id != empresa.Id)
                {
                    _logger.LogWarning($"ID mismatch: URL={id}, Body={empresa.Id}");
                    return BadRequest(new 
                    { 
                        message = "ID da URL não corresponde ao ID do objeto enviado.",
                        idUrl = id,
                        idBody = empresa.Id
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

                // Validação do CNPJ
                if (!ValidarCNPJ(empresa.CNPJ))
                {
                    return BadRequest(new 
                    { 
                        message = "CNPJ inválido",
                        campo = "cnpj",
                        valorRecebido = empresa.CNPJ
                    });
                }

                // Carregar do banco primeiro
                var existente = await _repository.GetAsync(id);
                if (existente == null)
                {
                    _logger.LogWarning($"Tentativa de atualizar empresa inexistente ID: {id}");
                    return NotFound(new 
                    { 
                        message = $"Empresa com ID {id} não encontrada para atualização" 
                    });
                }

                // Verificar se CNPJ já existe em outra empresa
                /*
                var empresas = await _repository.GetAllAsync();
                var cnpjDuplicado = empresas.Any(e => e.CNPJ == empresa.CNPJ && e.Id != id);
                if (cnpjDuplicado)
                {
                    return Conflict(new 
                    { 
                        message = "CNPJ já está em uso por outra empresa",
                        cnpj = empresa.CNPJ
                    });
                }
                */

                // Atualiza manualmente os campos
                existente.Nome = empresa.Nome;
                existente.CNPJ = empresa.CNPJ;
                // Funcionarios não é atualizado aqui (mantém os existentes)

                _logger.LogInformation($"Atualizando empresa ID: {id} - {empresa.Nome}");
                await _repository.UpdateAsync(existente);
                
                _logger.LogInformation($"Empresa ID: {id} atualizada com sucesso");
                
                return Ok(new 
                { 
                    message = "Empresa atualizada com sucesso",
                    empresa = new 
                    {
                        id = existente.Id,
                        nome = existente.Nome,
                        cnpj = FormatarCNPJ(existente.CNPJ),
                        totalFuncionarios = existente.Funcionarios?.Count ?? 0
                    }
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, $"Empresa não encontrada para atualização ID: {id}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar empresa ID: {id}");
                return StatusCode(500, new 
                { 
                    message = "Erro interno ao atualizar empresa",
                    detalhes = ex.Message
                });
            }
        }

        // DELETE: api/Empresas/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "ID inválido" });
                }

                _logger.LogInformation($"Recebida requisição DELETE para empresa ID: {id}");
                
                // Verificar se empresa existe antes de deletar
                var empresa = await _repository.GetAsync(id);
                if (empresa == null)
                {
                    _logger.LogWarning($"Tentativa de excluir empresa inexistente ID: {id}");
                    return NotFound(new 
                    { 
                        message = $"Empresa com ID {id} não encontrada para exclusão",
                        timestamp = DateTime.UtcNow
                    });
                }

                // Verificar se empresa tem funcionários
                if (empresa.Funcionarios != null && empresa.Funcionarios.Any())
                {
                    _logger.LogWarning($"Tentativa de excluir empresa com funcionários ID: {id}");
                    return Conflict(new 
                    { 
                        message = "Não é possível excluir empresa que possui funcionários",
                        detalhes = new 
                        {
                            empresa = empresa.Nome,
                            cnpj = FormatarCNPJ(empresa.CNPJ),
                            totalFuncionarios = empresa.Funcionarios.Count,
                            funcionarios = empresa.Funcionarios.Select(f => new 
                            { 
                                id = f.Id, 
                                nome = f.Nome, 
                                cargo = f.Cargo 
                            }).Take(5) // Mostra apenas os primeiros 5
                        },
                        sugestao = "Exclua ou transfira os funcionários antes de excluir a empresa"
                    });
                }

                _logger.LogInformation($"Excluindo empresa ID: {id} - Nome: {empresa.Nome}, CNPJ: {FormatarCNPJ(empresa.CNPJ)}");
                var deletado = await _repository.DeleteAsync(id);

                if (!deletado)
                {
                    _logger.LogError($"Falha ao excluir empresa ID: {id} - repositório retornou false");
                    return StatusCode(500, new 
                    { 
                        message = "Falha ao excluir empresa no banco de dados" 
                    });
                }

                _logger.LogInformation($"Empresa ID: {id} excluída com sucesso");
                
                return Ok(new 
                { 
                    message = $"Empresa '{empresa.Nome}' excluída com sucesso",
                    detalhes = new 
                    {
                        id = id,
                        nome = empresa.Nome,
                        cnpj = FormatarCNPJ(empresa.CNPJ)
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao excluir empresa ID: {id}");
                return StatusCode(500, new 
                { 
                    message = "Erro interno ao excluir empresa",
                    detalhes = ex.Message
                });
            }
        }

        // GET: api/Empresas/{id}/funcionarios
        [HttpGet("{id}/funcionarios")]
        public async Task<ActionResult<IEnumerable<Funcionario>>> GetFuncionarios(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "ID da empresa inválido" });
                }

                _logger.LogInformation($"Consultando funcionários da empresa ID: {id}");
                
                var empresa = await _repository.GetAsync(id);
                if (empresa == null)
                {
                    return NotFound(new 
                    { 
                        message = $"Empresa com ID {id} não encontrada" 
                    });
                }

                var funcionarios = empresa.Funcionarios?.ToList() ?? new List<Funcionario>();
                
                _logger.LogInformation($"Encontrados {funcionarios.Count} funcionários na empresa {empresa.Nome}");
                
                return Ok(new 
                {
                    empresaId = id,
                    empresaNome = empresa.Nome,
                    totalFuncionarios = funcionarios.Count,
                    funcionarios = funcionarios.Select(f => new 
                    {
                        id = f.Id,
                        nome = f.Nome,
                        cargo = f.Cargo,
                        salario = f.Salario
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao consultar funcionários da empresa {id}");
                return StatusCode(500, new 
                { 
                    message = "Erro ao buscar funcionários da empresa",
                    detalhes = ex.Message
                });
            }
        }

        // MÉTODOS AUXILIARES PARA CNPJ

        /// <summary>
        /// Valida se o CNPJ tem formato válido (14 dígitos)
        /// </summary>
        private bool ValidarCNPJ(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return false;

            // Remove caracteres não numéricos
            cnpj = Regex.Replace(cnpj, @"[^\d]", "");
            
            // Verifica se tem 14 dígitos
            if (cnpj.Length != 14)
                return false;

            // Verifica se não é uma sequência de números iguais
            if (cnpj.All(c => c == cnpj[0]))
                return false;

            // Aqui poderia implementar validação dos dígitos verificadores
            // Por enquanto, apenas valida o formato básico
            return true;
        }

        /// <summary>
        /// Formata o CNPJ para exibição: 00.000.000/0000-00
        /// </summary>
        private string FormatarCNPJ(string cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return cnpj;

            // Remove caracteres não numéricos
            cnpj = Regex.Replace(cnpj, @"[^\d]", "");
            
            if (cnpj.Length != 14)
                return cnpj; // Retorna original se não tiver 14 dígitos

            return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
        }
    }
}