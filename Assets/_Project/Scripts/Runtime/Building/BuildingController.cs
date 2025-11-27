// Purpose: Manages single building lifecycle: select, fold (pack), preview, deploy (SRP: building state controller)
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public sealed class BuildingController : MonoBehaviour
{
    [TabGroup("Refs")] [SerializeField] private SpriteRenderer buildingRenderer;
    [TabGroup("Refs")] [SerializeField] private Collider2D hitCollider;
    [TabGroup("Refs")] [SerializeField] private Slider slider; // existing slider in scene/UI
    [TabGroup("Refs")] [SerializeField] private Button foldButton;

    [TabGroup("Tuning")] [SerializeField] private float foldDuration = 2f;
    [TabGroup("Tuning")] [SerializeField] private float deployDuration = 2f;
    [TabGroup("Tuning")] [SerializeField] private float selectedAlpha = 0.5f;
    [TabGroup("Tuning")] [SerializeField] private float previewAlpha = 0.35f;

    public event System.Action<BuildingController> OnSelected;
    public event System.Action<BuildingController> OnFoldComplete;
    public event System.Action<BuildingController> OnDeployComplete;

    public bool IsInInventory => _inInventory;
    public bool IsBusy => _busy;

    private bool _inInventory;
    private bool _busy;
    private bool _isPreview;

    private void Awake()
    {
        if (buildingRenderer == null) buildingRenderer = GetComponentInChildren<SpriteRenderer>();
        if (hitCollider == null) hitCollider = GetComponent<Collider2D>();
        UpdateFoldButtonState();
    }

    private void OnMouseDown()
    {
        if (_busy || _inInventory) return;
        Select();
    }

    public void Select()
    {
        if (_busy) return;
        SetAlpha(selectedAlpha);
        OnSelected?.Invoke(this);
        UpdateFoldButtonState();
    }

    public void ClearSelection()
    {
        if (_busy || _inInventory || _isPreview) return;
        SetAlpha(1f);
        UpdateFoldButtonState();
    }

    public void Fold()
    {
        if (_busy || _inInventory) return;
        SetFoldButtonInteractable(false);
        StartCoroutine(FoldRoutine());
    }

    public void BeginPreview()
    {
        _isPreview = true;
        _inInventory = true;
        _busy = false;
        hitCollider.enabled = false;
        gameObject.SetActive(true);
        SetAlpha(previewAlpha);
        UpdateFoldButtonState();
    }

    public void UpdatePreviewPosition(Vector3 position)
    {
        if (!_isPreview) return;
        transform.position = position;
    }

    public void StartDeploy(Vector3 position)
    {
        if (!_inInventory || _busy) return;
        StartCoroutine(DeployRoutine(position));
    }

    private IEnumerator FoldRoutine()
    {
        _busy = true;
        SpawnSlider(1f);
        float elapsed = 0f;
        while (elapsed < foldDuration)
        {
            elapsed += Time.deltaTime;
            UpdateSlider(1f - Mathf.Clamp01(elapsed / foldDuration));
            UpdateSliderPosition();
            yield return null;
        }
        ClearSlider();
        hitCollider.enabled = false;
        SetAlpha(0f);
        _inInventory = true;
        _busy = false;
        UpdateFoldButtonState();
        OnFoldComplete?.Invoke(this);
    }

    private IEnumerator DeployRoutine(Vector3 position)
    {
        _busy = true;
        _isPreview = false;
        transform.position = position;
        UpdateFoldButtonState();
        SpawnSlider(0f);
        float elapsed = 0f;
        while (elapsed < deployDuration)
        {
            elapsed += Time.deltaTime;
            UpdateSlider(Mathf.Clamp01(elapsed / deployDuration));
            UpdateSliderPosition();
            yield return null;
        }
        ClearSlider();
        SetAlpha(1f);
        hitCollider.enabled = true;
        _inInventory = false;
        _busy = false;
        SetFoldButtonInteractable(true);
        OnDeployComplete?.Invoke(this);
        UpdateFoldButtonState();
    }

    private void SpawnSlider(float initialValue)
    {
        if (slider == null) return;
        slider.gameObject.SetActive(true);
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = initialValue;
    }

    private void UpdateSlider(float normalizedValue)
    {
        if (slider == null) return;
        slider.value = normalizedValue;
    }

    private void ClearSlider()
    {
        if (slider == null) return;
        slider.gameObject.SetActive(false);
    }

    private void UpdateSliderPosition()
    {
        if (slider == null) return;
        // Position managed externally (UI layout)
    }

    private void SetAlpha(float alpha)
    {
        if (buildingRenderer == null) return;
        Color c = buildingRenderer.color;
        c.a = alpha;
        buildingRenderer.color = c;
    }

    private void SetFoldButtonInteractable(bool interactable)
    {
        if (foldButton != null)
        {
            foldButton.interactable = interactable;
        }
    }

    private void UpdateFoldButtonState()
    {
        bool canFold = !_busy && !_inInventory && !_isPreview;
        SetFoldButtonInteractable(canFold);
    }
}
