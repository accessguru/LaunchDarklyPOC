using BlazorApp2.Client.FeatureFlag;
using Microsoft.AspNetCore.SignalR;

namespace BlazorApp2.Server.FeatureFlag
{
    public class FeatureFlagHub : Hub<IFeatureFlagClient>
    {
    }
}