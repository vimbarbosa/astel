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

        [HttpGet]
        public ActionResult<IEnumerable<DadosCadastraisDTO>> GetAll()
        {
            var dados = _service.GetAll().Select(d => new DadosCadastraisDTO
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
            });

            return Ok(dados);
        }

        [HttpGet("{matriculaSistel}")]
        public ActionResult<DadosCadastraisDTO> GetByMatriculaSistel(int matriculaSistel)
        {
            var dados = _service.GetByMatriculaSistel(matriculaSistel);
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
        public ActionResult<DadosCadastraisDTO> Create(DadosCadastraisDTO dto)
        {
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

            // ✅ Corrigido: CreatedAtAction aponta para o método GetByMatriculaSistel
            return CreatedAtAction(nameof(GetByMatriculaSistel),
                                   new { matriculaSistel = model.MatriculaSistel },
                                   dto);
        }

        [HttpPut("{matriculaSistel}")]
        public IActionResult Update(int matriculaSistel, DadosCadastraisDTO dto)
        {
            if (matriculaSistel != dto.MatriculaSistel)
                return BadRequest("Matrícula Sistel divergente.");

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

            _service.Update(model);
            return NoContent();
        }

        [HttpDelete("{matriculaSistel}")]
        public IActionResult Delete(int matriculaSistel)
        {
            var dados = _service.GetByMatriculaSistel(matriculaSistel);
            if (dados == null)
                return NotFound();

            _service.Delete(matriculaSistel);
            return NoContent();
        }
    }
}
