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
    public class DadosFinanceirosController : ControllerBase
    {
        private readonly DadosFinanceirosService _service;

        public DadosFinanceirosController(DadosFinanceirosService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<DadosFinanceirosDTO>> GetAll()
        {
            var dados = _service.GetAll().Select(d => new DadosFinanceirosDTO
            {
                MatriculaSistel = d.MatriculaSistel,
                MatriculaAstel = d.MatriculaAstel,
                Ano = d.Ano,
                Mes = d.Mes
            });
            return Ok(dados);
        }

        [HttpGet("{matriculaSistel}/{matriculaAstel}/{ano}")]
        public ActionResult<DadosFinanceirosDTO> GetById(int matriculaSistel, int matriculaAstel, int ano)
        {
            var dados = _service.GetById(matriculaSistel, matriculaAstel, ano);
            if (dados == null)
            {
                return NotFound();
            }
            var dadosDTO = new DadosFinanceirosDTO
            {
                MatriculaSistel = dados.MatriculaSistel,
                MatriculaAstel = dados.MatriculaAstel,
                Ano = dados.Ano,
                Mes = dados.Mes
            };
            return Ok(dadosDTO);
        }

        [HttpPost]
        public ActionResult<DadosFinanceirosDTO> Create(DadosFinanceirosDTO dadosDTO)
        {
            var dados = new DadosFinanceiros
            {
                MatriculaSistel = dadosDTO.MatriculaSistel,
                MatriculaAstel = dadosDTO.MatriculaAstel,
                Ano = dadosDTO.Ano,
                Mes = dadosDTO.Mes
            };
            _service.Add(dados);
            return CreatedAtAction(nameof(GetById), new { matriculaSistel = dados.MatriculaSistel, matriculaAstel = dados.MatriculaAstel, ano = dados.Ano }, dadosDTO);
        }

        [HttpPut("{matriculaSistel}/{matriculaAstel}/{ano}")]
        public IActionResult Update(int matriculaSistel, int matriculaAstel, int ano, DadosFinanceirosDTO dadosDTO)
        {
            if (matriculaSistel != dadosDTO.MatriculaSistel || matriculaAstel != dadosDTO.MatriculaAstel || ano != dadosDTO.Ano)
            {
                return BadRequest();
            }

            var dados = new DadosFinanceiros
            {
                MatriculaSistel = dadosDTO.MatriculaSistel,
                MatriculaAstel = dadosDTO.MatriculaAstel,
                Ano = dadosDTO.Ano,
                Mes = dadosDTO.Mes
            };

            _service.Update(dados);
            return NoContent();
        }

        [HttpDelete("{matriculaSistel}/{matriculaAstel}/{ano}")]
        public IActionResult Delete(int matriculaSistel, int matriculaAstel, int ano)
        {
            var dados = _service.GetById(matriculaSistel, matriculaAstel, ano);
            if (dados == null)
            {
                return NotFound();
            }

            _service.Delete(matriculaSistel, matriculaAstel, ano);
            return NoContent();
        }
    }
}