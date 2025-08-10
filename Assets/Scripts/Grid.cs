using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Portfolio.Match3.Core
{
    /// <summary>
    /// Stores data about a candy's type, image, and color.
    /// </summary>
    [System.Serializable]
    public struct CandyProperties
    {
        public string CandyType;
        public string CandyResourceImage;
        public Color CandyColor;
    }

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
        [SerializeField] private CandyProperties[] _candyProperties;
        [SerializeField] private GameObject _candyPrefab;
        [SerializeField] private AudioSource _shuffleSound;

        private MatchChecker _matchChecker;

        private void Start()
        {
            _matchChecker = GetComponent<MatchChecker>();
            CreateGrid();
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
                    var clone = Instantiate(_nodePrefab);
                    clone.transform.position = worldPosition;

                    var candyClone = Instantiate(_candyPrefab);
                    candyClone.transform.position = worldPosition;


                    var nodeCurrent = new Node(candyClone.GetComponent<Candy>(), worldPosition, new Vector2Int(x, y));
                    _grid[x, y] = nodeCurrent;

                    candyClone.GetComponent<Candy>().Init(SelectCandyProperties(new Vector2Int(x, y)), nodeCurrent);
                }
            }
        }

        /// <summary>
        /// Selects a random candy that is allowed to spawn at the given position.
        /// Avoids creating matches by checking banned types.
        /// </summary>
        private CandyProperties SelectCandyProperties(Vector2Int pos)
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
        private bool CheckPossibleSpawnCandyType(string type, List<CandyProperties> typeList)
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
        private List<CandyProperties> GetBannedCandyList(Vector2Int nodePos)
        {
            var bannedList = new List<CandyProperties>();

            if (nodePos.x >= 2)
            {
                var left1 = _grid[nodePos.x - 1, nodePos.y].Candy.CandyProperties.CandyType;
                var left2 = _grid[nodePos.x - 2, nodePos.y].Candy.CandyProperties.CandyType;

                if (left1 == left2)
                    bannedList.Add(_grid[nodePos.x - 1, nodePos.y].Candy.CandyProperties);
            }

            if (nodePos.y >= 2)
            {
                var down1 = _grid[nodePos.x, nodePos.y - 1].Candy.CandyProperties.CandyType;
                var down2 = _grid[nodePos.x, nodePos.y - 2].Candy.CandyProperties.CandyType;

                if (down1 == down2)
                    bannedList.Add(_grid[nodePos.x, nodePos.y - 1].Candy.CandyProperties);
            }

            return bannedList;
        }

        /// <summary>
        /// Picks a random candy property from the available list.
        /// </summary>
        private CandyProperties GetRandomCandyProperties() =>
            _candyProperties[Random.Range(0, _candyProperties.Length)];

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
            var directions = state == 0
                ? new[] { Vector2Int.right, Vector2Int.left }
                : new[] { Vector2Int.up, Vector2Int.down };
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
        /// Checks if a position is inside the grid boundaries.
        /// </summary>
        public bool IsNodeInBounds(Vector2Int pos) => pos.x >= 0 && pos.x < _gridSize.x && pos.y >= 0 && pos.y < _gridSize.y;

        /// <summary>
        /// Fills empty nodes in given columns by moving candies down and spawning new ones.
        /// </summary>
        public void FillEmptyNodes(HashSet<int> xPoses)
        {
            var p = xPoses.ToList();
            p.Sort();
            
            DOVirtual.DelayedCall(.2f, () =>
            {
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


                DOVirtual.DelayedCall(0.3f, () =>
                {
                    foreach (var x in p.ToArray())
                    {
                        for (var y = 0; y < _gridSize.y; y++)
                        {
                            var node = _grid[x, y];
                            if (node.IsEmpty())
                                FillInstantiateCandy(node);
                        }
                    }

                    DOVirtual.DelayedCall(1f, () => { _matchChecker.DefaultMatchCheckStart(); });
                });
            });
        }

        /// <summary>
        /// Creates a new candy at the given node with a valid random type.
        /// </summary>
        private void FillInstantiateCandy(Node _node)
        {
            var _candyClone = Instantiate(_candyPrefab).GetComponent<Candy>();
            _candyClone.transform.name = "INSTANTIATE";
            _candyClone.transform.position = new Vector3(_node.WorldPosition.x, 8);
            _candyClone.Init(SelectCandyProperties(new Vector2Int(_node.GridPosition.x, _node.GridPosition.y)), _node);
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