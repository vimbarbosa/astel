using ASTEL.Api.Services;
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
    }
}
