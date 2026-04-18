using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterRotate : MonoBehaviour
{
    public float rotationSpeed = 0.2f;
    private Vector2 lastPos;

    void Update()
    {
        if (MenuController.Instance != null && MenuController.Instance.IsUIOpen)
            return;

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
            lastPos = Input.mousePosition;

        if (Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastPos;
            transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
            lastPos = Input.mousePosition;
        }
    }
}