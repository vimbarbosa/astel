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
             bool? inadimplente = null,
             string? cidade = null,
             string? estado = null,
             string? email = null,
             string? telefone = null,
             bool? descontoFolha = null,
             string? formapagamento = null,
             int pageNumber = 1,
             int pageSize = 10)
        {
            var (dados, totalCount, somaValorPago) = await _service.GetFilteredAsync(
                dataInicio, dataFim, nome, cpf, inadimplente,
                cidade, estado, email, telefone,
                descontoFolha, formapagamento,
                pageNumber, pageSize);

            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Adiciona SomaValorPago e TotalRegistros em cada registro
            foreach (var dado in dados)
            {
                dado.SomaValorPago = somaValorPago;
                dado.TotalRegistros = dados.Count;
            }

            Response.Headers["X-Total-Count"] = totalCount.ToString();
            Response.Headers["X-Page-Number"] = pageNumber.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();
            Response.Headers["X-Total-Pages"] = totalPages.ToString();
            Response.Headers["X-Soma-Valor-Pago"] = somaValorPago.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            return Ok(dados);
        }

        [HttpGet("{id:long}")]
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
                df.Mes = Convert.ToInt32(df.Mes);
                df.Ano = Convert.ToInt32(df.Ano);

                _service.Add(df);

                return Ok(new { message = "Pagamento cadastrado com sucesso!" });
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
                {
                    // Se ainda ocorrer duplicata (caso raro), tenta novamente removendo o existente
                    try
                    {
                        df.Mes = Convert.ToInt32(df.Mes);
                        df.Ano = Convert.ToInt32(df.Ano);
                        _service.Add(df);
                        return Ok(new { message = "Pagamento atualizado com sucesso!" });
                    }
                    catch
                    {
                        return Conflict(new
                        {
                            message = "Erro ao atualizar pagamento existente."
                        });
                    }
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


        [HttpGet("export/excel/xlsx")]
        public async Task<IActionResult> ExportarExcel(
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            string? nome = null,
            string? cpf = null,
            bool? inadimplente = null,
            string? cidade = null,
            string? estado = null,
            string? email = null,
            string? telefone = null,
            bool? descontoFolha = null,
            string? formapagamento = null,
             [FromQuery] List<string>? columns = null)  
        {
            var dados = await _service.ExportarSemPaginacaoAsync(
                dataInicio, dataFim, nome, cpf, inadimplente,
                cidade, estado, email, telefone, descontoFolha, formapagamento
            );

            var arquivo = _service.GerarExcel(dados, columns);

            return File(
                arquivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "financeiro_export.xlsx"
            );
        }

        [HttpGet("gerar-modelo-importacao")]
        public async Task<IActionResult> GerarModeloImportacao(
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            string? nome = null,
            string? cpf = null,
            bool? inadimplente = null,
            string? cidade = null,
            string? estado = null,
            string? email = null,
            string? telefone = null,
            bool? descontoFolha = null,
            string? formapagamento = null)
        {
            var dados = await _service.ExportarSemPaginacaoAsync(
                dataInicio, dataFim, nome, cpf, inadimplente,
                cidade, estado, email, telefone, descontoFolha, formapagamento
            );

            var arquivo = _service.GerarModeloImportacao(dados);

            return File(
                arquivo,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "modelo_importacao.xlsx"
            );
        }

        [HttpGet("historico/{idDadosCadastrais}")]
        public async Task<ActionResult<IEnumerable<HistoricoPagamentoDTO>>> GetHistoricoPagamentoPorUsuario(long idDadosCadastrais)
        {
            var historico = await _service.GetHistoricoPagamentoPorUsuarioAsync(idDadosCadastrais);
            return Ok(historico);
        }

        [HttpGet("por-cadastro")]
        public async Task<ActionResult<IEnumerable<HistoricoPagamentoDTO>>> GetDadosFinanceirosPorCadastro(
            [FromQuery] long? idDadosCadastrais = null,
            [FromQuery] DateTime? dataInicio = null,
            [FromQuery] DateTime? dataFim = null)
        {
            var registros = await _service.GetDadosFinanceirosPorCadastroAsync(
                idDadosCadastrais, 
                dataInicio, 
                dataFim);
            
            return Ok(registros);
        }
    }
}
