using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Portfolio.Match3.Core
{
    /// <summary>
    /// Handles candy selection logic in the game.
    /// </summary>
    public class CandySelecter : MonoBehaviour
    {
        private Camera _cam;
        private List<Candy> _selectedSelectables = new();
        [SerializeField] private AudioSource _moveSound;
        [SerializeField] private AudioSource _moveFailSound;
        private MatchManager _matchManager;

        private void Start()
        {
            _cam = Camera.main;
            _matchManager = GetComponent<MatchManager>();
        }

        private void Update()
        {
            if (GameManager.CurrentGameState == GameState.Idle)
                return;
            
            
            GetPlayerInput();
        }

        /// <summary>
        /// Gets player touch input and handles the start of a touch.
        /// </summary>
        private void GetPlayerInput()
        {
            // if (Input.touchCount <= 0)
            //     return;
            //
            // var _touch = Input.GetTouch(0);
            // switch (_touch.phase)
            // {
            //     case TouchPhase.Began: HandleTouchBegan(_touch.position); break;
            // }

            if (Input.GetMouseButtonDown(0))
            {
                HandleTouchBegan(Input.mousePosition);
            }
        }

        /// <summary>
        /// Handles the start of a touch by selecting a candy if possible.
        /// </summary>
        private void HandleTouchBegan(Vector2 pos)
        {
            var _pos = _cam.ScreenToWorldPoint(pos);
            var _collider = Physics2D.OverlapPoint(_pos);
            if (_collider && _collider.TryGetComponent(out Candy selectable) &&
                !_selectedSelectables.Contains(selectable))
            {
                _selectedSelectables.Add(selectable);
                StartCoroutine(CheckCandySwitch());
            }
        }

        /// <summary>
        /// Checks if two selected candies can switch and handles the swap and match check.
        /// </summary>
        private IEnumerator CheckCandySwitch()
        {
            if (_selectedSelectables.Count < 2)
                yield break;

            var nodeA = _selectedSelectables[0].CurrentNode;
            var nodeB = _selectedSelectables[1].CurrentNode;

            var candyA = nodeA.Candy;
            var candyB = nodeB.Candy;

            _selectedSelectables.Clear();

            if (!Grid.AreNodesNeighbor(nodeA, nodeB))
                yield break;

            nodeA.SetCandy(candyB, false);
            nodeB.SetCandy(candyA, false);

            yield return new WaitForSeconds(0.3f);

            var matchA = _matchManager.StartCheck(candyA, true);
            var matchB = _matchManager.StartCheck(candyB, true);

            if (!matchA && !matchB)
            {
                nodeA.SetCandy(candyA, false);
                nodeB.SetCandy(candyB, false);

                candyA.FailMatchAnim();
                candyB.FailMatchAnim();

                _moveFailSound.Play();
            }
            else
            {
                _moveSound.Play();
            }
        }
    }
}