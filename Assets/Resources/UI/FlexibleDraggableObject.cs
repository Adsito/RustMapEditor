using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class FlexibleDraggableObject : MonoBehaviour
{
    public GameObject Target;
    private EventTrigger _eventTrigger;
    void Start ()
    {
        _eventTrigger = GetComponent<EventTrigger>();
        _eventTrigger.AddEventTrigger(OnDrag, EventTriggerType.Drag);
    }

    void OnDrag(BaseEventData data)
    {
        PointerEventData ped = (PointerEventData) data;
        Target.transform.Translate(ped.delta);
    }
}