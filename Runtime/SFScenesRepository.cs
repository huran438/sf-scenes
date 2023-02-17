using System;
using System.Collections.Generic;
using SFramework.Core.Runtime;
using SFramework.Repositories.Runtime;
using UnityEngine;

namespace SFramework.Scenes.Runtime
{
    public sealed class SFScenesRepository : SFRepository, ISFRepositoryGenerator
    {
        public SFScenesGroupContainer[] Groups;

        public override ISFNode[] Nodes => Groups;

        public void GetGenerationData(out SFGenerationData[] generationData)
        {
            var groups = new HashSet<string>();
            var scenes = new HashSet<string>();

            foreach (var layer0 in Groups)
            {
                groups.Add($"{_Name}/{layer0._Name}");
                foreach (var layer1 in layer0.Scenes)
                {
                    scenes.Add($"{_Name}/{layer0._Name}/{layer1._Name}");
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