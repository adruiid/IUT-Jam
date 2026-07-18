using System.Collections;
using UnityEngine;

/// <summary>
/// Briefly tints an enemy's renderers reddish when hit. Uses a MaterialPropertyBlock
/// (no material instancing) and auto-hooks to EnemyHealth.onDamaged, so just drop it
/// on the enemy prefab. Works across pipelines by picking whichever color property
/// the material exposes (_BaseColor for URP/HDRP, _Color for Built-in).
/// </summary>
[DisallowMultipleComponent]
public class HitFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = new Color(1f, 0.25f, 0.25f);
    [SerializeField] private float flashDuration = 0.08f;
    [Tooltip("Automatically flash when this object's EnemyHealth takes damage.")]
    [SerializeField] private bool autoHookEnemyHealth = true;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private Renderer[] _renderers;
    private int[] _colorProp;   // per renderer: the color property id (0 = none)
    private Color[] _original;  // per renderer: original color to restore
    private MaterialPropertyBlock _mpb;
    private Coroutine _routine;
    private EnemyHealth _health;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _colorProp = new int[_renderers.Length];
        _original = new Color[_renderers.Length];
        _mpb = new MaterialPropertyBlock();

        for (int i = 0; i < _renderers.Length; i++)
        {
            var mat = _renderers[i].sharedMaterial;
            if (mat != null && mat.HasProperty(BaseColorId))
            {
                _colorProp[i] = BaseColorId;
                _original[i] = mat.GetColor(BaseColorId);
            }
            else if (mat != null && mat.HasProperty(ColorId))
            {
                _colorProp[i] = ColorId;
                _original[i] = mat.GetColor(ColorId);
            }
            else
            {
                _colorProp[i] = 0; // no tintable color -> skip
            }
        }

        if (autoHookEnemyHealth)
        {
            _health = GetComponent<EnemyHealth>();
            if (_health != null) _health.onDamaged.AddListener(Flash);
        }
    }

    private void OnDestroy()
    {
        if (_health != null) _health.onDamaged.RemoveListener(Flash);
    }

    /// <summary>Flash the enemy reddish. Safe to call rapidly (restarts the timer).</summary>
    public void Flash()
    {
        if (!isActiveAndEnabled) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        ApplyFlash();
        yield return new WaitForSeconds(flashDuration);
        RestoreColors();
        _routine = null;
    }

    private void ApplyFlash()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_colorProp[i] == 0 || _renderers[i] == null) continue;
            _renderers[i].GetPropertyBlock(_mpb);
            _mpb.SetColor(_colorProp[i], flashColor);
            _renderers[i].SetPropertyBlock(_mpb);
        }
    }

    private void RestoreColors()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_colorProp[i] == 0 || _renderers[i] == null) continue;
            _renderers[i].GetPropertyBlock(_mpb);
            _mpb.SetColor(_colorProp[i], _original[i]);
            _renderers[i].SetPropertyBlock(_mpb);
        }
    }
}
