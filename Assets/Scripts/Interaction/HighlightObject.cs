 using UnityEngine;
using UnityEngine.EventSystems;

public class HighlightObject : MonoBehaviour
{
    private Transform highlight;
    private Transform selection;
    private RaycastHit raycastHit;


    [SerializeField]private Transform playerTransform;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private float maxCircleDist;

    private void Update()
    {
        if (highlight != null)
        {
            highlight.gameObject.GetComponent<Outline>().enabled = false;
            highlight = null;
        }

        checkMouseHover();
        checkCloseHover();

    }

    private void checkMouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!EventSystem.current.IsPointerOverGameObject() && Physics.Raycast(ray, out raycastHit, 200f, interactLayer))
        {
            highlight = raycastHit.transform;

            if (highlight.gameObject.GetComponent<Outline>() != null)
            {
                highlight.gameObject.GetComponent<Outline>().enabled = true;
            }
            else
            {
                Outline outline = highlight.gameObject.AddComponent<Outline>();
                outline.enabled = true;
                highlight.gameObject.GetComponent<Outline>().OutlineColor = Color.white;
                highlight.gameObject.GetComponent<Outline>().OutlineWidth = 7.0f;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (highlight)
            {
                if (selection != null)
                {
                    selection.gameObject.GetComponent<Outline>().enabled = false;
                }
                selection = raycastHit.transform;
                selection.gameObject.GetComponent<Outline>().enabled = true;
                highlight = null;
            }
            else
            {
                if (selection)
                {
                    selection.gameObject.GetComponent<Outline>().enabled = false;
                    selection = null;
                }
            }
        }
    }

    private void checkCloseHover()
    {
        Collider[] hits = Physics.OverlapSphere(playerTransform.position, maxCircleDist, interactLayer);

        Collider nearestCollider = null;
        float closestDist = float.MaxValue;

        foreach(Collider hit in hits)
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

            if (highlight.gameObject.GetComponent<Outline>() != null)
            {
                highlight.gameObject.GetComponent<Outline>().enabled = true;
            }
            else
            {
                Outline outline = highlight.gameObject.AddComponent<Outline>();
                outline.enabled = true;
                highlight.gameObject.GetComponent<Outline>().OutlineColor = Color.white;
                highlight.gameObject.GetComponent<Outline>().OutlineWidth = 7.0f;
            }
        }

    }
 
}
