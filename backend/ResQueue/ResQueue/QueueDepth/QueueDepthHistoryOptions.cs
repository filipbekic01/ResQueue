namespace ResQueue.QueueDepth;

/// <summary>
/// Controls periodic sampling of queue depth (ready / error / dead-letter counts) into a
/// ResQueue-owned table, so the UI can show how queue depth changes over time.
/// </summary>
public class QueueDepthHistoryOptions
{
    public static readonly TimeSpan MinimumSampleInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Whether the background sampler runs. Defaults to <c>false</c>; enabling it creates a
    /// ResQueue-owned table in the transport schema.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// How often queue depth is sampled. Defaults to 60 seconds; values below 5 seconds are clamped.
    /// </summary>
    public TimeSpan SampleInterval { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// How long samples are kept before being deleted. Defaults to 1 day.
    /// </summary>
    public TimeSpan Retention { get; set; } = TimeSpan.FromDays(1);

    internal TimeSpan EffectiveSampleInterval =>
        SampleInterval < MinimumSampleInterval ? MinimumSampleInterval : SampleInterval;

    internal TimeSpan EffectiveRetention =>
        Retention <= TimeSpan.Zero ? TimeSpan.FromDays(1) : Retention;
}
