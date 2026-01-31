using System.Collections;
using UnityEngine;

public class CameraCinematic2D : MonoBehaviour
{
    public float normalZoom = 5f;
    public float fusionZoom = 3f;
    public float zoomSpeed = 4f;

    public float slowTimeScale = 0.2f;
    public float slowDuration = 0.6f;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
        _cam.orthographicSize = normalZoom;
    }

    public IEnumerator PlayFusionCinematic()
    {
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        while (_cam.orthographicSize > fusionZoom)
        {
            _cam.orthographicSize -= zoomSpeed * Time.unscaledDeltaTime;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(slowDuration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        while (_cam.orthographicSize < normalZoom)
        {
            _cam.orthographicSize += zoomSpeed * Time.deltaTime;
            yield return null;
        }
    }
}
