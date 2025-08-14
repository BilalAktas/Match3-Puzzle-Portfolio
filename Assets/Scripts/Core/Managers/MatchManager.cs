using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Portfolio.Match3.Core
{
    /// <summary>
    /// Manages matching candies in the grid and handles match effects.
    /// </summary>
    public class MatchManager : MonoBehaviour
    {
        private HashSet<Node> nodes = new();
        private List<Node> matchedNodes = new();
        private List<Node> matchedNodesSpecials = new();
        private HashSet<int> emptyNodePosesAfterMatch = new();

        [SerializeField] private GameObject _explosionParticlePrefab;
        [SerializeField] private AudioSource _matchSound;
        [SerializeField] private AudioSource _specialMatchSound;

        private Queue<List<Node>> matchedNodesQueue = new();
        private Coroutine ProcessMatchedNodesCO;

        public static event Action<HashSet<int>> OnMatched;
        public static Action<HashSet<Node>> OnForceMatched;

        private void Start()
        {
            OnForceMatched += ForceMatchedNodes;
        }

        private void OnDestroy()
        {
            OnForceMatched -= ForceMatchedNodes;
        }

        /// <summary>
        /// Starts checking matches from the given candy node.
        /// </summary>
        /// <param name="selectable">The candy to start matching from.</param>
        /// <param name="doAction">If true, performs matching actions like score and effects.</param>
        /// <returns>Returns true if matches found, otherwise false.</returns>
        public bool StartCheck(Candy selectable, bool doAction)
        {
            if (selectable?.CurrentNode == null)
                return false;

            nodes.Clear();
            matchedNodes.Clear();
            matchedNodesSpecials.Clear();

            nodes.Add(selectable.CurrentNode);
            matchedNodes.Add(selectable.CurrentNode);

            return CheckAllNeighbors(doAction);
        }

        /// <summary>
        /// Checks neighbors horizontally and vertically for matching candies.
        /// </summary>
        /// <param name="doAction">If true, applies match effects and score.</param>
        /// <returns>Returns true if a match of 3 or more is found.</returns>
        private bool CheckAllNeighbors(bool doAction)
        {
            var startNode = nodes.FirstOrDefault();
            if (startNode == null)
                return false;

            // state 0 = horizontal, state 1 = vertical
            for (var state = 0; state < 2; state++)
            {
                nodes.Clear();
                matchedNodes.Clear();
                matchedNodesSpecials.Clear();

                nodes.Add(startNode);
                matchedNodes.Add(startNode);

                while (nodes.Count > 0)
                {
                    var current = nodes.First();
                    var neighbors = Grid.Instance.GetAllNeighbors(current, state);

                    foreach (var neighbor in neighbors)
                    {
                        if (neighbor.Candy == null || current.Candy == null)
                            continue;

                        var neighborType = neighbor.Candy.CandyData.CandyType;
                        var currentType = current.Candy.CandyData.CandyType;

                        if (!matchedNodes.Contains(neighbor))
                        {
                            if (neighborType == currentType && neighborType != "Bomb" && currentType != "Bomb")
                            {
                                nodes.Add(neighbor);
                                matchedNodes.Add(neighbor);
                            }

                            if (neighborType == "Bomb")
                                matchedNodesSpecials.Add(neighbor);
                        }
                    }

                    nodes.Remove(current);
                }

                if (matchedNodes.Count >= 3)
                {
                    foreach (var node in matchedNodesSpecials)
                        matchedNodes.Add(node);

                    matchedNodesQueue.Enqueue(matchedNodes);

                    return CheckMatchedNodes(doAction);
                }
            }

            matchedNodes.Clear();
            matchedNodesSpecials.Clear();
            return false;
        }

        /// <summary>
        /// Handles matched nodes and special nodes: plays effects, updates score, and signals matches.
        /// </summary>
        /// <param name="doAction">If true, executes scoring and visual effects.</param>
        /// <returns>Returns true if matches processed, otherwise false.</returns>
        private bool CheckMatchedNodes(bool doAction)
        {
            if (matchedNodes.Count < 3)
                return false;

            if (doAction)
                StartProcessMatchedNodes();

            matchedNodes.Clear();
            matchedNodesSpecials.Clear();
            return true;
        }

        private void StartProcessMatchedNodes()
        {
            if (ProcessMatchedNodesCO != null)
                StopCoroutine(ProcessMatchedNodesCO);

            ProcessMatchedNodesCO = StartCoroutine(ProcessMatchedNodes());
        }

        /// <summary>
        /// Processes given matched nodes and special nodes: score, effects, and event invoke.
        /// </summary>
        /// <param name="normalNodes">Normal matched nodes collection.</param>
        /// <param name="specialNodes">Special matched nodes collection (e.g., bombs).</param>
        private IEnumerator ProcessMatchedNodes()
        {
            while (matchedNodesQueue.Count > 0)
            {
                var nodes = matchedNodesQueue.Dequeue();
                foreach (var node in nodes)
                {
                    if (node.Candy != null)
                    {
                        GameManager.OnScored?.Invoke(Random.Range(5, 15));
                        PlayMatchEffect(node);
                        emptyNodePosesAfterMatch.Add(node.GridPosition.x);
                    }
                }


                yield return new WaitForSeconds(.05f);
            }

            OnMatched?.Invoke(emptyNodePosesAfterMatch);
            emptyNodePosesAfterMatch.Clear();
        }

        /// <summary>
        /// Plays matching effect and sound on the given node.
        /// </summary>
        /// <param name="node">Node to play effects on.</param>
        private void PlayMatchEffect(Node node)
        {
            var color = node.Candy.CandyData.CandyColor;
            if (node.Candy.CandyData.CandyType == "Bomb")
                _specialMatchSound.Play();

            node.Candy.Matched();

            _matchSound.Play();

            var particleClone = ObjectPool.Instance.GetFromPool("ExplosionParticle");
            particleClone.transform.position = node.WorldPosition;
            particleClone.SetActive(true);

            foreach (var ps in particleClone.GetComponentsInChildren<ParticleSystem>())
            {
                var main = ps.main;
                main.startColor = color;
                ps.Play();
            }
        }

        /// <summary>
        /// Forces external matched nodes to process their effects and scoring.
        /// </summary>
        /// <param name="_nodes">Nodes to force match.</param>
        private void ForceMatchedNodes(HashSet<Node> _nodes)
        {
            if (_nodes == null || _nodes.Count == 0)
                return;

            matchedNodesQueue.Enqueue(_nodes.ToList());

            StartProcessMatchedNodes();
        }
    }
}