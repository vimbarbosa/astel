using ASTEL.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASTEL.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly ImportService _importService;
        private readonly ImportFinanceiroService _financeiroService;

        public ImportController(ImportService importService, ImportFinanceiroService financeiroService)
        {
            _importService = importService;
            _financeiroService = financeiroService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportCsvAsync(IFormFile file)
        {
            var result = await _importService.ImportCsvAsync(file);
            return Ok(result);
        }

        [HttpPost("importFinanceiro")]
        public async Task<IActionResult> ImportFinanceiro([FromForm] IFormFile file)
        {
            var result = await _financeiroService.ImportCsvAsync(file);
            return Ok(result);
        }
    }
}
