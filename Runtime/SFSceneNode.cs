using System;
using SFramework.Repositories.Runtime;

// ReSharper disable once CheckNamespace
namespace SFramework.Scenes.Runtime
{
    [Serializable]
    public class SFSceneNode : SFNode
    {
        public string Path;
        public override ISFNode[] Nodes => null;
    }
}