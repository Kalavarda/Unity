using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

public class CameraBehaviour : MonoBehaviour
{
    public Camera Camera;
    public GameObject PointOfSight;

    public float ZoomSpeed = 2;
    public float ZoomMin = 1f;
    public float ZoomMax = 3f;

    public float CameraMoveSpeed = 1.5f;

    private float _zoom;
    private float mouseX, mouseY;

    void Start()
    {
        _zoom = Mathf.Lerp(ZoomMin,  ZoomMax, 0.5f);
    }

    void Update()
    {
        _zoom -= Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed;
        _zoom = Mathf.Clamp(_zoom, ZoomMin, ZoomMax);
        Camera.transform.localPosition = new Vector3(Camera.transform.localPosition.x, Camera.transform.localPosition.y, - _zoom);

        if (Input.GetMouseButton((int)MouseButton.LeftMouse))
        {
            Cursor.visible = false;
            mouseX += Input.GetAxis("Mouse X") * CameraMoveSpeed;
            mouseY += Input.GetAxis("Mouse Y") * CameraMoveSpeed;
            transform.rotation = Quaternion.Euler(-mouseY, mouseX, 0);
        }
        else
            Cursor.visible = true;

        Camera.transform.LookAt(PointOfSight.transform.position);
    }
}
