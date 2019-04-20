using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public enum HandlerType
{
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left,
    TopLeft,
    Top
}

[RequireComponent(typeof(EventTrigger))]
public class FlexibleResizeHandler : MonoBehaviour
{
    public HandlerType Type;
    public RectTransform Target;
    public Vector2 MinimumDimmensions = new Vector2(50, 50);
    public Vector2 MaximumDimmensions = new Vector2(800, 800);
    
    private EventTrigger _eventTrigger;
    
	void Start ()
	{
	    _eventTrigger = GetComponent<EventTrigger>();
        _eventTrigger.AddEventTrigger(OnDrag, EventTriggerType.Drag);
	}

    void OnDrag(BaseEventData data)
    {
        PointerEventData ped = (PointerEventData) data;
        //Target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Target.rect.width + ped.delta.x);
        //Target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Target.rect.height + ped.delta.y);
        RectTransform.Edge? horizontalEdge = null;
        RectTransform.Edge? verticalEdge = null;

        switch (Type)
        {
            case HandlerType.TopRight:
                horizontalEdge = RectTransform.Edge.Left;
                verticalEdge = RectTransform.Edge.Bottom;
                break;
            case HandlerType.Right:
                horizontalEdge = RectTransform.Edge.Left;
                break;
            case HandlerType.BottomRight:
                horizontalEdge = RectTransform.Edge.Left;
                verticalEdge = RectTransform.Edge.Top;
                break;
            case HandlerType.Bottom:
                verticalEdge = RectTransform.Edge.Top;
                break;
            case HandlerType.BottomLeft:
                horizontalEdge = RectTransform.Edge.Right;
                verticalEdge = RectTransform.Edge.Top;
                break;
            case HandlerType.Left:
                horizontalEdge = RectTransform.Edge.Right;
                break;
            case HandlerType.TopLeft:
                horizontalEdge = RectTransform.Edge.Right;
                verticalEdge = RectTransform.Edge.Bottom;
                break;
            case HandlerType.Top:
                verticalEdge = RectTransform.Edge.Bottom;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
if (horizontalEdge != null)
        {
            if (horizontalEdge == RectTransform.Edge.Right)
            {
                float newWidth = Mathf.Clamp(Target.sizeDelta.x - ped.delta.x, MinimumDimmensions.x, MaximumDimmensions.x);
                float deltaPosX = -(newWidth - Target.sizeDelta.x) * Target.pivot.x;

                Target.sizeDelta = new Vector2(newWidth, Target.sizeDelta.y);
                Target.anchoredPosition = Target.anchoredPosition + new Vector2(deltaPosX, 0);
            }
            else
            {
                float newWidth = Mathf.Clamp(Target.sizeDelta.x + ped.delta.x, MinimumDimmensions.x, MaximumDimmensions.x);
                float deltaPosX = (newWidth - Target.sizeDelta.x) * Target.pivot.x;

                Target.sizeDelta = new Vector2(newWidth, Target.sizeDelta.y);
                Target.anchoredPosition = Target.anchoredPosition + new Vector2(deltaPosX, 0);
            }
        }
        if (verticalEdge != null)
        {
            if (verticalEdge == RectTransform.Edge.Top)
            {
                float newHeight = Mathf.Clamp(Target.sizeDelta.y - ped.delta.y, MinimumDimmensions.y, MaximumDimmensions.y);
                float deltaPosY = -(newHeight - Target.sizeDelta.y) * Target.pivot.y;

                Target.sizeDelta = new Vector2(Target.sizeDelta.x, newHeight);
                Target.anchoredPosition = Target.anchoredPosition + new Vector2(0, deltaPosY);
            }
            else
            {
                float newHeight = Mathf.Clamp(Target.sizeDelta.y + ped.delta.y, MinimumDimmensions.y, MaximumDimmensions.y);
                float deltaPosY = (newHeight - Target.sizeDelta.y) * Target.pivot.y;

                Target.sizeDelta = new Vector2(Target.sizeDelta.x, newHeight);
                Target.anchoredPosition = Target.anchoredPosition + new Vector2(0, deltaPosY);
            }
        }
    }
}