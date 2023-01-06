using System;
using SFramework.Core.Runtime;
using SFramework.Scenes.Runtime;

namespace SFramework.UI.Runtime
{
    public sealed class SFSceneAttribute : SFTypeAttribute
    {
        public SFSceneAttribute() : base(typeof(SFScenesDatabase))
        {
        }
    }
}