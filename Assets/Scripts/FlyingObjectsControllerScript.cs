using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// CHANGES FOR ANDROID
public class FlyingObjectsControllerScript : MonoBehaviour
{
    [HideInInspector]
    public float speed = 1f;
    public float fadeDuration = 1.5f;
    public float waveAmplitude = 25f;
    public float waveFrequency = 1f;
    private ObjectScript objectScript;
    private ScreenBoundriesScript scrreenBoundriesScript;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isFadingOut = false;
    private bool isExploading = false;
    private Image image;
    private Color originalColor;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        originalColor = image.color;

        objectScript = FindFirstObjectByType<ObjectScript>();
        scrreenBoundriesScript = FindFirstObjectByType<ScreenBoundriesScript>();

        StartCoroutine(FadeIn());
    }

    void Update()
    {
        // ja spēle beigusies, tikai turpina kustību (bez interakcijas)
        bool gameEnded = GameManager.Instance != null && GameManager.Instance.IsGameEnded();

        float waveOffset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;
        rectTransform.anchoredPosition += new Vector2(-speed * Time.deltaTime, waveOffset * Time.deltaTime);

        // <- pa kreisi
        if (speed > 0 && transform.position.x < (scrreenBoundriesScript.minX + 80) && !isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;
        }

        // -> pa labi
        if (speed < 0 && transform.position.x > (scrreenBoundriesScript.maxX - 80) && !isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;
        }

        // 💣 Bombei — ja kursors ir virs, bet tikai ja spēle nav beigusies
        if (!gameEnded && CompareTag("Bomb") && !isExploading &&
            RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Camera.main))
        {
            Debug.Log("The cursor collided with a bomb! (without a car)");
            TriggerExplosion();
        }

        // 🚗 Kolīzija ar auto, ja spēle vēl notiek
        if (!gameEnded && ObjectScript.drag && !isFadingOut &&
            RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, Camera.main))
        {
            Debug.Log("The cursor collided with a flying object!");

            if (ObjectScript.lastDragged != null)
            {
                StartCoroutine(ShrinkAndDestroy(ObjectScript.lastDragged, 0.7f));
                ObjectScript.lastDragged = null;
                ObjectScript.drag = false;

                if (GameManager.Instance != null)
                    GameManager.Instance.OnVehicleDestroyed();
            }

            StartToDestroy();
        }
    }

    bool TryGetInputPosition(out Vector2 position)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
            position = Input.mousePosition;
            return true;

#elif UNITY_ANDROID
            if(Input.touchCount > 0)
            {
                position = Input.GetTouch(0).position;
                return true;
            }
            else
            {
                position = Vector2.zero;
                return false;
            }
#endif
    }

    public void TriggerExplosion()
    {
        // 🛑 ja spēle beigusies, neko vairs nedara
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded())
            return;

        isExploading = true;
        objectScript.effects.PlayOneShot(objectScript.audioCli[15], 1f);

        if (TryGetComponent<Animator>(out Animator animator))
            animator.SetBool("explode", true);

        image.color = Color.red;
        StartCoroutine(RecoverColor(0.4f));

        StartCoroutine(Vibrate());
        StartCoroutine(WaitbeforeExploaded());
    }

    IEnumerator WaitbeforeExploaded()
    {
        float radius = 0f;
        if (TryGetComponent<CircleCollider2D>(out CircleCollider2D circleCollider))
            radius = circleCollider.radius * transform.lossyScale.x;

        ExplodeAndDestroy(radius);
        yield return new WaitForSeconds(1f);
        ExplodeAndDestroy(radius);
        Destroy(gameObject);
    }

    void ExplodeAndDestroy(float radius)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, radius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider != null && hitCollider.gameObject != gameObject)
            {
                FlyingObjectsControllerScript obj = hitCollider.gameObject.GetComponent<FlyingObjectsControllerScript>();

                if (obj != null && !obj.isExploading)
                    obj.StartToDestroy();
            }
        }
    }

    public void StartToDestroy()
    {
        // 🛑 ja spēle beigusies, neko vairs nedara
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded())
            return;

        if (!isFadingOut)
        {
            StartCoroutine(FadeOutAndDestroy());
            isFadingOut = true;

            image.color = Color.cyan;
            StartCoroutine(RecoverColor(0.5f));

            objectScript.effects.PlayOneShot(objectScript.audioCli[14]);
            StartCoroutine(Vibrate());
        }
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOutAndDestroy()
    {
        float t = 0f;
        float startAlpha = canvasGroup.alpha;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
        Destroy(gameObject);
    }

    IEnumerator ShrinkAndDestroy(GameObject target, float duration)
    {
        Vector3 originalScale = target.transform.localScale;
        Quaternion originalRotation = target.transform.rotation;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            target.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t / duration);
            float angle = Mathf.Lerp(0f, 360f, t / duration);
            target.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            yield return null;
        }
        Destroy(target);
    }

    IEnumerator RecoverColor(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        image.color = originalColor;
    }

    IEnumerator Vibrate()
    {
#if UNITY_ANDROID
        Handheld.Vibrate();
#endif

        Vector2 orginalPosition = rectTransform.anchoredPosition;
        float duration = 0.3f;
        float elpased = 0f;
        float intensity = 5f;

        while(elpased < duration)
        {
            rectTransform.anchoredPosition = orginalPosition + Random.insideUnitCircle * intensity;
            elpased += Time.deltaTime;
            yield return null;
        }

    }
}
