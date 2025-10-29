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
                Mes = d.Mes,
                ValorPago = d.ValorPago
            });

            return Ok(dados);
        }

        [HttpGet("{matriculaSistel}/{matriculaAstel}/{ano}/{mes}")]
        public ActionResult<DadosFinanceirosDTO> GetById(long matriculaSistel, long matriculaAstel, int ano, double mes)
        {
            var dados = _service.GetById(matriculaSistel, matriculaAstel, ano, mes);
            if (dados == null)
                return NotFound();

            var dto = new DadosFinanceirosDTO
            {
                MatriculaSistel = dados.MatriculaSistel,
                MatriculaAstel = dados.MatriculaAstel,
                Ano = dados.Ano,
                Mes = dados.Mes,
                ValorPago = dados.ValorPago
            };

            return Ok(dto);
        }

        [HttpPost]
        public ActionResult<DadosFinanceirosDTO> Create([FromBody] DadosFinanceirosDTO dto)
        {
            try
            {
                var dados = new DadosFinanceiros
                {
                    MatriculaSistel = dto.MatriculaSistel,
                    MatriculaAstel = dto.MatriculaAstel,
                    Ano = dto.Ano,
                    Mes = dto.Mes,
                    ValorPago = dto.ValorPago
                };

                _service.Add(dados);

                return CreatedAtAction(nameof(GetById),
                    new
                    {
                        matriculaSistel = dados.MatriculaSistel,
                        matriculaAstel = dados.MatriculaAstel,
                        ano = dados.Ano,
                        mes = dados.Mes
                    },
                    dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    errors = new
                    {
                        MatriculaSistel = new[] { ex.Message }
                    }
                });
            }
        }

        [HttpDelete("{matriculaSistel}/{matriculaAstel}/{ano}/{mes}")]
        public IActionResult Delete(long matriculaSistel, long matriculaAstel, int ano, double mes)
        {
            var dados = _service.GetById(matriculaSistel, matriculaAstel, ano, mes);
            if (dados == null)
                return NotFound();

            _service.Delete(matriculaSistel, matriculaAstel, ano, mes);
            return NoContent();
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportCsv([FromServices] ImportFinanceiroService importService)
        {
            var bytes = await importService.ExportCsvAsync();
            return File(bytes, "text/csv", "dados_financeiros_export.csv");
        }

        [HttpGet("export-cadastrais")]
        public async Task<IActionResult> ExportCadastraisCsv([FromServices] ImportFinanceiroService importService)
        {
            var bytes = await importService.ExportCadastraisCsvAsync();
            return File(bytes, "text/csv", "dados_cadastrais_export.csv");
        }
    }
}
