using Microsoft.AspNetCore.Mvc;

namespace WpfApp1.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public string Test()
        {
            return $@"test {DateTime.Now}";
        }
    }
}
