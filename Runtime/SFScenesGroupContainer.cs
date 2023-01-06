using System;
using SFramework.Core.Runtime;
using UnityEngine;

namespace SFramework.Scenes.Runtime
{
    [Serializable]
    public class SFScenesGroupContainer : SFDatabaseNode
    {
        [SerializeField]
        private SFSceneContainer[] _scenes;

        public override ISFDatabaseNode[] Children => _scenes;
    }
}