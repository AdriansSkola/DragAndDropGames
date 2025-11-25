using UnityEngine;
using UnityEngine.SceneManagement;

public class TowerCamera : MonoBehaviour
{
    void Awake()
    {
        // Apply orientation immediately for the currently active scene
        ApplyOrientationForScene(SceneManager.GetActiveScene());

        // Listen for future scene changes to update orientation dynamically
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyOrientationForScene(scene);
    }

    private void ApplyOrientationForScene(Scene scene)
    {
        if (scene.name != null && scene.name.ToLower().Contains("tower"))
        {
            // Force landscape for tower scenes (exact or contains 'tower')
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }
        else
        {
            // Allow any rotation for other scenes using auto-rotation
            Screen.orientation = ScreenOrientation.AutoRotation;

            // Ensure all rotations are allowed so the app rotates freely
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }
    }
}
