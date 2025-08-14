namespace Portfolio.Match3.Core
{
    /// <summary>
    /// Represents a special candy.
    /// </summary>
    public class HorizontalBomb : Candy
    {
        /// <summary>
        /// Triggers the horizontal bomb effect when matched.
        /// Clears all candies in the same row and returns this bomb to the object pool.
        /// </summary>
        public override void Matched()
        {
            if (_matched)
                return;

            _matched = true;

            var neighbors = Grid.Instance.GetAllHorizontalNodesFromNode(CurrentNode);

            CurrentNode.SetCandy(null, false);
            ObjectPool.Instance.Deposit(gameObject, "HorizontalBomb");

            MatchManager.OnForceMatched?.Invoke(neighbors);
        }
    }
}