using System;
using SFramework.Configs.Runtime;
using SFramework.Core.Runtime;
using UnityEngine;

namespace SFramework.Scenes.Runtime
{
    [Serializable]
    public class SFScenesGroupContainer : SFConfigNode
    {
        public SFSceneNode[] Scenes;
        public override ISFConfigNode[] Children => Scenes;
    }
}