using System;
using SFramework.Core.Runtime;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.AddressableAssets;

// ReSharper disable once CheckNamespace
namespace SFramework.Scenes.Runtime
{
    [Serializable]
    public class SFSceneContainer : SFDatabaseNode
    {
        public AssetReference Scene => _scene;

        [SerializeField]
        private AssetReference _scene;

        public override ISFDatabaseNode[] Children => null;
    }
}