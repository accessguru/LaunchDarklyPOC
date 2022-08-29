using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;
using System.Reflection;
using System.Web;

namespace BlazorApp2.Client.FeatureFlag
{
    public class FeatureFlagClientService
    {
        #region Events

        /// <summary>
        /// Occurs when [feature flag updated].
        /// </summary>
        public event EventHandler<FeatureFlagEventArgs> FeatureFlagUpdated;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the HTTP client.
        /// </summary>
        /// <value>
        /// The HTTP client.
        /// </value>
        public HttpClient HttpClient { get; set; }

        /// <summary>
        /// Gets or sets the navigation manager.
        /// </summary>
        /// <value>
        /// The navigation manager.
        /// </value>
        public NavigationManager NavigationManager { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <returns></returns>
        public static string GetApplicationName() => Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// Raises the <see cref="E:FeatureFlagUpdated" /> event.
        /// </summary>
        /// <param name="e">The <see cref="FeatureFlagEventArgs"/> instance containing the event data.</param>
        protected virtual void OnFeatureFlagUpdated(FeatureFlagEventArgs e) => FeatureFlagUpdated.Invoke(this, e);

        /// <summary>
        /// Gets the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">HttpClient</exception>
        public async Task<bool> Get(string key)
        {
            return await CacheUtility.GetAsync(key, async () =>
            {
                if (this.HttpClient == null)
                {
                    throw new ArgumentNullException(nameof(this.HttpClient));
                }

                return await this.HttpClient.GetFromJsonAsync<bool>($"FeatureFlag/GetByKey/{HttpUtility.UrlEncode(key)}");
            });
        }

        /// <summary>
        /// Starts the listener.
        /// </summary>
        public async Task StartListener()
        {
            if (this.NavigationManager == null)
            {
                throw new NullReferenceException(nameof(this.NavigationManager));
            }

            var hubConnection = new HubConnectionBuilder()
                .WithUrl(this.NavigationManager.ToAbsoluteUri("/featureFlagHub"))
                .Build();

            await hubConnection.StartAsync();

            hubConnection.On<FeatureFlag>(nameof(IFeatureFlagClient.SendFeatureFlag), flag =>
            {
                CacheUtility.Update(flag.Key, flag.Value);
                this.OnFeatureFlagUpdated(new FeatureFlagEventArgs
                {
                    Key = flag.Key,
                    Value = flag.Value
                });
            });
        }

        #endregion Methods
    }
}