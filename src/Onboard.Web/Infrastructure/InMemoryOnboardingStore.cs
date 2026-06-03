using System.Collections.Concurrent;
using Onboard.Web.Domain;

namespace Onboard.Web.Infrastructure;

public sealed class InMemoryOnboardingStore
{
    public ConcurrentDictionary<string, OnboardingCase> Cases { get; } = new();
    public ConcurrentDictionary<string, PersonProfile> ProfilesByCase { get; } = new();
    public ConcurrentDictionary<string, IdentityDocument> DocumentsByCase { get; } = new();
    public ConcurrentDictionary<string, BiometricSession> BiometricsByCase { get; } = new();
    public ConcurrentDictionary<string, RiskResult> RisksByCase { get; } = new();
    public ConcurrentDictionary<string, ReviewCase> ReviewsByCase { get; } = new();
    public ConcurrentDictionary<string, SignaturePackage> SignaturesByCase { get; } = new();
    public ConcurrentDictionary<string, List<AuditEvent>> AuditEventsByCase { get; } = new();
    public ConcurrentDictionary<string, List<ChannelTransition>> ChannelTransitionsByCase { get; } = new();
    public ConcurrentDictionary<string, bool> ProcessedCallbacks { get; } = new();

    public void AppendAuditEvent(AuditEvent auditEvent)
    {
        var events = AuditEventsByCase.GetOrAdd(auditEvent.CaseId, _ => new List<AuditEvent>());
        lock (events)
        {
            events.Add(auditEvent);
        }
    }

    public void AppendTransition(ChannelTransition transition)
    {
        var transitions = ChannelTransitionsByCase.GetOrAdd(transition.CaseId, _ => new List<ChannelTransition>());
        lock (transitions)
        {
            transitions.Add(transition);
        }
    }
}
