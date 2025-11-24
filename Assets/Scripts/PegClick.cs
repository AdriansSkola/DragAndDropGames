using UnityEngine;

public class PegClick : MonoBehaviour
{
    public int pegNumber; // 1,2,3
    private TowerManager manager;
    private void Start()
    {
        manager = UnityEngine.Object.FindAnyObjectByType<TowerManager>();
    }

    private void OnMouseDown()
    {
        if (manager == null)
            return;

        manager.OnPegClicked(pegNumber);
    }
}
