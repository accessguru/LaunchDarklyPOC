using BlazorApp2.Server.FeatureFlag;
using Microsoft.AspNetCore.Mvc;

namespace BlazorApp2.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FeatureFlagController : ControllerBase
    {
        private readonly FeatureFlagServerService featureFlagServerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureFlagController"/> class.
        /// </summary>
        /// <param name="featureFlagServerService">The feature flag server service.</param>
        public FeatureFlagController(FeatureFlagServerService featureFlagServerService)
        {
            this.featureFlagServerService = featureFlagServerService;
        }

        /// <summary>
        /// Gets the by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetByKey/{key}")]
        public bool GetByKey(string key) => this.featureFlagServerService.GetValue(key);
    }
}