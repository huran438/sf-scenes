using System;
using System.Collections.Generic;
using SFramework.Configs.Runtime;
using SFramework.Core.Runtime;
using UnityEngine;

namespace SFramework.Scenes.Runtime
{
    public sealed class SFScenesConfig : SFConfig, ISFConfigsGenerator
    {
        public SFScenesGroupContainer[] Groups;

        public override ISFConfigNode[] Nodes => Groups;

        public void GetGenerationData(out SFGenerationData[] generationData)
        {
            var groups = new HashSet<string>();
            var scenes = new HashSet<string>();

            foreach (var layer0 in Groups)
            {
                groups.Add($"{Name}/{layer0.Name}");
                foreach (var layer1 in layer0.Scenes)
                {
                    scenes.Add($"{Name}/{layer0.Name}/{layer1.Name}");
                }
            }

            generationData = new[]
            {
                new SFGenerationData
                {
                    FileName = "SFScenes",
                    Properties = scenes
                },
                new SFGenerationData
                {
                    FileName = "SFScenesGroups",
                    Properties = groups
                }
            };
        }
    }
}