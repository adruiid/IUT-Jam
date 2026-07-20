using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Mouse-wheel zoom for a Cinemachine 3rd-Person-Follow camera. Version-agnostic: it
/// finds whichever body component on the camera exposes a float "CameraDistance"
/// (works with both the deprecated Cinemachine3rdPersonFollow and the CM3
/// CinemachineThirdPersonFollow) and eases it between min/max on scroll.
///
/// Put this on the Cinemachine camera (vcam) object, or set Camera Object to it.
/// </summary>
[DisallowMultipleComponent]
public class CameraZoom : MonoBehaviour
{
    [Tooltip("Object holding the Cinemachine body component. Defaults to this object.")]
    [SerializeField] private GameObject cameraObject;

    [Header("Zoom")]
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 10f;
    [Tooltip("Distance change per scroll notch.")]
    [SerializeField] private float zoomStep = 1f;
    [Tooltip("Ease time toward the target distance (smaller = snappier).")]
    [SerializeField] private float smoothTime = 0.12f;
    [Tooltip("Scroll up zooms IN (closer). Turn off to invert.")]
    [SerializeField] private bool scrollUpZoomsIn = true;

    private object _body;      // the body component instance
    private FieldInfo _field;  // its CameraDistance field...
    private PropertyInfo _prop; // ...or property
    private float _target;
    private float _vel;

    private void Awake()
    {
        GameObject go = cameraObject != null ? cameraObject : gameObject;

        foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (mb == null) continue;
            var type = mb.GetType();

            var f = type.GetField("CameraDistance");
            if (f != null && f.FieldType == typeof(float)) { _body = mb; _field = f; break; }

            var p = type.GetProperty("CameraDistance");
            if (p != null && p.PropertyType == typeof(float) && p.CanRead && p.CanWrite) { _body = mb; _prop = p; break; }
        }

        if (_body == null)
            Debug.LogWarning($"[CameraZoom DIAG] no component with a float 'CameraDistance' found under '{go.name}'. " +
                             "Put this on the vcam (PlayerFollowCamera) or set Camera Object to it.", this);
        else
            Debug.Log($"[CameraZoom DIAG] using '{((MonoBehaviour)_body).GetType().Name}' on '{((MonoBehaviour)_body).gameObject.name}'", this);
    }

    private void Start()
    {
        _target = GetDistance();
    }

    private void Update()
    {
        if (_body == null || Mouse.current == null) return;

        float scroll = Mouse.current.scroll.ReadValue().y; // ~120 per notch on Windows
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float dir = Mathf.Sign(scroll) * (scrollUpZoomsIn ? -1f : 1f);
            _target = Mathf.Clamp(_target + dir * zoomStep, minDistance, maxDistance);
            Debug.Log($"[CameraZoom DIAG] scroll={scroll}, target={_target:F2}, dist={GetDistance():F2}", this);
        }

        SetDistance(Mathf.SmoothDamp(GetDistance(), _target, ref _vel, smoothTime));
    }

    private float GetDistance()
    {
        if (_field != null) return (float)_field.GetValue(_body);
        if (_prop != null) return (float)_prop.GetValue(_body);
        return 0f;
    }

    private void SetDistance(float value)
    {
        if (_field != null) _field.SetValue(_body, value);
        else if (_prop != null) _prop.SetValue(_body, value);
    }
}
