using System.Collections;
using UnityEngine;

namespace Portfolio.Match3.Core
{
    /// <summary>
    /// Checks the grid for matches and valid moves, triggers reshuffle if none found.
    /// </summary>
    public class MatchChecker : MonoBehaviour
    {
        private Coroutine _defaultMatchCheckCO;
        private MatchManager _matchManager;

        private void Start()
        {
            _matchManager = GetComponent<MatchManager>();
        }

        /// <summary>
        /// Start checking matches automatically on the grid.
        /// </summary>
        public void DefaultMatchCheckStart()
        {
            if (_defaultMatchCheckCO != null)
                StopCoroutine(_defaultMatchCheckCO);

            _defaultMatchCheckCO = StartCoroutine(DefaultMatchCheck());
        }

        /// <summary>
        /// Coroutine to check each candy on the grid for matches.
        /// If no moves are possible, shuffle the grid.
        /// </summary>
        private IEnumerator DefaultMatchCheck()
        {
            var grid = Grid.Instance.GetGrid;
            foreach (var node in grid)
            {
                if (_matchManager.StartCheck(node.Candy, true))
                {
                    yield return new WaitForSeconds(1f);

                    DefaultMatchCheckStart();
                }
            }

            if (!HasValidMoves())
                Grid.Instance.ShuffleGrid();
        }

        /// <summary>
        /// Check if there are any valid moves left on the grid.
        /// Returns true if at least one valid move exists.
        /// </summary>
        public bool HasValidMoves()
        {
            var grid = Grid.Instance.GetGrid;
            var width = grid.GetLength(0);
            var height = grid.GetLength(1);

            Vector2Int[] directions = { Vector2Int.right, Vector2Int.down };

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var currentNode = grid[x, y];
                    var currentCandy = currentNode.Candy;

                    if (currentCandy == null)
                        continue;

                    foreach (var dir in directions)
                    {
                        var nx = x + dir.x;
                        var ny = y + dir.y;

                        if (nx >= width || ny >= height || nx < 0 || ny < 0)
                            continue;

                        var neighborNode = grid[nx, ny];
                        var neighborCandy = neighborNode.Candy;

                        if (neighborCandy == null)
                            continue;

                        currentNode.Candy = neighborCandy;
                        neighborCandy.CurrentNode = currentNode;

                        neighborNode.Candy = currentCandy;
                        currentCandy.CurrentNode = neighborNode;

                        var matchFound = _matchManager.StartCheck(currentCandy, false) ||
                                         _matchManager.StartCheck(neighborCandy, false);

                        // Undo swap
                        currentNode.Candy = currentCandy;
                        currentCandy.CurrentNode = currentNode;

                        neighborNode.Candy = neighborCandy;
                        neighborCandy.CurrentNode = neighborNode;

                        if (matchFound)
                            return true;
                    }
                }
            }

            return false;
        }
    }
}