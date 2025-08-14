using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Portfolio.Match3.Core
{
    /// <summary>
    /// Represents a candy in the game with its properties and behaviors.
    /// </summary>
    public abstract class Candy : MonoBehaviour
    {
        private const float _MOVE_TIME = .2f;

        public CandyData CandyData;
        public Node CurrentNode;
        private Animator _anim;
        private static readonly int _FALL = Animator.StringToHash("Fall");
        private static readonly int _FAIL = Animator.StringToHash("Fail");

        protected bool _matched;
        
        private void Start()
        {
            _anim = GetComponent<Animator>();
        }

        /// <summary>
        /// Sets candy properties and assigns it to a node.
        /// </summary>
        public void Init(CandyData data, Node node)
        {
            CandyData = data;
            GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"Tiles/{data.CandyResourceImage}");
            CurrentNode = node;

            _matched = false;
        }

        /// <summary>
        /// Moves the candy to a position and updates its node; triggers fall animation if needed.
        /// </summary>
        public void Move(Vector2 pos, Node currentNode, bool fall)
        {
            transform.DOMove(pos, _MOVE_TIME).OnComplete(() =>
            {
                if (fall)
                    _anim.SetTrigger(_FALL);
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
        public abstract void Matched();
        
    }
}