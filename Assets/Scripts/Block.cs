using UnityEngine;

public class Block : MonoBehaviour
{
    public int size;            // 1 = smallest
    public int currentPeg;

    public bool isTopBlock = false;

    private SpriteRenderer sr;
    private UnityEngine.UI.Image uiImage;
    private Renderer meshRenderer;

    private Color baseSpriteColor = Color.white;
    private Color baseImageColor = Color.white;
    private Color baseMaterialColor = Color.white;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        uiImage = GetComponent<UnityEngine.UI.Image>();
        meshRenderer = GetComponent<Renderer>();

        if (sr != null)
            baseSpriteColor = sr.color;

        if (uiImage != null)
            baseImageColor = uiImage.color;

        if (meshRenderer != null && meshRenderer.material != null)
            baseMaterialColor = meshRenderer.material.color;
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
}
