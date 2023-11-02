using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PointerHandler : MonoBehaviour
{
    public bool debugLog = false;

    private List<GraphicRaycaster> raycasters = new List<GraphicRaycaster>();

    private List<GameObject> hoveredObjects = new List<GameObject>();
    private List<GameObject> pressedObjects = new List<GameObject>();

    public static Vector2 GetPixelCoord(RectTransform canvasRect, Vector3 relPos, bool normalize = false)
    {
        float pixelCoordX = (canvasRect.rect.width / 2) + (relPos.x / canvasRect.localScale.x);
        float pixelCoordY = (canvasRect.rect.height / 2) + (relPos.y / canvasRect.localScale.y);
        Debug.Log(pixelCoordX + ", " + pixelCoordY);

        if (normalize)
        {
            pixelCoordX = pixelCoordX / canvasRect.rect.width;
            pixelCoordY = pixelCoordY / canvasRect.rect.height;
            Debug.Log(pixelCoordX + ", " + pixelCoordY);
        }

        return new Vector2(pixelCoordX, pixelCoordY);
    }

    public static Vector2 WorldPosToScreenPos(Vector3 worldPos, Camera eventCamera)
    {
        // Convierte la posición local a la posición de la pantalla si hay una cámara asignada.
        Vector3 screenPosition3D = eventCamera.WorldToScreenPoint(worldPos);

        // Descarta la componente z, ya que sólo queremos las coordenadas x, y en la pantalla.
        return new Vector2(screenPosition3D.x, screenPosition3D.y);
    }

    public void OnPointerDown(Vector3 worldPos)
    {
        var raycastResults = new List<RaycastResult>();
        PointerEventData eventData = Raycast(worldPos, raycastResults);

        // Debug log
        if (debugLog)
        {
            string raycasted = string.Join(", ", raycastResults.Select(x => x.gameObject.name).ToList());
            Debug.Log($"[OnPointerDown] Raycasted:{raycasted}");
        }

        // Envía eventos de clic a todos los objetos en la posición.
        foreach (var result in raycastResults)
        {
            // Aquí debemos usar ExecuteEvents.ExecuteHierarchy en lugar de ExecuteEvents.Execute
            // ya que queremos que el evento se propague a través de la jerarquía de objetos UI.
            eventData.pointerPressRaycast = result;
            ExecuteEvents.ExecuteHierarchy(result.gameObject, eventData, ExecuteEvents.pointerDownHandler);

            // Lo guardamos para poder ejecutarle OnPointerUp aunque no estemos sobre él
            if (!pressedObjects.Contains(result.gameObject))
            {
                pressedObjects.Add(result.gameObject);
            }
        }
    }

    public void OnPointerUp(Vector3 worldPos)
    {
        var raycastResults = new List<RaycastResult>();
        PointerEventData eventData = Raycast(worldPos, raycastResults);

        // Purga objetos que se hayan destruido
        pressedObjects = pressedObjects.Where(x => x != null && x.gameObject != null).ToList();

        // Debug log
        if (debugLog)
        {
            string raycasted = string.Join(", ", raycastResults.Select(x => x.gameObject.name).ToList());
            string cached = string.Join(", ", pressedObjects.Select(x => x.gameObject.name).ToList());
            Debug.Log($"[OnPointerUp] Raycasted:{raycasted}. Cached: {cached}");
        }

        // Envía eventos de clic a todos los objetos en la posición.
        foreach (var result in raycastResults)
        {
            // Aquí debemos usar ExecuteEvents.ExecuteHierarchy en lugar de ExecuteEvents.Execute
            // ya que queremos que el evento se propague a través de la jerarquía de objetos UI.
            eventData.pointerPressRaycast = result;
            ExecuteEvents.ExecuteHierarchy(result.gameObject, eventData, ExecuteEvents.pointerUpHandler);

            // Lo eliminamos porque ya hemos ejecutado OnPointerUp sobre él
            if (pressedObjects.Contains(result.gameObject))
            {
                pressedObjects.Remove(result.gameObject);
            }
        }

        // Ejecutamos OnPointerUp aunque no estemos sobre ellos
        foreach (var pressedObject in pressedObjects)
        {
            ExecuteEvents.ExecuteHierarchy(pressedObject, eventData, ExecuteEvents.pointerUpHandler);
        }
        pressedObjects.Clear();
    }

    public void OnDrag(Vector3 worldPos)
    {
        var raycastResults = new List<RaycastResult>();
        PointerEventData eventData = Raycast(worldPos, raycastResults);

        // Debug log
        if (debugLog)
        {
            string raycasted = string.Join(", ", raycastResults.Select(x => x.gameObject.name).ToList());
            Debug.Log($"[OnDrag] Raycasted:{raycasted}");
        }

        foreach (var result in raycastResults)
        {
            eventData.pointerPressRaycast = result;
            ExecuteEvents.ExecuteHierarchy(result.gameObject, eventData, ExecuteEvents.dragHandler);
        }
    }

    // Puedes llamar a este método desde otro script para comenzar el proceso de arrastre manualmente
    public void OnBeginDrag(Vector3 worldPos)
    {
        var raycastResults = new List<RaycastResult>();
        PointerEventData eventData = Raycast(worldPos, raycastResults);

        // Debug log
        if (debugLog)
        {
            string raycasted = string.Join(", ", raycastResults.Select(x => x.gameObject.name).ToList());
            Debug.Log($"[OnBeginDrag] Raycasted:{raycasted}");
        }

        foreach (var result in raycastResults)
        {
            eventData.pointerPressRaycast = result;
            ExecuteEvents.ExecuteHierarchy(result.gameObject, eventData, ExecuteEvents.beginDragHandler);
        }
    }

    // Puedes llamar a este método desde otro script para finalizar el proceso de arrastre manualmente
    public void OnEndDrag(Vector3 worldPos)
    {
        var raycastResults = new List<RaycastResult>();
        PointerEventData eventData = Raycast(worldPos, raycastResults);

        // Debug log
        if (debugLog)
        {
            string raycasted = string.Join(", ", raycastResults.Select(x => x.gameObject.name).ToList());
            Debug.Log($"[OnEndDrag] Raycasted:{raycasted}");
        }

        foreach (var result in raycastResults)
        {
            eventData.pointerPressRaycast = result;
            ExecuteEvents.ExecuteHierarchy(result.gameObject, eventData, ExecuteEvents.endDragHandler);
        }
    }

    public void OnPointerHover(Vector3 worldPos)
    {
        var raycastResults = new List<RaycastResult>();
        PointerEventData eventData = Raycast(worldPos, raycastResults);

        // Debug log
        if (debugLog)
        {
            string raycasted = string.Join(", ", raycastResults.Select(x => x.gameObject.name).ToList());
            Debug.Log($"[OnPointerHover] Raycasted:{raycasted}");
        }

        foreach (var result in raycastResults)
        {
            eventData.pointerPressRaycast = result;
            ExecuteEvents.ExecuteHierarchy(result.gameObject, eventData, ExecuteEvents.pointerEnterHandler);

            // Lo guardamos para poder ejecutarle OnPointerUp aunque no estemos sobre él
            if (!hoveredObjects.Contains(result.gameObject))
            {
                hoveredObjects.Add(result.gameObject);
            }
        }
    }

    public void OnPointerUnhover(Vector3 worldPos)
    {
        var raycastResults = new List<RaycastResult>();
        PointerEventData eventData = Raycast(worldPos, raycastResults);

        // Purga objetos que se hayan destruido
        hoveredObjects = hoveredObjects.Where(x => x != null && x.gameObject != null).ToList();

        // Debug log
        if (debugLog)
        {
            string raycasted = string.Join(", ", raycastResults.Select(x => x.gameObject.name).ToList());
            string cached = string.Join(", ", hoveredObjects.Select(x => x.gameObject.name).ToList());
            Debug.Log($"[OnPointerUnhover] Raycasted:{raycasted}. Cached: {cached}");
        }

        foreach (var result in raycastResults)
        {
            eventData.pointerPressRaycast = result;
            ExecuteEvents.ExecuteHierarchy(result.gameObject, eventData, ExecuteEvents.pointerExitHandler);

            // Lo eliminamos porque ya hemos ejecutado OnPointerExit sobre él
            if (hoveredObjects.Contains(result.gameObject))
            {
                hoveredObjects.Remove(result.gameObject);
            }
        }

        // Ejecutamos OnPointerExit aunque no estemos sobre ellos
        foreach (var hoveredObject in hoveredObjects)
        {
            ExecuteEvents.ExecuteHierarchy(hoveredObject, eventData, ExecuteEvents.pointerExitHandler);
        }
        hoveredObjects.Clear();
    }

    public void OnSubmit(Vector3 worldPos)
    {
        var raycastResults = new List<RaycastResult>();
        PointerEventData eventData = Raycast(worldPos, raycastResults);

        // Debug log
        if (debugLog)
        {
            string raycasted = string.Join(", ", raycastResults.Select(x => x.gameObject.name).ToList());
            Debug.Log($"[OnSubmit] Raycasted:{raycasted}");
        }

        foreach (var result in raycastResults)
        {
            eventData.pointerPressRaycast = result;
            ExecuteEvents.ExecuteHierarchy(result.gameObject, eventData, ExecuteEvents.submitHandler);
        }
    }

    PointerEventData Raycast(Vector3 worldPos, List<RaycastResult> raycastResults)
    {
        // Verifica si el EventSystem está activo
        if (EventSystem.current == null)
        {
            Debug.LogWarning("EventSystem no encontrado");
            return null;
        }

        // Crea el PointerEventData.
        PointerEventData eventData = new PointerEventData(EventSystem.current);

        // Realiza el raycast.
        raycastResults.Clear();

        List<RaycastResult> tmp = new List<RaycastResult>();
        raycasters = FindObjectsOfType<GraphicRaycaster>().ToList();
        foreach (var raycaster in raycasters)
        {
            eventData.position = WorldPosToScreenPos(worldPos, raycaster.eventCamera);

            tmp.Clear();
            raycaster.Raycast(eventData, tmp);
            raycastResults.AddRange(tmp);
        }

        return eventData;
    }
}
