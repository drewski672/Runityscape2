using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    public CinemachineFreeLook freeLookCam;
    public float rotationSpeed = 10f;
    public float zoomSpeed = 10f;

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            freeLookCam.m_XAxis.Value -= rotationSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            freeLookCam.m_XAxis.Value += rotationSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.W))
        {
            freeLookCam.m_YAxis.Value += zoomSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            freeLookCam.m_YAxis.Value -= zoomSpeed * Time.deltaTime;
        }
    }
}
