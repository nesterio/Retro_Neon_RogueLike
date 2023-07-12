using UnityEditor;
using UnityEngine;

namespace MSZ.FlatColorizer
{
    [CustomEditor(typeof(FlatColorizer))]
    public class FlatColorizerEditor : Editor
    {
        private FlatColorizer _flatColorizer;
        private int _colorsCount;

        private void OnEnable()
        {
            _flatColorizer = serializedObject.targetObject as FlatColorizer;

            if (_flatColorizer != _flatColorizer.GetComponent<FlatColorizer>())
            {
                Debug.LogError($"there is already a {nameof(FlatColorizer)} component");
                DestroyImmediate(_flatColorizer);
                return;
            }

            _flatColorizer.FlatColorize();
            _colorsCount = _flatColorizer.GetColorCount();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            for (int i = 0; i < _colorsCount; i++)
            {
                Color colorValue = _flatColorizer.GetColorAtIndex(i);

                EditorGUI.BeginChangeCheck();

                colorValue = EditorGUILayout.ColorField("Color " + i, colorValue);

                if (EditorGUI.EndChangeCheck())
                {
                    _flatColorizer.UpdateColor(i, colorValue);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
