using Microsoft.AspNetCore.Mvc;

namespace AD_Users_Publisher.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublishController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "value";
        }
    }
}

// https://prod-24.centralus.logic.azure.com:443/workflows/27dfb5ebbe844a1595d1a7ec0bd2575a/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=wVh5Ycww6R2qqY-ck_w2iWSG7Ij3ASTvS3FmeBpv-_w