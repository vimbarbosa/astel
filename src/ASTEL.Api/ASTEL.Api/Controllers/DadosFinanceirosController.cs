using ASTEL.Api.DTOs;
using ASTEL.Api.Models;
using ASTEL.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<DadosFinanceirosDTO>>> Filtrar(
             DateTime? dataInicio = null,
             DateTime? dataFim = null,
             string? nome = null,
             string? cpf = null,
             long? matriculaAstel = null,
             bool? inadimplente = null,
             int pageNumber = 1,
             int pageSize = 10)
        {
               var (dados, totalCount) = await _service.GetFilteredAsync(
                dataInicio, dataFim, nome, cpf, matriculaAstel, inadimplente,
                pageNumber, pageSize);

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            Response.Headers["X-Total-Count"] = totalCount.ToString();
            Response.Headers["X-Page-Number"] = pageNumber.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();
            Response.Headers["X-Total-Pages"] = totalPages.ToString();

            return Ok(dados);
        }



        // ------------------------------------------------------------
        // CRUD POR ID
        // ------------------------------------------------------------
        [HttpGet("{id}")]
        public ActionResult<DadosFinanceirosDTO> GetById(long id)
        {
            var fin = _service.GetById(id);
            if (fin == null)
                return NotFound();

            return Ok(fin);
        }

        [HttpPost]
        public IActionResult Create([FromBody] DadosFinanceiros df)
        {
            try
            {
                // Normaliza mês e ano
                df.Mes = Convert.ToInt32(df.Mes);
                df.Ano = Convert.ToInt32(df.Ano);

                _service.Add(df);

                return Ok(new { message = "Pagamento cadastrado com sucesso!" });
            }
            catch (DbUpdateException ex)
            {
                // SqlException 2627 = Duplicate PK
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                {
                    return Conflict(new
                    {
                        message = "Já existe um pagamento cadastrado para esta matrícula neste mês/ano."
                    });
                }

                return StatusCode(500, new
                {
                    message = "Erro ao salvar os dados financeiros.",
                    detail = ex.InnerException?.Message ?? ex.Message
                });
            }
        }



        [HttpPut("{id}")]
        public IActionResult Update(long id, [FromBody] DadosFinanceiros df)
        {
            df.Id = id;
            _service.Update(df);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            if (!_service.Delete(id))
                return NotFound();

            return NoContent();
        }
    }
}
