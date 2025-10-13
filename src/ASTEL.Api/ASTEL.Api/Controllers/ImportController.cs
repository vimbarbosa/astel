using ASTEL.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASTEL.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly ImportService _importService;

        public ImportController(ImportService importService)
        {
            _importService = importService;
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportCsvAsync(IFormFile file)
        {
            var result = await _importService.ImportCsvAsync(file);
            return Ok(result);
        }
    }
}
