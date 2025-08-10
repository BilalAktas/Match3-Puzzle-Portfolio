using UnityEngine;

namespace Portfolio.Match3.Core
{
    /// <summary>
    /// Represents a grid node holding a candy and its position info.
    /// </summary>
    public class Node
    {
        public Candy Candy;
        public Vector2 WorldPosition;
        public Vector2Int GridPosition;

        public Node(Candy candy, Vector2 worldPosition, Vector2Int gridPosition)
        {
            Candy = candy;
            WorldPosition = worldPosition;
            GridPosition = gridPosition;
        }

        /// <summary>
        /// Sets the candy on this node and updates the candy's position.
        /// </summary>
        public void SetCandy(Candy candy, bool candyFall)
        {
            Candy = candy;

            if (Candy != null)
            {
                Candy.CurrentNode = this;
                Candy.Move(WorldPosition, this, candyFall);
            }
            else
            {
                if (Candy != null)
                    Candy.CurrentNode = null;
            }
        }

        /// <summary>
        /// Returns true if the node has no candy.
        /// </summary>
        public bool IsEmpty() => !Candy;
    }
}