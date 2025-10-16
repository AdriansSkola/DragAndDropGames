using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public float maxZoom = 300f,
        minZoom = 150f,
        panSpeed = 6;
    Vector3 bottomLeft, topRight;
    float cameraMaxX, cameraMinX, cameraMaxY, cameraMinY, x, y;
    public Camera cam;
    float defaultZoom;

    void Start()
    {
        cam = GetComponent<Camera>();
        defaultZoom = cam.orthographicSize; // store starting zoom

        topRight = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, -transform.position.z));
        bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, -transform.position.z));
        cameraMaxX = topRight.x;
        cameraMinX = bottomLeft.x;
        cameraMaxY = topRight.y;
        cameraMinY = bottomLeft.y;
    }

    void Update()
    {
        x = Input.GetAxis("Mouse X") * panSpeed;
        y = Input.GetAxis("Mouse Y") * panSpeed;
        transform.Translate(x, y, 0);

        // ZOOM IN
        if ((Input.GetAxis("Mouse ScrollWheel") > 0) && cam.orthographicSize > minZoom)
        {
            cam.orthographicSize -= 50f;
        }

        // ZOOM OUT
        if ((Input.GetAxis("Mouse ScrollWheel") < 0) && cam.orthographicSize < maxZoom)
        {
            cam.orthographicSize += 50f;

            // If we're at max zoom, restore to default camera state
            if (cam.orthographicSize >= maxZoom)
            {
                cam.orthographicSize = defaultZoom;
                transform.position = new Vector3(0, 0, transform.position.z); // reset position if desired
            }
        }

        // Update visible area based on new zoom
        topRight = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, -transform.position.z));
        bottomLeft = cam.ScreenToWorldPoint(new Vector3(0, 0, -transform.position.z));

        if (topRight.x > cameraMaxX)
            transform.position = new Vector3(transform.position.x - (topRight.x - cameraMaxX), transform.position.y, transform.position.z);

        if (topRight.y > cameraMaxY)
            transform.position = new Vector3(transform.position.x, transform.position.y - (topRight.y - cameraMaxY), transform.position.z);

        if (bottomLeft.x < cameraMinX)
            transform.position = new Vector3(transform.position.x + (cameraMinX - bottomLeft.x), transform.position.y, transform.position.z);

        if (bottomLeft.y < cameraMinY)
            transform.position = new Vector3(transform.position.x, transform.position.y + (cameraMinY - bottomLeft.y), transform.position.z);
    }
}
