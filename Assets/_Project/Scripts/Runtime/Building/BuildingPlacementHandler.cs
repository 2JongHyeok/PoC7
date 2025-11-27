// Purpose: Handles building selection preview and deploy within player range (SRP: building placement flow)
using UnityEngine;
using Sirenix.OdinInspector;

public sealed class BuildingPlacementHandler : MonoBehaviour
{
    [TabGroup("Refs")] [SerializeField] private BuildingController building;
    [TabGroup("Refs")] [SerializeField] private PlayerController player;
    [TabGroup("Refs")] [SerializeField] private Camera viewCamera;
    [TabGroup("Refs")] [SerializeField] private LayerMask groundMask;

    [TabGroup("Input")] [SerializeField] private KeyCode deployKey = KeyCode.Alpha6;

    private bool _isPreviewing;

    private void Awake()
    {
        if (viewCamera == null) viewCamera = Camera.main;
    }

    private void Update()
    {
        if (building == null || player == null) return;

        if (Input.GetKeyDown(deployKey))
        {
            TryEnterPreview();
        }

        if (_isPreviewing)
        {
            UpdatePreviewPosition();

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 placePos = GetClampedCursorPosition();
                building.StartDeploy(placePos);
                _isPreviewing = false;
            }
        }
    }

    private void TryEnterPreview()
    {
        if (!building.IsInInventory || building.IsBusy) return;
        _isPreviewing = true;
        building.BeginPreview();
        building.UpdatePreviewPosition(GetClampedCursorPosition());
    }

    private void UpdatePreviewPosition()
    {
        building.UpdatePreviewPosition(GetClampedCursorPosition());
    }

    private Vector3 GetClampedCursorPosition()
    {
        Vector3 pos = player.transform.position;
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        int mask = groundMask.value == 0 ? Physics.DefaultRaycastLayers : groundMask.value;

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, mask, QueryTriggerInteraction.Ignore))
        {
            pos = hit.point;
        }
        else
        {
            RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, 200f, mask);
            if (hit2D.collider != null)
            {
                pos = hit2D.point;
            }
        }

        return player.ClampPositionToAttackRange(pos);
    }
}
