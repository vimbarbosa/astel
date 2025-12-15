using ASTEL.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace ASTEL.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly ImportFinanceiroService _financeiroService;

        public ImportController(ImportFinanceiroService financeiroService)
        {
            _financeiroService = financeiroService;
        }

        [HttpPost("importFinanceiro")]
        public async Task<IActionResult> ImportFinanceiro([FromForm] IFormFile file)
        {
            var result = await _financeiroService.ImportCsvAsync(file);
            return Ok(result);
        }

        [HttpPost("importFinanceiroExcel")]
        public async Task<IActionResult> ImportFinanceiroExcel([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Nenhum arquivo foi enviado." });

            // Valida extensão do arquivo
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
                return BadRequest(new { message = "Arquivo deve ser Excel (.xlsx ou .xls)." });

            var result = await _financeiroService.ImportExcelAsync(file);
            return Ok(new { message = result });
        }

        /// <summary>
        /// Importa dados financeiros a partir de arquivo Excel usando Matrícula Sistel
        /// </summary>
        /// <remarks>
        /// Este endpoint importa dados financeiros de um arquivo Excel (.xlsx ou .xls).
        /// Diferente do endpoint importFinanceiroExcel, este usa a Matrícula Sistel para buscar o IdDadosCadastrais,
        /// e extrai automaticamente o mês e ano do campo DATA_PAGAMENTO.
        /// 
        /// **Layout da planilha (colunas obrigatórias):**
        /// - PATROCINADORA (ignorado)
        /// - MATRICULA (obrigatório) - Matrícula Sistel
        /// - NOME (ignorado)
        /// - REGPAT (ignorado)
        /// - DATA_PAGAMENTO (obrigatório) - campo de data para extrair mês e ano
        /// - VALOR (obrigatório) - valor do pagamento
        /// - VERBA (ignorado)
        /// 
        /// **Processamento:**
        /// - O sistema busca o IdDadosCadastrais usando a Matrícula Sistel (campo MATRICULA)
        /// - Extrai o mês e ano do campo DATA_PAGAMENTO
        /// - Insere ou atualiza registros na tabela DadosFinanceiros
        /// - Registros com Matrícula Sistel não encontrada são ignorados
        /// </remarks>
        /// <param name="file">Arquivo Excel (.xlsx ou .xls) contendo os dados financeiros</param>
        /// <returns>Mensagem de sucesso com quantidade de registros processados</returns>
        /// <response code="200">Importação realizada com sucesso</response>
        /// <response code="400">Arquivo inválido ou ausente</response>
        [HttpPost("importSistel")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ImportSistel([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Nenhum arquivo foi enviado." });

            // Valida extensão do arquivo
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xlsx" && extension != ".xls")
                return BadRequest(new { message = "Arquivo deve ser Excel (.xlsx ou .xls)." });

            var result = await _financeiroService.ImportSistelExcelAsync(file);
            return Ok(new { message = result });
        }
    }
}
