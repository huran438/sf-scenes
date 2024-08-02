using System;
using System.Collections.Generic;
using SFramework.Configs.Runtime;
using SFramework.Core.Runtime;
using UnityEngine;

namespace SFramework.Scenes.Runtime
{
    public sealed class SFScenesConfig : SFNodesConfig
    {
        public SFScenesGroupContainer[] Groups;
        public override ISFConfigNode[] Children => Groups;
    }
}