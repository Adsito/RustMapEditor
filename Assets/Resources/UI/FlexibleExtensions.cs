using UnityEngine.Events;
using UnityEngine.EventSystems;

public static class FlexibleExtensions
{
    public static void AddEventTrigger(this EventTrigger eventTrigger, UnityAction<BaseEventData> action,
        EventTriggerType triggerType)
    {
        EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
        trigger.AddListener(action);

        EventTrigger.Entry entry = new EventTrigger.Entry { callback = trigger, eventID = triggerType };
        eventTrigger.triggers.Add(entry);
    }
}