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
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("pageNumber e pageSize devem ser maiores que zero.");

            var (dados, totalCount) = await _service.GetPagedFilteredAsync(
                nome,
                cpf,
                matriculaAstel,
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
                CEP = d.CEP
            });

            Response.Headers["X-Total-Count"] = totalCount.ToString();
            Response.Headers["X-Total-Pages"] = totalPages.ToString();
            Response.Headers["X-Current-Page"] = pageNumber.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();

            return Ok(dtos);
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
                CEP = d.CEP
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
                Ativo = dto.Ativo,
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
                CEP = dto.CEP
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
            existente.Ativo = dto.Ativo;
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
    }
}
