using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Portfolio.Match3.Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Portfolio.Match3.Core
{
    /// <summary>
    /// Manages the game grid and its related operations.
    /// </summary>
    public class Grid : Singleton<Grid>
    {
        private Node[,] _grid;
        public Node[,] GetGrid => _grid;
        [SerializeField] private Vector2Int _gridSize;
        [SerializeField] private Vector2Int _nodeSize;
        [SerializeField] private GameObject _nodePrefab;
        [SerializeField] private CandyConfig[] _candyConfigs;
        [SerializeField] private GameObject _candyPrefab;
        [SerializeField] private AudioSource _shuffleSound;

        private MatchChecker _matchChecker;

        private void Start()
        {
            _matchChecker = GetComponent<MatchChecker>();
            CreateGrid();

            MatchManager.OnMatched += FillEmptyNodes;
        }

        private void OnDestroy()
        {
            MatchManager.OnMatched -= FillEmptyNodes;
        }

        /// <summary>
        /// Makes the game grid and fills it with nodes and random candies.
        /// </summary>
        private void CreateGrid()
        {
            _grid = new Node[_gridSize.x, _gridSize.y];

            var origin = new Vector2Int(0, 0);
            var totalGridSize = new Vector2(_gridSize.x * _nodeSize.x, _gridSize.y * _nodeSize.y);

            var bottomLeft = origin - new Vector2(totalGridSize.x, totalGridSize.y) / 2f +
                             new Vector2(_nodeSize.x, _nodeSize.y) / 2f;

            for (var x = 0; x < _gridSize.x; x++)
            {
                for (var y = 0; y < _gridSize.y; y++)
                {
                    var worldPosition = bottomLeft + new Vector2(x * _nodeSize.x, y * _nodeSize.y);
                    var clone = ObjectPool.Instance.GetFromPool("Node");
                    clone.SetActive(true);
                    clone.transform.position = worldPosition;

                    var selectedCandy = SelectCandyProperties(new Vector2Int(x, y));

                    var candyClone = ObjectPool.Instance.GetFromPool(selectedCandy.PoolName);
                    candyClone.SetActive(true);
                    candyClone.transform.position = worldPosition;


                    var nodeCurrent = new Node(candyClone.GetComponent<Candy>(), worldPosition, new Vector2Int(x, y));
                    _grid[x, y] = nodeCurrent;

                    candyClone.GetComponent<Candy>().Init(selectedCandy, nodeCurrent);
                }
            }

            DOVirtual.DelayedCall(1f, () => { _matchChecker.DefaultMatchCheckStart(); });
        }

        /// <summary>
        /// Selects a random candy that is allowed to spawn at the given position.
        /// Avoids creating matches by checking banned types.
        /// </summary>
        private CandyData SelectCandyProperties(Vector2Int pos)
        {
            var cSelectedCandy = GetRandomCandyProperties();
            while (CheckPossibleSpawnCandyType(cSelectedCandy.CandyType,
                       GetBannedCandyList(pos)))
                cSelectedCandy = GetRandomCandyProperties();

            return cSelectedCandy;
        }

        /// <summary>
        /// Checks if the given candy type is in the banned list.
        /// </summary>
        private bool CheckPossibleSpawnCandyType(string type, List<CandyData> typeList)
        {
            var s = false;
            foreach (var tL in typeList)
            {
                if (tL.CandyType == type)
                {
                    s = true;
                    break;
                }
            }

            return s;
        }

        /// <summary>
        /// Gets a list of candy types not allowed to spawn at the given position.
        /// </summary>
        private List<CandyData> GetBannedCandyList(Vector2Int nodePos)
        {
            var bannedList = new List<CandyData>();

            if (nodePos.x >= 2)
            {
                if (!_grid[nodePos.x - 1, nodePos.y].IsEmpty() && !_grid[nodePos.x - 2, nodePos.y].IsEmpty())
                {
                    var left1 = _grid[nodePos.x - 1, nodePos.y].Candy.CandyData.CandyType;
                    var left2 = _grid[nodePos.x - 2, nodePos.y].Candy.CandyData.CandyType;

                    if (left1 == left2)
                        bannedList.Add(_grid[nodePos.x - 1, nodePos.y].Candy.CandyData);
                }
            }

            if (nodePos.y >= 2)
            {
                if (!_grid[nodePos.x, nodePos.y - 1].IsEmpty() && !_grid[nodePos.x, nodePos.y - 2].IsEmpty())
                {
                    var down1 = _grid[nodePos.x, nodePos.y - 1].Candy.CandyData.CandyType;
                    var down2 = _grid[nodePos.x, nodePos.y - 2].Candy.CandyData.CandyType;

                    if (down1 == down2)
                        bannedList.Add(_grid[nodePos.x, nodePos.y - 1].Candy.CandyData);
                }
            }

            return bannedList;
        }

        /// <summary>
        /// Picks a random candy property from the available list.
        /// </summary>
        private CandyData GetRandomCandyProperties()
        {
            var p = 0f;
            var r = Random.Range(0, 100);
            foreach (var config in _candyConfigs)
            {
                p += config.CandyData.Percent;
                if (r <= p)
                {
                    return config.CandyData;
                }
            }

            return _candyConfigs[Random.Range(0, _candyConfigs.Length)].CandyData;
        }


        /// <summary>
        /// Checks if two nodes are next to each other on the grid.
        /// </summary>
        public static bool AreNodesNeighbor(Node node1, Node node2)
        {
            var differenceX = Mathf.Abs(node1.GridPosition.x - node2.GridPosition.x);
            var differenceY = Mathf.Abs(node1.GridPosition.y - node2.GridPosition.y);

            return (differenceX == 1 && differenceY == 0) || (differenceX == 0 && differenceY == 1);
        }

        /// <summary>
        /// Gets neighbor nodes in horizontal or vertical directions based on state.
        /// </summary>
        public HashSet<Node> GetAllNeighbors(Node node, int state)
        {
            var _nodes = new HashSet<Node>();
            var directions = new List<Vector2Int>();

            switch (state)
            {
                case 0:
                    directions = new List<Vector2Int>()
                    {
                        Vector2Int.right, Vector2Int.left
                    };
                    break;

                case 1:
                    directions = new List<Vector2Int>()
                    {
                        Vector2Int.up, Vector2Int.down
                    };
                    break;

                case 2:
                    directions = new List<Vector2Int>()
                    {
                        new Vector2Int(1, 0),
                        new Vector2Int(-1, 0),
                        new Vector2Int(0, 1),
                        new Vector2Int(0, -1),
                        new Vector2Int(1, 1),
                        new Vector2Int(-1, 1),
                        new Vector2Int(1, -1),
                        new Vector2Int(-1, -1),
                    };
                    break;
            }

            foreach (var direction in directions)
            {
                var _gridPos = node.GridPosition + direction;

                if (IsNodeInBounds(_gridPos))
                {
                    var _node = _grid[_gridPos.x, _gridPos.y];

                    if (!_node.IsEmpty())
                        _nodes.Add(_node);
                }
            }

            return _nodes;
        }

        /// <summary>
        /// Gets all non-empty nodes in the same row as the given node.
        /// </summary>
        /// <param name="node">The reference node.</param>
        /// <returns>A hash set of nodes in the same horizontal line.</returns>
        public HashSet<Node> GetAllHorizontalNodesFromNode(Node node)
        {
            var _nodes = new HashSet<Node>();

            for (var x = 0; x < _gridSize.y; x++)
            {
                var _checkNode = _grid[x, node.GridPosition.y];
                if (!_checkNode.IsEmpty())
                {
                    _nodes.Add(_checkNode);
                }
            }

            return _nodes;
        }


        /// <summary>
        /// Gets all non-empty nodes in the same column as the given node.
        /// </summary>
        /// <param name="node">The reference node.</param>
        /// <returns>A hash set of nodes in the same vertical line.</returns>
        public HashSet<Node> GetAllVerticalNodesFromNode(Node node)
        {
            var _nodes = new HashSet<Node>();

            for (var y = 0; y < _gridSize.y; y++)
            {
                var _checkNode = _grid[node.GridPosition.x, y];
                if (!_checkNode.IsEmpty())
                {
                    _nodes.Add(_checkNode);
                }
            }

            return _nodes;
        }

        /// <summary>
        /// Returns the node at the given grid position.
        /// </summary>
        public Node GetNodeAt(Vector2Int pos) => _grid[pos.x, pos.y];


        /// <summary>
        /// Checks if a position is inside the grid boundaries.
        /// </summary>
        private bool IsNodeInBounds(Vector2Int pos) =>
            pos.x >= 0 && pos.x < _gridSize.x && pos.y >= 0 && pos.y < _gridSize.y;

        /// <summary>
        /// Fills empty nodes in given columns by moving candies down and spawning new ones.
        /// </summary>
        private void FillEmptyNodes(HashSet<int> xPoses)
        {
            try
            {
                StartCoroutine(FillEmptyNodesCO(xPoses.ToList()));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Coroutine that fills empty nodes by dropping candies down and spawning new ones.
        /// </summary>
        /// <param name="xPoses">List of X positions (columns) to fill.</param>
        /// <returns>IEnumerator for coroutine execution.</returns>
        private IEnumerator FillEmptyNodesCO(List<int> xPoses)
        {
            xPoses.Sort();
            yield return new WaitForSeconds(.05f);

            foreach (var x in xPoses)
            {
                bool needsAnotherPass;

                do
                {
                    needsAnotherPass = false;

                    for (var y = 0; y < _gridSize.y - 1; y++)
                    {
                        var currentNode = _grid[x, y];
                        if (currentNode.IsEmpty())
                        {
                            for (var j = y + 1; j < _gridSize.y; j++)
                            {
                                var upperNode = _grid[x, j];
                                if (!upperNode.IsEmpty())
                                {
                                    currentNode.SetCandy(upperNode.Candy, true);
                                    upperNode.SetCandy(null, false);

                                    needsAnotherPass = true;
                                    break;
                                }
                            }
                        }
                    }
                } while (needsAnotherPass);
            }

            yield return new WaitForSeconds(.2f);
            foreach (var x in xPoses.ToArray())
            {
                for (var y = 0; y < _gridSize.y; y++)
                {
                    var node = _grid[x, y];
                    if (node.IsEmpty())
                        FillInstantiateCandy(node);
                }
            }

            yield return new WaitForSeconds(.5f);
            _matchChecker.DefaultMatchCheckStart();
        }

        /// <summary>
        /// Creates a new candy at the specified node with a randomly selected type.
        /// </summary>
        /// <param name="_node">The node to fill with a new candy.</param>
        private void FillInstantiateCandy(Node _node)
        {
            var selected = SelectCandyProperties(new Vector2Int(_node.GridPosition.x, _node.GridPosition.y));
            var _candyClone = ObjectPool.Instance.GetFromPool(selected.PoolName).GetComponent<Candy>();
            _candyClone.gameObject.SetActive(true);
            _candyClone.transform.position = new Vector3(_node.WorldPosition.x, 8);
            _candyClone.Init(selected, _node);
            _node.SetCandy(_candyClone, true);
        }

        /// <summary>
        /// Mixes all candies on the grid randomly and starts match checking.
        /// </summary>
        public void ShuffleGrid()
        {
            var allCandies = new List<Candy>();

            _shuffleSound.Play();

            foreach (var node in _grid)
            {
                if (!node.IsEmpty())
                    allCandies.Add(node.Candy);
            }

            Helper.ShuffleList(allCandies);

            var i = 0;
            foreach (var node in _grid)
            {
                if (!node.IsEmpty())
                {
                    var candy = allCandies[i];
                    node.SetCandy(candy, true);
                    candy.CurrentNode = node;
                    i++;
                }
            }

            DOVirtual.DelayedCall(1f, () => { _matchChecker.DefaultMatchCheckStart(); });
        }
    }
}