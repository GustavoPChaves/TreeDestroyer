using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Game Manager controls the gameplay loop: rounds and trees creation.
/// Just for simplicity Game Manager Updates the UI and Implements the action button, because there're only one button and one label.
/// I'd rather move the tree than the camera to prevent the camera to move too far from origin (0, 0, 0)
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The size of the trunk model to properly create the tree
    /// </summary>
    private const float TrunkSize = 5;

    [SerializeField] private ObjectPool _trunkPool;
    [SerializeField] private AnimationController _animationController;
    [SerializeField] private Text _roundText;

    /// <summary>
    /// Position of the tree when it is far from camera
    /// </summary>
    [SerializeField] private Vector3 _treeOriginPosition = new Vector3(5, 0, 15);

    private GameObject tree;

    private int _treesPerRound;
    private int _roundCount = 1;
    private int _collapsedTrunks;
    private int _treeSize;

    /// <summary>
    /// Prevents from colapse a tree that is not ready
    /// </summary>
    private bool _canColapse;

    /// <summary>
    /// Setup the round and generate a tree
    /// </summary>
    void Start()
    {
        tree = new GameObject("Tree");
        _treesPerRound = Random.Range(2, 5);
        GenerateTree();
    }

    /// <summary>
    /// Generate a tree with random size and animates it
    /// </summary>
    void GenerateTree()
    {
        _treeSize = Random.Range(8, 16);
        _canColapse = false;
        CreateTree(_treeSize, AnimateTreePoppingOutAndEnter);
    }

    /// <summary>
    /// Generate a tree with size and execute a action at the end
    /// </summary>
    /// <param name="treeSize">The size of the tree</param>
    /// <param name="completition">Action to be executed at the end</param>
    void CreateTree(int treeSize, Action completition = null)
    {
        tree.transform.position = Vector3.zero;

        for (int i = 1; i <= treeSize; i++)
        {
            var temp = _trunkPool.GetPooledObject(Vector3.up * i * TrunkSize, tree.transform);
            temp.SetActive(true);
        }
        completition?.Invoke();
    }

    /// <summary>
    /// Animate the tree to pop from ground and move to front of the camera
    /// </summary>
    void AnimateTreePoppingOutAndEnter()
    {
        var positionBelowGround = new Vector3(5, -TrunkSize * _treeSize, 15);
        _animationController.PoppingOutAnimation(tree.transform, positionBelowGround, _treeOriginPosition, () => { _canColapse = true; });
    }

    /// <summary>
    /// Collapse the tree by removing the lower trunk, Called by UI Button
    /// </summary>
    public void CollapseTree()
    {
        if (!_canColapse) return;

        _animationController.CollapseAnimation(tree.transform, tree.transform.position, tree.transform.position - (Vector3.up * TrunkSize));
        RemoveTrunk();
        CheckTreeSize();
    }

    /// <summary>
    /// Removes the lower trunk and release it for reuse
    /// </summary>
    void RemoveTrunk()
    {
        _collapsedTrunks++;
        var temp = tree.transform.GetChild(0);
        temp.gameObject.SetActive(false);
        temp.SetParent(_trunkPool.transform);
    }

    /// <summary>
    /// Check if the tree is over
    /// </summary>
    void CheckTreeSize()
    {
        if (_collapsedTrunks == _treeSize)
        {
            _collapsedTrunks = 0;
            _treesPerRound--;
            CheckRoundOver();
        }
    }

    /// <summary>
    /// Check if the round is over and updates the UI
    /// </summary>
    void CheckRoundOver()
    {
        if (_treesPerRound <= 0)
        {
            _roundCount++;
            _treesPerRound = Random.Range(2, 5);
            _roundText.text = "Round " + _roundCount;
        }
        GenerateTree();
    }
}
