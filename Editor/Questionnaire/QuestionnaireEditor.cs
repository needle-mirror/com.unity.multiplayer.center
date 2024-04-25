using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Questionnaire
{
    /// <summary>
    /// Because of the "questionnaire" extension, the default inspector is not shown.
    /// Double clicking on the asset will open this custom inspector (in debug mode), which has a way to force saving
    /// the asset.
    /// </summary>
    [CustomEditor(typeof(QuestionnaireObject))]
    internal class QuestionnaireEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var so = new SerializedObject(target);
            var root = new VisualElement();
            var questionnaire = (QuestionnaireObject) target;
            root.Add(new Button(() => questionnaire.ForceSave()){text = "Apply changes"});
            var defaultInspector = new VisualElement();
            InspectorElement.FillDefaultInspector(defaultInspector, so, this);
            root.Add(defaultInspector);
            return root;
        }

        [OnOpenAsset(1)]
        public static bool OpenMyCustomAsset(int instanceID, int line)
        {
            if (!EditorPrefs.GetBool("DeveloperMode")) return false;
            var asset = EditorUtility.InstanceIDToObject(instanceID);
            var path = AssetDatabase.GetAssetPath(asset);
            if(string.IsNullOrEmpty(path) || !path.EndsWith("questionnaire"))
                return false;

            Selection.activeObject = QuestionnaireObject.instance;
            return true;
        }
    }
}