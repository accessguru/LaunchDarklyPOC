namespace BlazorApp2.Client.FeatureFlag;

public interface IFeatureFlagClient
{
    Task SendFeatureFlag(FeatureFlag featureFlag);
}