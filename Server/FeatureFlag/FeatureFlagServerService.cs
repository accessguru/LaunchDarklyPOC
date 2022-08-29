using System.Diagnostics;
using BlazorApp2.Client.FeatureFlag;
using LaunchDarkly.Sdk;
using LaunchDarkly.Sdk.Server;
using Microsoft.AspNetCore.SignalR;

namespace BlazorApp2.Server.FeatureFlag
{
    public class FeatureFlagServerService
    {
        #region Fields

        private LdClient _ldClient;
        private readonly string _applicationName;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureFlagServerService"/> class.
        /// </summary>
        public FeatureFlagServerService()
        {
            this._applicationName = FeatureFlagClientService.GetApplicationName();
            this.Init();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the feature flag hub context.
        /// </summary>
        /// <value>
        /// The feature flag hub context.
        /// </value>
        public IHubContext<FeatureFlagHub, IFeatureFlagClient> FeatureFlagHubContext { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public bool GetValue(string key)
        {
            var user = User.Builder(key)
                .Name(this._applicationName)
                .Build();

            return this._ldClient.BoolVariation(key, user);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Could not initialize Launch Darkly.</exception>
        private void Init()
        {
            var config = Configuration.Default("sdk-34f83348-d251-4389-a8af-c5590a46f4a8");
            var user = User.WithKey(this._applicationName);

            this._ldClient = new LdClient(config);
            if (!this._ldClient.Initialized)
            {
                throw new InvalidOperationException("Could not initialize Launch Darkly.");
            }

            this._ldClient.FlagTracker.FlagChanged += async (sender, e) =>
            {
                var flagValue = _ldClient.BoolVariation(e.Key, user);

                // Move to the handler below once working
                await this.FeatureFlagHubContext.Clients.All.SendFeatureFlag(new Client.FeatureFlag.FeatureFlag { Key = e.Key, Value = flagValue });

                //this._ldClient.FlagTracker.FlagValueChangeHandler(e.Key, user,
                //    (s, changeArgs) =>
                //    {
                //        // TODO: SignalR here
                //        Debug.WriteLine($"flag '{changeArgs.Key} from {changeArgs.OldValue} to {changeArgs.NewValue}");
                //    });
            };
        }

        #endregion Methods
    }
}