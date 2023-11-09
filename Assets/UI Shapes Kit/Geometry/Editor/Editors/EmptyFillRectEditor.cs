using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

namespace ThisOtherThing.Appreal.UI_ShapesKit
{
    [CustomEditor(typeof(EmptyFillRect))]
    [CanEditMultipleObjects]
    public class EmptyFillRectEditor : GraphicEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            RaycastControlsGUI();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
