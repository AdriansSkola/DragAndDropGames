using UnityEngine;

public class Block : MonoBehaviour
{
    public int size;            // 1 = smallest
    public int currentPeg;

    public bool isTopBlock = false;

    private SpriteRenderer sr;
    private UnityEngine.UI.Image uiImage;
    private Renderer meshRenderer;
    private Rigidbody rb3D;
    private Rigidbody2D rb2D;

    private Color baseSpriteColor = Color.white;
    private Color baseImageColor = Color.white;
    private Color baseMaterialColor = Color.white;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        uiImage = GetComponent<UnityEngine.UI.Image>();
        meshRenderer = GetComponent<Renderer>();

        // Physics components: try 3D then 2D
        rb3D = GetComponent<Rigidbody>();
        rb2D = GetComponent<Rigidbody2D>();

        if (sr != null)
            baseSpriteColor = sr.color;

        if (uiImage != null)
            baseImageColor = uiImage.color;

        if (meshRenderer != null && meshRenderer.material != null)
            baseMaterialColor = meshRenderer.material.color;
    }

    /// <summary>
    /// Ensure a Rigidbody (3D or 2D) exists on the block. Will prefer existing components and adds none by default.
    /// </summary>
    public void EnsureRigidbody()
    {
        if (rb3D == null && rb2D == null)
        {
            rb3D = GetComponent<Rigidbody>();
            rb2D = GetComponent<Rigidbody2D>();
        }
    }

    /// <summary>
    /// Make the block kinematic (attached/snapped) or dynamic (free) depending on physics type.
    /// </summary>
    public void SetKinematic(bool makeKinematic)
    {
        EnsureRigidbody();
        if (rb3D != null)
        {
            rb3D.isKinematic = makeKinematic;
            if (makeKinematic)
            {
                // zero velocities when kinematic
                rb3D.linearVelocity = Vector3.zero;
                rb3D.angularVelocity = Vector3.zero;
            }
        }
        if (rb2D != null)
        {
            rb2D.bodyType = makeKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            if (makeKinematic)
            {
                rb2D.linearVelocity = Vector2.zero;
                rb2D.angularVelocity = 0f;
            }
        }
    }

    public void SetHighlight(bool on, bool selected = false)
    {
        // Prefer Image (UI), fall back to SpriteRenderer, then to other Renderer
        if (uiImage == null && sr == null && meshRenderer == null)
        {
            uiImage = GetComponent<UnityEngine.UI.Image>();
            if (uiImage == null)
            {
                sr = GetComponent<SpriteRenderer>();
                meshRenderer = GetComponent<Renderer>();
            }
        }

        // If nothing to color, nothing to do
        if (uiImage == null && sr == null && (meshRenderer == null || meshRenderer.material == null))
            return;

        if (on)
        {
            if (selected)
            {
                float alpha = 0.85f;
                if (uiImage != null)
                {
                    var c = uiImage.color;
                    c.a = alpha;
                    uiImage.color = c;
                }
                else if (sr != null)
                {
                    var c = sr.color;
                    c.a = alpha;
                    sr.color = c;
                }
                else if (meshRenderer != null && meshRenderer.material != null)
                {
                    var c = meshRenderer.material.color;
                    c.a = alpha;
                    meshRenderer.material.color = c; // note: creates instance of material at runtime
                }
            }
            else
            {
                // Non-selected highlight: keep fully opaque and no scale change
                if (uiImage != null)
                {
                    var c = uiImage.color;
                    c.a = 1f;
                    uiImage.color = c;
                }
                else if (sr != null)
                {
                    var c = sr.color;
                    c.a = 1f;
                    sr.color = c;
                }
                else if (meshRenderer != null && meshRenderer.material != null)
                {
                    var c = meshRenderer.material.color;
                    c.a = 1f;
                    meshRenderer.material.color = c;
                }
            }
        }
        else
        {
            // restore original colors
            if (uiImage != null)
                uiImage.color = baseImageColor;
            if (sr != null)
                sr.color = baseSpriteColor;
            if (meshRenderer != null && meshRenderer.material != null)
                meshRenderer.material.color = baseMaterialColor;
        }
    }

    // Helper: Reset transform position and parent cleanly (useful when a drag fails and we want to revert)
    public void ResetTransform(Vector3 position, Transform parent)
    {
        transform.SetParent(parent, true);
        transform.position = position;
        SetKinematic(true);
    }

    // UI-aware reset: if the GameObject has a RectTransform and parent is a RectTransform, restore anchored position.
    public void ResetTransform(Vector3 position, Transform parent, Vector2 anchoredPosition)
    {
        var rt = GetComponent<RectTransform>();
        if (rt != null && parent is RectTransform)
        {
            transform.SetParent(parent, false);
            rt.anchoredPosition = anchoredPosition;
        }
        else
        {
            ResetTransform(position, parent);
        }
        SetKinematic(true);
    }
}
