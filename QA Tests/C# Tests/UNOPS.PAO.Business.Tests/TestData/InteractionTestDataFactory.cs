using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

namespace UNOPS.PAO.Business.Tests.TestData;

/// <summary>
/// Factory for creating Interaction test data
/// </summary>
public class InteractionTestDataFactory
{
    private int _sequenceNumber = 1;

    public Interaction CreateInteraction(Action<Interaction>? customize = null)
    {
        var interaction = new Interaction
        {
            Id = _sequenceNumber++,
            Name = $"Interaction {_sequenceNumber}",
            Subject = $"Interaction {_sequenceNumber}",
            Description = $"Description {_sequenceNumber}",
            Type = InteractionType.InPersonMeeting,
            Date = DateTime.UtcNow,
            Status = EntityStatus.Active,
            CreatedDate = DateTime.UtcNow,
            IsDeleted = false
        };

        customize?.Invoke(interaction);
        return interaction;
    }

    public Interaction CreateMeetingInteraction(DateTime? date = null)
    {
        return CreateInteraction(i =>
        {
            i.Type = InteractionType.InPersonMeeting;
            i.Date = date ?? DateTime.UtcNow;
        });
    }

    public Interaction CreateVirtualMeetingInteraction(DateTime? date = null)
    {
        return CreateInteraction(i =>
        {
            i.Type = InteractionType.VirtualMeeting;
            i.Date = date ?? DateTime.UtcNow;
        });
    }

    public Interaction CreateEmailInteraction()
    {
        return CreateInteraction(i => i.Type = InteractionType.Email);
    }

    public Interaction CreateCallInteraction()
    {
        return CreateInteraction(i => i.Type = InteractionType.Call);
    }

    public Interaction CreateChatInteraction()
    {
        return CreateInteraction(i => i.Type = InteractionType.Chat);
    }
}
