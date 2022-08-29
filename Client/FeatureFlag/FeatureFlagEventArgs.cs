namespace BlazorApp2.Client.FeatureFlag
{
    public class FeatureFlagEventArgs : EventArgs
    {
        public string Key { get; set; }

        public bool Value { get; set; }
    }
}