 using UnityEngine;
using UnityEngine.EventSystems;

public class HighlightObject : MonoBehaviour
{
    private Transform highlight;
    private Transform selection;
    private RaycastHit raycastHit;


    [SerializeField] private Transform playerTransform;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private float maxCircleDist;

    private void Update()
    {
        ClearHighlight();

        bool mouseHighlighted = checkMouseHover();

        if (!mouseHighlighted)
        {
            checkCloseHover();
        }

    }

    private bool checkMouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!EventSystem.current.IsPointerOverGameObject() &&
            Physics.Raycast(ray, out raycastHit, 200f, interactLayer))
        {
            highlight = raycastHit.transform;
            EnableOutline(highlight);
            return true;
        }

        return false;
    }



    private void checkCloseHover()
    {
        Collider[] hits = Physics.OverlapSphere(playerTransform.position, maxCircleDist, interactLayer);

        Collider nearestCollider = null;
        float closestDist = float.MaxValue;

        foreach (Collider hit in hits)
        {
            float distance = Vector3.Distance(playerTransform.position, hit.transform.position);

            if (distance < closestDist)
            {
                closestDist = distance;
                nearestCollider = hit;
            }
        }

        if (nearestCollider != null)
        {
            highlight = nearestCollider.gameObject.transform;
            EnableOutline(highlight);
        }
    }

    private void EnableOutline(Transform obj)
    {
        Outline outline = obj.GetComponent<Outline>();

        if (outline == null)
        {
            outline = obj.gameObject.AddComponent<Outline>();
            outline.OutlineColor = Color.white;
            outline.OutlineWidth = 7f;
        }

        outline.enabled = true;
    }

    private void ClearHighlight()
    {
        if (highlight != null)
        {
            Outline outline = highlight.GetComponent<Outline>();
            if (outline != null)
                outline.enabled = false;

            highlight = null;
        }
    }
}

