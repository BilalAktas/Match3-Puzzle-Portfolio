namespace Portfolio.Match3.Core
{
    /// <summary>
    /// Represents a special candy.
    /// </summary>
    public class Bomb : Candy
    {
        /// <summary>
        /// Handles the logic when the bomb candy is matched.
        /// Clears itself and all adjacent candies in every direction,
        /// then returns the bomb to the object pool.
        /// </summary>
        public override void Matched()
        {
            if (_matched)
                return;

            _matched = true;

            var neighbors = Grid.Instance.GetAllNeighbors(CurrentNode, 2);

            CurrentNode.SetCandy(null, false);
            ObjectPool.Instance.Deposit(gameObject, "Bomb");

            MatchManager.OnForceMatched?.Invoke(neighbors);
        }
    }
}