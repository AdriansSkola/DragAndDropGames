using UnityEngine;
using System.Collections.Generic;

public class TowerManager : MonoBehaviour
{
    [Header("Pegs")]
    public Transform peg1;
    public Transform peg2;
    public Transform peg3;

    [Header("Peg Anchors")]
    public Transform[] peg1Anchors;
    public Transform[] peg2Anchors;
    public Transform[] peg3Anchors;

    [Header("Blocks")]
    public List<Block> allBlocks;

    private List<Block> peg1Blocks = new List<Block>();
    private List<Block> peg2Blocks = new List<Block>();
    private List<Block> peg3Blocks = new List<Block>();

    private Block selectedBlock;
    private int moveCount = 0;
    private bool puzzleSolved = false;

    [Header("UI (optional)")]
    public UnityEngine.UI.Text movesText;
    public GameObject winPanel;
    public UnityEngine.UI.Text winText;

    private System.Random rnd = new System.Random();

    private void Start()
    {
        // make sure allBlocks is populated (in case the inspector list wasn't filled)
        if (allBlocks == null || allBlocks.Count == 0)
        {
            var found = UnityEngine.Object.FindObjectsByType<Block>(UnityEngine.FindObjectsSortMode.None);
            allBlocks = new List<Block>(found);
        }

        if (winPanel != null)
            winPanel.SetActive(false);

        InitializeRandomBlocks();
        UpdateUI();
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    // Randomize block distribution while keeping valid stacks (bigger at bottom)
    private void InitializeRandomBlocks()
    {
        peg1Blocks.Clear();
        peg2Blocks.Clear();
        peg3Blocks.Clear();

        if (allBlocks == null || allBlocks.Count == 0)
            return;

        // assign each block randomly to a peg
        foreach (var b in allBlocks)
        {
            int peg = rnd.Next(1, 4); // 1..3
            b.SetHighlight(false);
            b.isTopBlock = false;
            b.currentPeg = peg;
            switch (peg)
            {
                case 1: peg1Blocks.Add(b); break;
                case 2: peg2Blocks.Add(b); break;
                case 3: peg3Blocks.Add(b); break;
            }
        }

        // For each peg, ensure blocks are sorted with largest at bottom and placed on anchors
        PlaceBlocksOnPeg(peg1Blocks, peg1Anchors);
        PlaceBlocksOnPeg(peg2Blocks, peg2Anchors);
        PlaceBlocksOnPeg(peg3Blocks, peg3Anchors);

        moveCount = 0;
        puzzleSolved = false;
    }

    private void PlaceBlocksOnPeg(List<Block> pegList, Transform[] anchors)
    {
        if (pegList == null)
            return;

        // Sort descending by size (largest first so bottom is index 0)
        pegList.Sort((a, b) => b.size.CompareTo(a.size));

        for (int i = 0; i < pegList.Count; i++)
        {
            var block = pegList[i];
            if (anchors != null && anchors.Length > i && anchors[i] != null)
            {
                block.transform.position = anchors[i].position;
                block.transform.SetParent(anchors[i], true);
            }
            else if (peg1 != null)
            {
                // fallback parenting so the scene stays tidy
                block.transform.SetParent(peg1, true);
            }
            block.isTopBlock = (i == pegList.Count - 1); // top is last
            block.currentPeg = GetPegNumberForList(pegList);
            block.SetHighlight(false);
        }
    }

    private int GetPegNumberForList(List<Block> list)
    {
        if (list == peg1Blocks) return 1;
        if (list == peg2Blocks) return 2;
        if (list == peg3Blocks) return 3;
        return 1;
    }

    private void UpdateUI()
    {
        if (movesText != null)
            movesText.text = "Moves: " + moveCount;
    }

    private List<Block> GetPegList(int pegNumber)
    {
        switch (pegNumber)
        {
            case 1: return peg1Blocks;
            case 2: return peg2Blocks;
            case 3: return peg3Blocks;
            default: return peg1Blocks;
        }
    }

    private Transform[] GetAnchorsForPeg(int pegNumber)
    {
        switch (pegNumber)
        {
            case 1: return peg1Anchors;
            case 2: return peg2Anchors;
            case 3: return peg3Anchors;
            default: return peg1Anchors;
        }
    }

    // Called by BlockClick when a block is tapped
    public void SelectBlock(Block b)
    {
        if (puzzleSolved) return;
        if (b == null) return;

        // If player taps a top block, select/deselect it
        if (b.isTopBlock)
        {
            if (selectedBlock == b)
            {
                selectedBlock.SetHighlight(false);
                selectedBlock = null;
            }
            else
            {
                if (selectedBlock != null)
                    selectedBlock.SetHighlight(false);

                selectedBlock = b;
                selectedBlock.SetHighlight(true, true);
            }
        }
        else
        {
            // ignore taps on non-top blocks
            return;
        }
    }

    // Called by PegClick when a peg is tapped
    public void OnPegClicked(int pegNumber)
    {
        if (puzzleSolved) return;

        // If nothing selected, select the top block on that peg
        if (selectedBlock == null)
        {
            var top = GetTopBlock(pegNumber);
            if (top != null)
                SelectBlock(top);
            return;
        }

        // Try to move selected block to clicked peg
        TryMoveSelectedToPeg(pegNumber);
    }

    private Block GetTopBlock(int pegNumber)
    {
        var list = GetPegList(pegNumber);
        if (list == null || list.Count == 0) return null;
        return list[list.Count - 1];
    }

    private void TryMoveSelectedToPeg(int pegNumber)
    {
        if (selectedBlock == null) return;

        int fromPeg = selectedBlock.currentPeg;
        if (fromPeg == pegNumber)
        {
            // deselect - tapping same peg does nothing
            selectedBlock.SetHighlight(false);
            selectedBlock = null;
            return;
        }

        var destList = GetPegList(pegNumber);
        var destTop = GetTopBlock(pegNumber);

        // allowed if dest empty OR selected smaller than dest top
        if (destTop == null || selectedBlock.size < destTop.size)
        {
            // remove from source list
            var fromList = GetPegList(fromPeg);
            if (fromList != null)
                fromList.Remove(selectedBlock);

            // add to dest
            destList.Add(selectedBlock);

            // reposition all blocks on both pegs to anchors
            PlaceBlocksOnPeg(fromList, GetAnchorsForPeg(fromPeg));
            PlaceBlocksOnPeg(destList, GetAnchorsForPeg(pegNumber));

            moveCount++;
            UpdateUI();

            // clear highlight
            selectedBlock.SetHighlight(false);
            selectedBlock = null;

            CheckWinCondition();
        }
        else
        {
            // cannot move, maybe give feedback later
            // keep selection but flash highlight briefly
        }
    }

    private void CheckWinCondition()
    {
        if (allBlocks != null && peg3Blocks.Count == allBlocks.Count)
        {
            puzzleSolved = true;
            if (winPanel != null)
                winPanel.SetActive(true);
            if (winText != null)
            {
                winText.text = "You win! Moves: " + moveCount;
            }

            Debug.Log("Puzzle Solved in " + moveCount + " moves!");

            // update moves UI too
            UpdateUI();
        }
    }

    // Reduce the recorded move count (used by rewarded ad) and update UI.
    // This is public so external systems (ads, powerups) can grant move reductions.
    public void ReduceMoves(int amount)
    {
        if (puzzleSolved) // no effect if already solved
            return;

        if (amount <= 0)
            return;

        moveCount = Mathf.Max(0, moveCount - amount);
        UpdateUI();
        Debug.Log($"TowerManager: reduced moves by {amount}. New move count: {moveCount}");
    }
}