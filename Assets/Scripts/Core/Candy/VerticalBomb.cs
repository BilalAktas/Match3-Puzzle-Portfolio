namespace Portfolio.Match3.Core
{
    /// <summary>
    /// Represents a special candy.
    /// </summary>
    public class VerticalBomb : Candy
    {
        /// <summary>
        /// Handles the logic when the vertical bomb is matched.
        /// Clears all candies in the same vertical column, 
        /// removes itself from the grid, and returns to the object pool.
        /// </summary>
        public override void Matched()
        {
            if (_matched)
                return;

            _matched = true;

            var neighbors = Grid.Instance.GetAllVerticalNodesFromNode(CurrentNode);

            CurrentNode.SetCandy(null, false);
            ObjectPool.Instance.Deposit(gameObject, "VerticalBomb");

            MatchManager.OnForceMatched?.Invoke(neighbors);
        }
    }
}