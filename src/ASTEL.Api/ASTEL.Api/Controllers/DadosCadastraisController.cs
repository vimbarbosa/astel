using ASTEL.Api.DTOs;
using ASTEL.Api.Models;
using ASTEL.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Retorna todos os registros de dados cadastrais.
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<DadosCadastraisDTO>> GetAll()
        {
            var dados = _service.GetAll()
                .Select(d => new DadosCadastraisDTO
                {
                    MatriculaSistel = d.MatriculaSistel,
                    MatriculaAstel = d.MatriculaAstel,
                    Nome = d.Nome,
                    Endereco = d.Endereco,
                    Situacao = d.Situacao,
                    ValorBeneficio = (float?)d.ValorBeneficio,
                    EstadoCivil = d.EstadoCivil,
                    Telefone = d.Telefone,
                    NomeEsposa = d.NomeEsposa,
                    CPF = d.CPF,
                    RG = d.RG,
                    Ativo = d.Ativo,
                    DescontoFolha = d.DescontoFolha
                })
                .ToList();

            return Ok(dados);
        }

        /// <summary>
        /// Retorna um registro específico pela matrícula Sistel.
        /// </summary>
        [HttpGet("{matriculaSistel:long}")]
        public ActionResult<DadosCadastraisDTO> GetByMatriculaSistel(long matriculaSistel)
        {
            var dados = _service.GetById(matriculaSistel);
            if (dados == null)
                return NotFound();

            var dto = new DadosCadastraisDTO
            {
                MatriculaSistel = dados.MatriculaSistel,
                MatriculaAstel = dados.MatriculaAstel,
                Nome = dados.Nome,
                Endereco = dados.Endereco,
                Situacao = dados.Situacao,
                ValorBeneficio = (float?)dados.ValorBeneficio,
                EstadoCivil = dados.EstadoCivil,
                Telefone = dados.Telefone,
                NomeEsposa = dados.NomeEsposa,
                CPF = dados.CPF,
                RG = dados.RG,
                Ativo = dados.Ativo,
                DescontoFolha = dados.DescontoFolha
            };

            return Ok(dto);
        }

        [HttpPost]
        public ActionResult<DadosCadastraisDTO> Create([FromBody] DadosCadastraisDTO dto)
        {
            // A validação FluentValidation roda automaticamente antes dessa linha
            var model = new DadosCadastrais
            {
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
                DescontoFolha = dto.DescontoFolha
            };

            _service.Add(model);

            return CreatedAtAction(nameof(GetByMatriculaSistel), new { matriculaSistel = model.MatriculaSistel }, dto);
        }


        /// <summary>
        /// Atualiza um registro existente.
        /// </summary>
        [HttpPut("{matriculaSistel:long}")]
        public IActionResult Update(long matriculaSistel, [FromBody] DadosCadastraisDTO dto)
        {
            // Verifica se o corpo é nulo ou a rota não corresponde ao DTO
            if (dto == null || matriculaSistel != dto.MatriculaSistel)
                return BadRequest(new
                {
                    errors = new
                    {
                        MatriculaSistel = new[] { "Matrícula Sistel divergente ou dados inválidos." }
                    }
                });

            // Caso o registro não exista
            var existente = _service.GetById(matriculaSistel);
            if (existente == null)
                return NotFound(new { message = $"Registro com matrícula {matriculaSistel} não encontrado." });

            // Se chegou aqui, o FluentValidation já garantiu que dto está válido.
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

            _service.Update(existente);

            return Ok(new { message = "Registro atualizado com sucesso!" });
        }


        /// <summary>
        /// Exclui um registro de dados cadastrais.
        /// </summary>
        [HttpDelete("{matriculaSistel:long}")]
        public IActionResult Delete(long matriculaSistel)
        {
            var existente = _service.GetById(matriculaSistel);
            if (existente == null)
                return NotFound();

            _service.Delete(matriculaSistel);
            return NoContent();
        }
    }
}
