using System.Collections.Generic;
using UnityEngine;

namespace Whalebone.Gameplay.D16
{
    /// <summary>
    /// 显式管理一组表现音频引用。只有通过本 scope 加载的 clip 才会被释放，
    /// 避免误卸载其他系统共享的 AudioClip。
    /// </summary>
    public sealed class D16AudioResourceScope : MonoBehaviour
    {
        [SerializeField] private bool _allowAssetUnload = false;
        [SerializeField] private string _ownedResourcePrefix = "D16Audio/";
        private readonly Dictionary<string, AudioClip> _loaded = new Dictionary<string, AudioClip>();
        private readonly HashSet<string> _unownedPaths = new HashSet<string>(System.StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _pathsByOwner =
            new Dictionary<string, HashSet<string>>(System.StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> _ownersByPath =
            new Dictionary<string, HashSet<string>>(System.StringComparer.Ordinal);
        private sealed class SharedReferenceState
        {
            public int Count;
            public int NonOwnedCount;
        }

        private static readonly Dictionary<int, SharedReferenceState> SharedReferences =
            new Dictionary<int, SharedReferenceState>();

        public int LoadedCount => _loaded.Count;
        public int SkippedSharedAssetReleaseCount { get; private set; }
        public int UnloadedAssetCount { get; private set; }

        /// <summary>
        /// 返回当前作用域中已登记资源路径的快照。
        /// 流式单元用它区分“本单元加载前已存在的共享引用”和“本单元加载期间新增的引用”。
        /// </summary>
        public HashSet<string> CaptureLoadedResourcePaths()
        {
            return new HashSet<string>(_loaded.Keys, System.StringComparer.Ordinal);
        }

        /// <summary>
        /// 只释放快照之后新增的引用，避免一个流式单元卸载时清空同作用域的其他单元。
        /// 传入 null 才表示明确请求释放整个作用域。
        /// </summary>
        public int ReleaseResourcesAddedSince(ISet<string> baselinePaths)
        {
            var pathsToRelease = new List<string>();
            foreach (var resourcePath in _loaded.Keys)
            {
                if (baselinePaths == null || !baselinePaths.Contains(resourcePath))
                    pathsToRelease.Add(resourcePath);
            }

            for (int i = 0; i < pathsToRelease.Count; i++)
                Release(pathsToRelease[i]);
            return pathsToRelease.Count;
        }

        /// <summary>配置本 scope 是否可以卸载其明确归属的 Resources 音频。</summary>
        public void ConfigureOwnership(bool allowAssetUnload, string ownedResourcePrefix = null)
        {
            _allowAssetUnload = allowAssetUnload;
            if (!string.IsNullOrWhiteSpace(ownedResourcePrefix))
                _ownedResourcePrefix = ownedResourcePrefix;
        }

        public AudioClip Load(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath)) return null;
            AudioClip clip;
            if (_loaded.TryGetValue(resourcePath, out clip)) return clip;
            clip = Resources.Load<AudioClip>(resourcePath);
            if (clip != null) TrackLoadedClip(resourcePath, clip);
            return clip;
        }

        /// <summary>
        /// 登记一个已经由表现加载器取得的 clip。普通运行时应优先使用 Load；
        /// 这个入口用于 Addressables/测试夹具等外部加载器，仍由本 scope 负责引用计数和释放边界。
        /// </summary>
        public bool TrackLoadedClip(string resourcePath, AudioClip clip)
        {
            return TrackLoadedClipInternal(null, resourcePath, clip);
        }

        /// <summary>
        /// 将资源登记到一个明确的流式 owner。相同路径可以被多个 owner 持有，
        /// 只有最后一个 owner 和无 owner 引用都释放后，底层 clip 才会进入释放判断。
        /// </summary>
        public bool TrackLoadedClipForOwner(string ownerId, string resourcePath, AudioClip clip)
        {
            if (string.IsNullOrWhiteSpace(ownerId)) return false;
            return TrackLoadedClipInternal(ownerId, resourcePath, clip);
        }

