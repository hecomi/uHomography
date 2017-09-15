using UnityEngine;
using UnityEngine.EventSystems;

namespace uHomography
{

[ExecuteInEditMode]
public class DraggableVertex 
    : MonoBehaviour
    , IBeginDragHandler
    , IDragHandler
{
    public new Camera camera;

    Vector3 startPos_;
    Vector3 startMousePos_;

    public bool hasChanged { get; set; }

    public Vector2 viewPosition
    {
        get 
        { 
            if (!camera) return Vector2.zero;
            var p = camera.WorldToViewportPoint(transform.position); 
            return new Vector2(p.x, p.y);
        }
    }

    public void OnBeginDrag(PointerEventData data)
    {
        startPos_ = transform.localPosition;
        startMousePos_ = Input.mousePosition;
    }

    public void OnDrag(PointerEventData data)
    {
        var z = transform.localPosition.z;
        var mousePos = Input.mousePosition;
        var currentPos = camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, z));
        var startPos = camera.ScreenToWorldPoint(new Vector3(startMousePos_.x, startMousePos_.y, z));
        var dPos = currentPos - startPos;
        transform.localPosition = startPos_ + dPos;
        hasChanged = true;
    }
}

}