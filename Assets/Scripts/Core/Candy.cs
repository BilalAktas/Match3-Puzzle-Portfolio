using DG.Tweening;
using UnityEngine;

namespace Portfolio.Match3.Core
{
    /// <summary>
    /// Represents a candy in the game with its properties and behaviors.
    /// </summary>
    public class Candy : MonoBehaviour
    {
        private const float _MOVE_TIME = .2f;

        public CandyProperties CandyProperties;
        public Node CurrentNode;

        private BoxCollider2D _collider;

        private Animator _anim;
        private static readonly int _FALL = Animator.StringToHash("Fall");
        private static readonly int _FAIL = Animator.StringToHash("Fail");

        private void Start()
        {
            _anim = GetComponent<Animator>();
            _collider = GetComponent<BoxCollider2D>();
        }

        /// <summary>
        /// Sets candy properties and assigns it to a node.
        /// </summary>
        public void Init(CandyProperties properties, Node node)
        {
            this.CandyProperties = properties;
            GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"Tiles/{properties.CandyResourceImage}");
            CurrentNode = node;
        }

        /// <summary>
        /// Moves the candy to a position and updates its node; triggers fall animation if needed.
        /// </summary>
        public void Move(Vector2 pos, Node currentNode, bool fall)
        {
            //_collider.enabled = false;
            transform.DOMove(pos, _MOVE_TIME).OnComplete(() =>
            {
                if (fall)
                    _anim.SetTrigger(_FALL);

                //_collider.enabled = true;
            });
            CurrentNode = currentNode;
        }

        /// <summary>
        /// Plays animation for a failed match.
        /// </summary>
        public void FailMatchAnim() => _anim.SetTrigger(_FAIL);

        /// <summary>
        /// Returns the candy's current position.
        /// </summary>
        public Vector2 GetPosition() => transform.position;

        /// <summary>
        /// Handles candy being matched and deactivates it.
        /// </summary>
        public void Matched()
        {
            CurrentNode.SetCandy(null, false);
            //gameObject.SetActive(false);
            ObjectPool.Instance.Deposit(gameObject, "Candy");
        }
    }
}