        public bool Release(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath)) return false;
            if (!_unownedPaths.Remove(resourcePath)) return false;
            TryReleaseUnderlying(resourcePath);
            return true;
        }

        /// <summary>释放一个流式 owner 仍持有的全部资源路径。</summary>
        public int ReleaseOwner(string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId)) return 0;
            HashSet<string> paths;
            if (!_pathsByOwner.TryGetValue(ownerId, out paths)) return 0;

            var pathsToRelease = new List<string>(paths);
            for (int i = 0; i < pathsToRelease.Count; i++)
            {
                var resourcePath = pathsToRelease[i];
                HashSet<string> owners;
                if (_ownersByPath.TryGetValue(resourcePath, out owners))
                {
                    owners.Remove(ownerId);
                    if (owners.Count == 0) _ownersByPath.Remove(resourcePath);
                }
                TryReleaseUnderlying(resourcePath);
            }
            _pathsByOwner.Remove(ownerId);
            return pathsToRelease.Count;
        }

        public void ReleaseAll()
        {
            var paths = new List<string>(_loaded.Keys);
            for (int i = 0; i < paths.Count; i++)
            {
                AudioClip clip;
                if (_loaded.TryGetValue(paths[i], out clip))
                    ReleaseReference(paths[i], clip);
            }
            _loaded.Clear();
            _unownedPaths.Clear();
            _pathsByOwner.Clear();
            _ownersByPath.Clear();
        }

        private bool TrackLoadedClipInternal(string ownerId, string resourcePath, AudioClip clip)
        {
            if (string.IsNullOrWhiteSpace(resourcePath) || clip == null) return false;
            AudioClip existing;
            if (_loaded.TryGetValue(resourcePath, out existing) && existing != clip) return false;
            if (!_loaded.ContainsKey(resourcePath))
            {
                _loaded[resourcePath] = clip;
                AddSharedReference(clip, resourcePath);
            }

            if (string.IsNullOrWhiteSpace(ownerId))
            {
                _unownedPaths.Add(resourcePath);
                return true;
            }

            HashSet<string> ownerPaths;
            if (!_pathsByOwner.TryGetValue(ownerId, out ownerPaths))
            {
                ownerPaths = new HashSet<string>(System.StringComparer.Ordinal);
                _pathsByOwner.Add(ownerId, ownerPaths);
            }
            if (!ownerPaths.Add(resourcePath)) return true;

            HashSet<string> owners;
            if (!_ownersByPath.TryGetValue(resourcePath, out owners))
            {
                owners = new HashSet<string>(System.StringComparer.Ordinal);
                _ownersByPath.Add(resourcePath, owners);
            }
            owners.Add(ownerId);
            return true;
        }

        private void AddSharedReference(AudioClip clip, string resourcePath)
        {
            int instanceId = clip.GetInstanceID();
            SharedReferenceState state;
            if (!SharedReferences.TryGetValue(instanceId, out state))
            {
                state = new SharedReferenceState();
                SharedReferences.Add(instanceId, state);
            }
            state.Count++;
            if (!IsExplicitlyOwned(resourcePath)) state.NonOwnedCount++;
        }

        private void TryReleaseUnderlying(string resourcePath)
        {
            if (_unownedPaths.Contains(resourcePath)) return;
            HashSet<string> owners;
            if (_ownersByPath.TryGetValue(resourcePath, out owners) && owners.Count > 0) return;

            AudioClip clip;
            if (!_loaded.TryGetValue(resourcePath, out clip)) return;
            _loaded.Remove(resourcePath);
            _ownersByPath.Remove(resourcePath);
            ReleaseReference(resourcePath, clip);
        }

        private void ReleaseReference(string resourcePath, AudioClip clip)
        {
            if (clip == null) return;
            int instanceId = clip.GetInstanceID();
            SharedReferenceState state;
            if (!SharedReferences.TryGetValue(instanceId, out state)) return;
            state.Count = Mathf.Max(0, state.Count - 1);
            if (!IsExplicitlyOwned(resourcePath))
                state.NonOwnedCount = Mathf.Max(0, state.NonOwnedCount - 1);
            if (state.Count > 0)
            {
                return;
            }
            SharedReferences.Remove(instanceId);
            if (state.NonOwnedCount == 0 && IsExplicitlyOwned(resourcePath))
            {
                Resources.UnloadAsset(clip);
                UnloadedAssetCount++;
            }
            else
            {
                SkippedSharedAssetReleaseCount++;
            }
        }

        private bool IsExplicitlyOwned(string resourcePath)
        {
            return _allowAssetUnload &&
                !string.IsNullOrWhiteSpace(resourcePath) &&
                resourcePath.StartsWith(_ownedResourcePrefix, System.StringComparison.OrdinalIgnoreCase);
        }

        private void OnDestroy()
        {
            ReleaseAll();
        }
    }
}
