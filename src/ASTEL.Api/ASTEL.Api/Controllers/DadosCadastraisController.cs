using ASTEL.Api.DTOs;
using ASTEL.Api.Models;
using ASTEL.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASTEL.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DadosCadastraisController : ControllerBase
    {
        private readonly DadosCadastraisService _service;

        public DadosCadastraisController(DadosCadastraisService service)
        {
            _service = service;
        }

        // GET COM PAGINAÇÃO E FILTROS
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DadosCadastraisDTO>>> GetAll(
            string? nome = null,
            string? cpf = null,
            long? matriculaAstel = null,
            string? formapagamento = null,
            string? cidade = null,
            string? estado = null,
            bool? ativo = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("pageNumber e pageSize devem ser maiores que zero.");

            var (dados, totalCount) = await _service.GetPagedFilteredAsync(
                nome,
                cpf,
                matriculaAstel,
                formapagamento,
                cidade,
                estado,
                ativo,
                pageNumber,
                pageSize
            );

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var dtos = dados.Select(d => new DadosCadastraisDTO
            {
                Id = d.Id,
                MatriculaSistel = d.MatriculaSistel,
                MatriculaAstel = d.MatriculaAstel,
                Nome = d.Nome,
                Endereco = d.Endereco,
                Situacao = d.Situacao,
                ValorBeneficio = d.ValorBeneficio,
                EstadoCivil = d.EstadoCivil,
                Telefone = d.Telefone,
                NomeEsposa = d.NomeEsposa,
                CPF = d.CPF,
                RG = d.RG,
                Ativo = d.Ativo,
                DescontoFolha = d.DescontoFolha,

                // NOVOS CAMPOS
                Logradouro = d.Logradouro,
                CelSkype = d.CelSkype,
                Estado = d.Estado,
                Cidade = d.Cidade,
                TipoEndereco = d.TipoEndereco,
                Correspondencia = d.Correspondencia,
                Numero = d.Numero,
                Complemento = d.Complemento,
                Bairro = d.Bairro,
                Email = d.Email,
                CEP = d.CEP,
                FormaPagamento = d.FormaPagamento,

                // NOVOS CAMPOS DE DATA
                DataAlteracaoStatus = d.DataAlteracaoStatus,
                DataObto = d.DataObto,
                DataInadimplencia = d.DataInadimplencia,
                DataPedidoDesligamento = d.DataPedidoDesligamento,

                // NOVO CAMPO TIPO_VINCULO
                TipoVinculo = d.TipoVinculo
            });

            Response.Headers["X-Total-Count"] = totalCount.ToString();
            Response.Headers["X-Total-Pages"] = totalPages.ToString();
            Response.Headers["X-Current-Page"] = pageNumber.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();

            // Retorna também totalPages no corpo, semelhante ao DadosFinanceirosController
            return Ok(new
            {
                totalPages,
                items = dtos
            });
        }

        // GET BY ID
        [HttpGet("{id:long}")]
        public async Task<ActionResult<DadosCadastraisDTO>> GetById(long id)
        {
            var d = await _service.GetByIdAsync(id);
            if (d == null)
                return NotFound();

            var dto = new DadosCadastraisDTO
            {
                Id = d.Id,
                MatriculaSistel = d.MatriculaSistel,
                MatriculaAstel = d.MatriculaAstel,
                Nome = d.Nome,
                Endereco = d.Endereco,
                Situacao = d.Situacao,
                ValorBeneficio = d.ValorBeneficio,
                EstadoCivil = d.EstadoCivil,
                Telefone = d.Telefone,
                NomeEsposa = d.NomeEsposa,
                CPF = d.CPF,
                RG = d.RG,
                Ativo = d.Ativo,
                DescontoFolha = d.DescontoFolha,

                Logradouro = d.Logradouro,
                CelSkype = d.CelSkype,
                Estado = d.Estado,
                Cidade = d.Cidade,
                TipoEndereco = d.TipoEndereco,
                Correspondencia = d.Correspondencia,
                Numero = d.Numero,
                Complemento = d.Complemento,
                Bairro = d.Bairro,
                Email = d.Email,
                CEP = d.CEP,
                FormaPagamento = d.FormaPagamento,

                DataAlteracaoStatus = d.DataAlteracaoStatus,
                DataObto = d.DataObto,
                DataInadimplencia = d.DataInadimplencia,
                DataPedidoDesligamento = d.DataPedidoDesligamento,

                TipoVinculo = d.TipoVinculo
            };

            return Ok(dto);
        }

        // CREATE
        [HttpPost]
        public async Task<ActionResult<DadosCadastraisDTO>> Create([FromBody] DadosCadastraisDTO dto)
        {
            var model = new DadosCadastrais
            {
                Id = dto.MatriculaAstel.Value,
                MatriculaSistel = dto.MatriculaSistel,
                MatriculaAstel = dto.MatriculaAstel,
                Nome = dto.Nome,
                Endereco = dto.Endereco,
                Situacao = dto.Situacao,
                ValorBeneficio = dto.ValorBeneficio,
                EstadoCivil = dto.EstadoCivil,
                Telefone = dto.Telefone,
                NomeEsposa = dto.NomeEsposa,
                CPF = dto.CPF,
                RG = dto.RG,
                Ativo = string.Equals(dto.Situacao, "ATIVO", StringComparison.OrdinalIgnoreCase),
                DescontoFolha = dto.DescontoFolha,

                Logradouro = dto.Logradouro,
                CelSkype = dto.CelSkype,
                Estado = dto.Estado,
                Cidade = dto.Cidade,
                TipoEndereco = dto.TipoEndereco,
                Correspondencia = dto.Correspondencia,
                Numero = dto.Numero,
                Complemento = dto.Complemento,
                Bairro = dto.Bairro,
                Email = dto.Email,
                CEP = dto.CEP,
                FormaPagamento = dto.FormaPagamento,

                DataAlteracaoStatus = dto.DataAlteracaoStatus,
                DataObto = dto.DataObto,
                DataInadimplencia = dto.DataInadimplencia,
                DataPedidoDesligamento = dto.DataPedidoDesligamento,

                TipoVinculo = dto.TipoVinculo
            };

            await _service.AddAsync(model);

            dto.Id = model.Id;

            return CreatedAtAction(nameof(GetById), new { id = model.Id }, dto);
        }

        // UPDATE
        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] DadosCadastraisDTO dto)
        {
            if (dto.Id != id)
                return BadRequest("ID do corpo difere do ID da rota.");

            var existente = await _service.GetByIdAsync(id);
            if (existente == null)
                return NotFound();

            existente.MatriculaSistel = dto.MatriculaSistel;
            existente.MatriculaAstel = dto.MatriculaAstel;
            existente.Nome = dto.Nome;
            existente.Endereco = dto.Endereco;
            existente.Situacao = dto.Situacao;
            existente.ValorBeneficio = dto.ValorBeneficio;
            existente.EstadoCivil = dto.EstadoCivil;
            existente.Telefone = dto.Telefone;
            existente.NomeEsposa = dto.NomeEsposa;
            existente.CPF = dto.CPF;
            existente.RG = dto.RG;
            existente.Ativo = string.Equals(dto.Situacao, "ATIVO", StringComparison.OrdinalIgnoreCase);
            existente.DescontoFolha = dto.DescontoFolha;

            // NOVOS CAMPOS
            existente.Logradouro = dto.Logradouro;
            existente.CelSkype = dto.CelSkype;
            existente.Estado = dto.Estado;
            existente.Cidade = dto.Cidade;
            existente.TipoEndereco = dto.TipoEndereco;
            existente.Correspondencia = dto.Correspondencia;
            existente.Numero = dto.Numero;
            existente.Complemento = dto.Complemento;
            existente.Bairro = dto.Bairro;
            existente.Email = dto.Email;
            existente.CEP = dto.CEP;
            existente.FormaPagamento = dto.FormaPagamento;

            existente.DataAlteracaoStatus = dto.DataAlteracaoStatus;
            existente.DataObto = dto.DataObto;
            existente.DataInadimplencia = dto.DataInadimplencia;
            existente.DataPedidoDesligamento = dto.DataPedidoDesligamento;

            existente.TipoVinculo = dto.TipoVinculo;

            await _service.UpdateAsync(existente);

            return Ok(new { message = "Registro atualizado com sucesso!" });
        }

        // DELETE
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok)
                return NotFound();

            return NoContent();
        }

        // AUTocomplete - Busca nomes para autocomplete
        [HttpGet("autocomplete")]
        public async Task<ActionResult<IEnumerable<AutocompleteNomeDTO>>> AutocompleteNomes(
            [FromQuery] string? termo = null,
            [FromQuery] bool? ativo = null,
            [FromQuery] int limit = 10)
        {
            if (limit <= 0 || limit > 50)
                limit = 10; // Limita entre 1 e 50 resultados

            var resultados = await _service.SearchNomesAsync(termo, ativo, limit);

            var dtos = resultados.Select(r => new AutocompleteNomeDTO
            {
                Id = r.Id,
                Nome = r.Nome,
                MatriculaAstel = r.MatriculaAstel
            });

            return Ok(dtos);
        }
    }
}
