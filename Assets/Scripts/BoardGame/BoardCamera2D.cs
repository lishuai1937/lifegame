using UnityEngine;

/// <summary>
/// 2D Camera that follows the player token on the board
/// Orthographic, side-view, smooth follow
/// </summary>
public class BoardCamera2D : MonoBehaviour
{
    public Transform Target;            // PlayerToken
    public float SmoothSpeed = 5f;
    public Vector3 Offset = new Vector3(0, 2, -10);
    public float MinX = 0f;
    public float MinY = 0f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 8;
        }
    }

    void LateUpdate()
    {
        if (Target == null) return;

        // Only follow in BoardGame state
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameState.BoardGame &&
            GameManager.Instance.CurrentState != GameState.MainMenu)
            return;

        Vector3 desired = Target.position + Offset;
        desired.x = Mathf.Max(desired.x, MinX);
        desired.y = Mathf.Max(desired.y, MinY);
        desired.z = Offset.z; // keep z fixed for 2D

        transform.position = Vector3.Lerp(transform.position, desired, SmoothSpeed * Time.deltaTime);
    }
}