using System;
using System.Collections.Generic;
using SFramework.Core.Runtime;
using UnityEngine;

namespace SFramework.Scenes.Runtime
{
    [CreateAssetMenu(menuName = "SFramework/Scenes Database")]
    public sealed class SFScenesDatabase : SFDatabase
    {
 
        [SerializeField]
        private SFScenesGroupContainer[] _groups;

        public override string Title => "Scenes";
        public override ISFDatabaseNode[] Nodes => _groups;

        // protected override void Generate(out SFGenerationData[] generationData)
        // {
        //     var groups = new Dictionary<string, string>();
        //     var scenes = new Dictionary<string, string>();
        //
        //     foreach (var layer0 in _groups)
        //     {
        //         groups[layer0._Id] = $"{layer0._Name}";
        //         foreach (var layer1 in layer0.Scenes)
        //         {
        //             scenes[layer1._Id] = $"{layer0._Name}/{layer1._Name}";
        //         }
        //     }
        //
        //     generationData = new[]
        //     {
        //         new SFGenerationData
        //         {
        //             FileName = "SFScenes",
        //             Usings = new[]
        //             {
        //                 "using SFramework.Scenes.Runtime;",
        //             },
        //             SFType = typeof(SFScene),
        //             Properties = scenes
        //         },
        //         new SFGenerationData
        //         {
        //             FileName = "SFScenesGroups",
        //             Usings = new string[]
        //             {
        //                 "using SFramework.Scenes.Runtime;"
        //             },
        //             SFType = typeof(SFScenesGroup),
        //             Properties = groups
        //         }
        //     };
        // }
    }
}