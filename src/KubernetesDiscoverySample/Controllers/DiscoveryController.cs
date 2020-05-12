using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Steeltoe.Discovery;
using Steeltoe.Discovery.KubernetesBase;

namespace KubernetesDiscoverySample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DiscoveryController : ControllerBase
    {
        private readonly ILogger<DiscoveryController> _logger;
        private readonly IDiscoveryClient _discoveryClient;

        public DiscoveryController(ILogger<DiscoveryController> logger, IDiscoveryClient discoveryClient)
        {
            _logger = logger;
            _discoveryClient = discoveryClient;
        }

        [HttpGet]
        public IEnumerable<KubernetesServiceInstance> Get()
        {
            return _discoveryClient.Services.SelectMany(x => _discoveryClient.GetInstances(x)).Cast<KubernetesServiceInstance>();
        }
    }
}