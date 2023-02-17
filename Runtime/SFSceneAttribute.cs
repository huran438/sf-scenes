using System;
using SFramework.Core.Runtime;
using SFramework.Repositories.Runtime;
using SFramework.Scenes.Runtime;

namespace SFramework.UI.Runtime
{
    public sealed class SFSceneAttribute : SFIdAttribute
    {
        public SFSceneAttribute() : base(typeof(SFScenesRepository), -1)
        {
        }
    }
}