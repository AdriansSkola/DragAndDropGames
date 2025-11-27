using UnityEngine;

// Handles dragging a block with the mouse (or touch via Unity's Input system using mouse events).
// When dropped near a peg, it tries to move the selected block via TowerManager.TryMoveSelectedToPeg.
// If the move fails or the block is not released near a peg, it will revert to its original position.
public class BlockDrag : MonoBehaviour
{
    private TowerManager manager;
    private Block block;
    private Camera cam;

    private Vector3 originalPosition;
    private Transform originalParent;
    private Vector2 originalAnchoredPosition;
    private float pickZ = 0f;
    private bool dragging = false;

    [Tooltip("Maximum distance (world units) from a peg for a drop to snap to that peg")]
    public float snapDistance = 1.0f;
    [Tooltip("Maximum distance in screen pixels used when the block or pegs are UI elements (RectTransform).")]
    public float snapDistancePixels = 120f;
    [Tooltip("Enable extra snap debug logs")] 
    public bool debugSnap = false;

    private Canvas canvas;
    private RectTransform dragLayerRect;
    private RectTransform myRect;
    private bool isUIElement = false;

    private void Start()
    {
        manager = UnityEngine.Object.FindAnyObjectByType<TowerManager>();
        block = GetComponent<Block>();
        cam = Camera.main;
        myRect = GetComponent<RectTransform>();
        isUIElement = myRect != null;

        if (isUIElement)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                var layer = canvas.transform.Find("DragLayer");
                if (layer == null)
                {
                    var go = new GameObject("DragLayer", typeof(RectTransform));
                    go.transform.SetParent(canvas.transform, false);
                    var rt = go.GetComponent<RectTransform>();
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    layer = go.transform;
                }
                dragLayerRect = layer as RectTransform;
            }
        }
    }

    private void OnMouseDown()
    {
        if (manager == null || block == null)
            return;

        if (!block.isTopBlock)
            return; // only top blocks can be dragged

        Debug.Log($"BlockDrag: OnMouseDown block(size={block.size}, peg={block.currentPeg}, isTop={block.isTopBlock})");

        // record start transform so we can revert if needed
        originalPosition = transform.position;
        originalParent = transform.parent;

        if (isUIElement && myRect != null)
        {
            originalAnchoredPosition = myRect.anchoredPosition;
        }

        // ensure a rigidbody exists and put the block into kinematic mode so we can move it directly
        block.EnsureRigidbody();
        block.SetKinematic(true);

        // detach from parent so the block can move freely
        if (isUIElement && dragLayerRect != null)
        {
            // move into DragLayer so the element stays visible inside canvas
            transform.SetParent(dragLayerRect, false);
            transform.SetAsLastSibling();
        }
        else
        {
            transform.SetParent(null, true);
        }
        dragging = true;

        // also select the block in the manager so UI/logic stay in sync
        manager.SelectBlock(block);

        // compute depth for ScreenToWorldPoint
        if (cam != null)
            pickZ = cam.WorldToScreenPoint(transform.position).z;
    }

    private void OnMouseDrag()
    {
        if (!dragging) return;

        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        if (isUIElement && canvas != null && dragLayerRect != null)
        {
            var camForCanvas = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayerRect, Input.mousePosition, camForCanvas, out Vector2 localPoint);
            myRect.anchoredPosition = localPoint;
        }
        else
        {
            Vector3 screen = Input.mousePosition;
            screen.z = pickZ;
            Vector3 world = cam.ScreenToWorldPoint(screen);
            transform.position = world;
        }
    }

    private void OnMouseUp()
    {
        if (!dragging) return;
        dragging = false;

        if (manager == null || block == null)
            return;

        // find closest peg using a normalized score: distance / threshold
        int nearestPeg = -1;
        float bestScore = float.MaxValue; // score <= 1 means within threshold
        float bestDist = float.MaxValue;
        bool usingUiDistance = false;

        // helper to evaluate a peg
        System.Action<Transform, int> evalPeg = (peg, pegIndex) =>
        {
            if (peg == null) return;
            var camForCanvas = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;
            Vector2 screenA = RectTransformUtility.WorldToScreenPoint(camForCanvas, myRect != null ? myRect.position : transform.position);
            Vector2 screenB = RectTransformUtility.WorldToScreenPoint(camForCanvas, peg.position);
            float screenDist = Vector2.Distance(screenA, screenB);
            float worldDist = Vector3.Distance(transform.position, peg.position);
            bool pegIsUI = isUIElement || peg is RectTransform;
            float score;
            if (pegIsUI)
                score = screenDist / Mathf.Max(1f, snapDistancePixels);
            else
                score = worldDist / Mathf.Max(0.0001f, snapDistance);

            if (debugSnap)
                Debug.Log($"BlockDrag: evalPeg {pegIndex} screenDist={screenDist:F1} worldDist={worldDist:F2} pegIsUI={pegIsUI} score={score:F3}");

            if (score < bestScore)
            {
                bestScore = score;
                nearestPeg = pegIndex;
                usingUiDistance = pegIsUI;
                bestDist = pegIsUI ? screenDist : worldDist;
            }
        };

        evalPeg(manager.peg1, 1);
        evalPeg(manager.peg2, 2);
        evalPeg(manager.peg3, 3);

        if (nearestPeg != -1 && bestScore <= 1f)
        {
            Debug.Log($"BlockDrag: OnMouseUp nearestPeg={nearestPeg} bestDist={bestDist} usingUiDistance={usingUiDistance} threshold={(usingUiDistance?snapDistancePixels:snapDistance)}");
            // try to move using manager logic; if move fails, we revert
            Debug.Log($"BlockDrag: attempting move to peg {nearestPeg} for block size {block.size}");
            bool moved = manager.TryMoveSelectedToPeg(nearestPeg);
            Debug.Log($"BlockDrag: move result = {moved}");
            if (!moved)
            {
                Debug.Log("BlockDrag: move denied or failed, reverting position.");
                if (isUIElement && myRect != null)
                    block.ResetTransform(originalPosition, originalParent, originalAnchoredPosition);
                else
                    block.ResetTransform(originalPosition, originalParent);
                manager.DeselectCurrentBlock();
            }
        }
        else
        {
            Debug.Log($"BlockDrag: not near a peg (nearestPeg={nearestPeg}, bestDist={bestDist}). Reverting.");
            // not near a peg: revert position
            if (isUIElement && myRect != null)
                block.ResetTransform(originalPosition, originalParent, originalAnchoredPosition);
            else
                block.ResetTransform(originalPosition, originalParent);
            manager.DeselectCurrentBlock();
        }
    }
}
