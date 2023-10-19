using System;
using SFramework.Configs.Runtime;
using SFramework.Core.Runtime;
using SFramework.Scenes.Runtime;

namespace SFramework.UI.Runtime
{
    public sealed class SFSceneAttribute : SFIdAttribute
    {
        public SFSceneAttribute() : base(typeof(SFScenesConfig), -1)
        {
        }
    }
}