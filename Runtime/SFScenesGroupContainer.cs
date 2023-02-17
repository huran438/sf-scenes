using System;
using SFramework.Core.Runtime;
using SFramework.Repositories.Runtime;
using UnityEngine;

namespace SFramework.Scenes.Runtime
{
    [Serializable]
    public class SFScenesGroupContainer : SFNode
    {
        public SFSceneNode[] Scenes;
        public override ISFNode[] Nodes => Scenes;
    }
}