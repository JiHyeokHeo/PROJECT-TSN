using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class HoverDebug : MonoBehaviour
{
    void Update()
    {
        if (EventSystem.current == null)
            return;

        //if (Pointer.current == null)
        //    return;
        
        Vector2 pointerPosition = Mouse.current.position.ReadValue();
        //Debug.Log(pointerPosition);

        PointerEventData data = new PointerEventData(EventSystem.current)
        {
            position = pointerPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        if (results.Count > 0)
        {
            Debug.Log($"Top UI Hit: {results[0].gameObject.name}");
        }
    }
}