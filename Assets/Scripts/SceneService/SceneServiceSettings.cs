using Eflatun.SceneReference;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;

namespace BaseServices.SceneServices
{
    [CreateAssetMenu(fileName = "SceneServiceSettings", menuName = "Scriptable Objects/Scenes/Scene Service Settings", order = 0)]
    public sealed class SceneServiceSettings : ScriptableObject
    {
        [BoxGroup("Template Scenes")]
        public SceneConfig gameSceneConfig;
        public SceneConfig baseSceneConfig;

        [BoxGroup("All Scenes")]
        public List<SceneConfig> sceneConfigs;

        [Header("Utility Settings")]
        public float delayBeforeFirstSceneLoad;

        public SceneReference GetSceneReferenceByBuildIndex(int buildIndex)
        {
            SceneConfig sceneConfig = sceneConfigs.FirstOrDefault(s => s.sceneReference.BuildIndex == buildIndex);

            if (sceneConfig != null)
            {
                return sceneConfig.sceneReference;
            }

            return null;
        }
    }
}
