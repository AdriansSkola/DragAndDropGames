using UnityEngine;

public class BlockClick : MonoBehaviour
{
    private TowerManager manager;
    private Block block;

    private void Start()
    {
        manager = UnityEngine.Object.FindAnyObjectByType<TowerManager>();
        block = GetComponent<Block>();
    }

    private void OnMouseDown()
    {
        if (manager == null || block == null)
            return;

        manager.SelectBlock(block);
    }
}
