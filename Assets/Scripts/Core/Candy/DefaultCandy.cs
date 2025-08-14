namespace Portfolio.Match3.Core
{
    /// <summary>
    ///  Represents a basic candy.
    /// </summary>
    public class DefaultCandy : Candy
    {
        /// <summary>
        /// Handles the logic when the default candy is matched.
        /// Clears the candy from its node and returns it to the object pool.
        /// </summary>
        public override void Matched()
        {
            if (_matched)
                return;

            _matched = true;

            CurrentNode.SetCandy(null, false);
            ObjectPool.Instance.Deposit(gameObject, "Candy");
        }
    }
}