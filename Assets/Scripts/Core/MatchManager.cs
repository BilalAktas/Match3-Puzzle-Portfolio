using System;
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
        private HashSet<int> emptyNodePosesAfterMatch = new HashSet<int>();
        [SerializeField] private GameObject _explosionParticlePrefab;
        [SerializeField] private AudioSource _matchSound;

        public static event Action<HashSet<int>> OnMatched;

        /// <summary>
        /// Starts checking matches from the given candy.
        /// </summary>
        public bool StartCheck(Candy selectable, bool doAction)
        {
            if (selectable && selectable.CurrentNode != null)
            {
                nodes.Add(selectable.CurrentNode);
                matchedNodes.Add(selectable.CurrentNode);

                return CheckAllNeighbors(doAction);
            }

            return false;
        }

        /// <summary>
        /// Checks all neighbors horizontally and vertically for matches.
        /// </summary>
        private bool CheckAllNeighbors(bool doAction)
        {
            var startNode = nodes.FirstOrDefault();

            for (var state = 0; state < 2; state++)
            {
                matchedNodes.Clear();
                nodes.Clear();

                nodes.Add(startNode);
                matchedNodes.Add(startNode);

                while (nodes.Count > 0)
                {
                    var current = nodes.First();

                    var neighbors = Grid.Instance.GetAllNeighbors(current, state);

                    foreach (var neighbor in neighbors)
                    {
                        if (!matchedNodes.Contains(neighbor) &&
                            neighbor.Candy.CandyProperties.CandyType == current.Candy.CandyProperties.CandyType)
                        {
                            matchedNodes.Add(neighbor);
                            nodes.Add(neighbor);
                        }
                    }

                    nodes.Remove(current);
                }

                if (matchedNodes.Count >= 3)
                    return CheckMatchedNodes(doAction);
            }

            matchedNodes.Clear();
            return false;
        }

        /// <summary>
        /// Handles matched nodes, plays effects, updates score, and fills empty spaces.
        /// </summary>
        private bool CheckMatchedNodes(bool doAction)
        {
            if (matchedNodes.Count >= 3)
            {
                var point = matchedNodes.Count * Random.Range(5, 15);
                if (doAction) GameManager.OnScored?.Invoke(point);
                foreach (var node in matchedNodes)
                {
                    if (doAction)
                    {
                        var color = node.Candy.CandyProperties.CandyColor;
                        node.Candy.Matched();

                        _matchSound.Play();

                        var particleClone = ObjectPool.Instance.GetFromPool("ExplosionParticle");
                        particleClone.transform.position = node.WorldPosition;
                        particleClone.SetActive(true);
                        foreach (var particleSystem in particleClone.GetComponentsInChildren<ParticleSystem>())
                        {
                            var main = particleSystem.main;
                            main.startColor = color;
                            particleSystem.Play();
                        }
                    }

                    emptyNodePosesAfterMatch.Add(node.GridPosition.x);
                }

                if (doAction) OnMatched?.Invoke(emptyNodePosesAfterMatch);

                //emptyNodePosesAfterMatch.Clear();
                matchedNodes.Clear();
                return true;
            }

            //emptyNodePosesAfterMatch.Clear();
            matchedNodes.Clear();
            return false;
        }
    }
}