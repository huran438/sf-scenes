using System;
using SFramework.Configs.Runtime;

// ReSharper disable once CheckNamespace
namespace SFramework.Scenes.Runtime
{
    [Serializable]
    public class SFSceneNode : SFConfigNode
    {
        public string Path;
        public override ISFConfigNode[] Children => null;
    }
}