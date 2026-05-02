using Microsoft.AspNetCore.Mvc;

namespace VecinoApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { message = "API funcionando" });
    }
}