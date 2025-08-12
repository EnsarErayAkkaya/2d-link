using UnityEngine;
using Eflatun.SceneReference;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

namespace BaseServices.SceneServices
{
    [CreateAssetMenu(fileName = "SceneConfig", menuName = "Scriptable Objects/Scenes/Scene Config", order = 1)]
    public class SceneConfig : ScriptableObject
    {
        public SceneReference sceneReference;
        public LoadSceneMode loadMode = LoadSceneMode.Additive;
        public bool removeAllOtherScenes = false;
        public bool showSceneTransition;
    }
}
