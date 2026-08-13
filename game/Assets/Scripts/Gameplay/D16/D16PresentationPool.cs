using System.Collections.Generic;
using UnityEngine;

namespace Whalebone.Gameplay.D16
{
    /// <summary>只复用表现 prefab，不复用或隐藏领域实体。</summary>
    public sealed class D16PresentationPool : MonoBehaviour
    {
        private readonly Dictionary<int, Queue<GameObject>> _available = new Dictionary<int, Queue<GameObject>>();
        private readonly Dictionary<int, int> _sourceByInstance = new Dictionary<int, int>();
        private readonly HashSet<int> _pooledInstances = new HashSet<int>();
        private Transform _poolRoot;

        public int RentCount { get; private set; }
        public int ReuseCount { get; private set; }
        public int ReturnCount { get; private set; }
        public int PooledCount => _pooledInstances.Count;

        private void Awake()
        {
            var root = new GameObject("D16_PresentationPool_Root");
            root.transform.SetParent(transform, false);
            _poolRoot = root.transform;
        }

        public GameObject Rent(GameObject prefab, Transform parent, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;
            int sourceId = prefab.GetInstanceID();
            Queue<GameObject> queue;
            if (!_available.TryGetValue(sourceId, out queue))
            {
                queue = new Queue<GameObject>();
                _available[sourceId] = queue;
            }

            GameObject instance = null;
            while (queue.Count > 0 && instance == null)
            {
                instance = queue.Dequeue();
                if (instance != null)
                {
                    _pooledInstances.Remove(instance.GetInstanceID());
                    ReuseCount++;
                }
            }
            if (instance == null)
            {
                instance = Instantiate(prefab);
            }
            _sourceByInstance[instance.GetInstanceID()] = sourceId;
            instance.transform.SetParent(parent, false);
            instance.transform.SetPositionAndRotation(position, rotation);
            foreach (var registration in instance.GetComponentsInChildren<D16PresentationRegistration>(true))
                if (registration != null)
                    registration.ResetPresentationState();
            instance.SetActive(true);
            RentCount++;
            return instance;
        }

        public bool Return(GameObject instance)
        {
            if (instance == null || _poolRoot == null) return false;
            int sourceId;
            if (!_sourceByInstance.TryGetValue(instance.GetInstanceID(), out sourceId)) return false;
            if (!_pooledInstances.Add(instance.GetInstanceID())) return false;
            Queue<GameObject> queue;
            if (!_available.TryGetValue(sourceId, out queue))
            {
                queue = new Queue<GameObject>();
                _available[sourceId] = queue;
            }
            instance.SetActive(false);
            instance.transform.SetParent(_poolRoot, false);
            queue.Enqueue(instance);
            ReturnCount++;
            return true;
        }

        private void OnDestroy()
        {
            foreach (var queue in _available.Values)
                while (queue.Count > 0)
                {
                    var instance = queue.Dequeue();
                    if (instance != null) Destroy(instance);
                }
            _available.Clear();
            _sourceByInstance.Clear();
            _pooledInstances.Clear();
        }
    }
}
