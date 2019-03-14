using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Matrix.Multiply.Models;
using Matrix.Multiply.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Matrix.Multiply.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatrixMultiplyController : ControllerBase
    {

        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger logger;
        private HttpClient client;

        public MatrixMultiplyController(ILogger<MatrixMultiplyController> _logger, IHttpClientFactory _clientFactory)
        {
            clientFactory = _clientFactory;
            logger = _logger;
            client = clientFactory.CreateClient("investcloud");

        }


        // GET api/numbers/init/1000
        [HttpGet("{size}")]
        public async Task<ActionResult<string>> Get(int size)
        {
            LanguageService languageService = new LanguageService(client, logger, size);
            return languageService.process();

        }
    }
}
