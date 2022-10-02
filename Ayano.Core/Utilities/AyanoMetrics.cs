namespace Ayano.Core.Utilities;

using Prometheus;

public static class AyanoMetrics
{
    /// <summary>
    /// How many phishing links Ayano has loaded.
    /// </summary>
    public static readonly Gauge LoadedPhishingLinks = Metrics.CreateGauge("loaded_phishing_links", "Number of loaded phishing links");

    /// <summary>
    /// How many phishing links Ayano has seen.
    /// </summary>
    public static readonly Counter SeenPhishingLinks = Metrics.CreateCounter("seen_phishing_links", "Number of seen phishing links", "domain");

    /// <summary>
    /// Tracker for measuring how long it takes to detect phishing via various means (messages, avatars, or usernames).
    /// </summary>
    public static readonly Gauge PhishingDetection = Metrics.CreateGauge("phishing_detection_time", "How long it takes to detect phishing", "reason");

    /// <summary>
    /// How many reminders Ayano has loaded.
    /// </summary>
    public static readonly Gauge LoadedReminders = Metrics.CreateGauge("loaded_reminders", "Number of loaded reminders");

    /// <summary>
    /// How many infractions Ayano has loaded.
    /// </summary>
    public static readonly Gauge LoadedInfractions = Metrics.CreateGauge("loaded_infractions", "Number of loaded infractions");

    /// <summary>
    /// How long it takes for Ayano to dispatch an infraction.
    /// </summary>
    public static readonly Gauge InfractionDispatchTime = Metrics.CreateGauge("infraction_dispatch_time", "Time spent dispatching infractions", "type");

    /// <summary>
    /// How long it takes for Ayano to dispatch a reminder.
    /// </summary>
    public static readonly Gauge ReminderDispatchTime = Metrics.CreateGauge("reminder_dispatch_time", "Time spent dispatching reminders");

    /// <summary>
    /// How many commands Ayano has handled, by type.
    /// </summary>
    public static readonly Counter SeenCommands = Metrics.CreateCounter("seen_commands", "Number of seen commands", "type");

    /// <summary>
    /// How long it takes for Ayano to evaluate exemptions.
    /// </summary>
    public static readonly Gauge EvaluationExemptionTime = Metrics.CreateGauge("evaluation_exemption_time", "Time spent evaluating exemption", "type");

    /// <summary>
    /// Gateway events Ayano has received, by type.
    /// </summary>
    public static readonly Counter GatewayEventReceieved = Metrics.CreateCounter("gateway_event_received", "Number of gateway events received", "type");

    /// <summary>
    /// API calls to Discord.
    /// </summary>
    public static readonly Counter HttpRequests = Metrics.CreateCounter("http_requests", "Number of HTTP requests", "method", "response", "endpoint");
}