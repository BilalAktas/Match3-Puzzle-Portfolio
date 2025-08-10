using System.Collections.Generic;
using UnityEngine;

namespace Portfolio.Match3.Core
{
    /// <summary>
    /// Represents a poolable object with settings for spawning and expanding the pool.
    /// </summary>
    [System.Serializable]
    public class PooledObject
    {
        public string Name;
        public GameObject Prefab;
        public int SpawnAmount;
        public int ExpandAmount;
        public Queue<GameObject> ObjectPooled = new();
    }
    
    /// <summary>
    /// A generic object pooling system for reusing GameObjects efficiently.
    /// </summary>
    public class ObjectPool : Singleton<ObjectPool>
    {
        [SerializeField] private PooledObject[] _pooledObjects;
        
        private void Awake()
        {
            Init();
        }
        
        /// <summary>
        /// Instantiates and queues the initial objects for each pool.
        /// </summary>
        private void Init()
        {
            foreach (var pooledObject in _pooledObjects)
            {
                for (var i = 0; i < pooledObject.SpawnAmount; i++)
                {
                    var clone = Instantiate(pooledObject.Prefab, transform);
                    clone.SetActive(false);
                    pooledObject.ObjectPooled.Enqueue(clone);
                }
            }
        }
        
        /// <summary>
        /// Retrieves an object from the specified pool by name. 
        /// If no objects are available, the pool will expand.
        /// </summary>
        /// <param name="name">The name of the pool to pull from.</param>
        /// <returns>An inactive GameObject ready for use, or null if the pool is not found.</returns>
        public GameObject GetFromPool(string name)
        {
            foreach (var pooledObject in _pooledObjects)
            {   
                if(pooledObject.Name != name)
                    continue;

                if (pooledObject.ObjectPooled.Count > 0)
                    return pooledObject.ObjectPooled.Dequeue();
                
      
                for (var i = 0; i < pooledObject.ExpandAmount; i++)
                {
                    var clone = Instantiate(pooledObject.Prefab, transform);
                    clone.SetActive(false);
                    pooledObject.ObjectPooled.Enqueue(clone);
                }
                
                return  pooledObject.ObjectPooled.Dequeue();
            }

            return null;
        }
        
        /// <summary>
        /// Returns a GameObject to its respective pool for future reuse.
        /// </summary>
        /// <param name="_object">The object to return to the pool.</param>
        /// <param name="poolName">The name of the pool to return the object to.</param>
        public void Deposit(GameObject _object, string poolName)
        {
            foreach (var pooledObject in _pooledObjects)
            {
                if(pooledObject.Name != poolName)
                    continue;
                
                _object.SetActive(false);
                _object.transform.SetParent(transform);
                pooledObject.ObjectPooled.Enqueue(_object);
            }
        }
    }   
}
