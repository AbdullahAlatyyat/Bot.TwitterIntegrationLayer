using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;

namespace WhatsppDirectLineInterface.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class BotApiController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "Done!";
        }
    }
}
