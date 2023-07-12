using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MSZ.FlatColorizer
{
    [System.Serializable]
    public class FlatColoredMeshData : ScriptableObject
    {
#if UNITY_EDITOR

        [HideInInspector] public int[] occupiedPixelsIndexes = new int[0];
        [HideInInspector] public List<ColorGroupData> colorGroups;

        [HideInInspector] public Mesh targetMesh;

        private void AddToColorGroups(int uvIndex, Vector2 uvPos)
        {
            for (int i = 0; i < colorGroups.Count; i++)
            {
                if (colorGroups[i].pos == uvPos)
                {
                    colorGroups[i].AddPosIndex(uvIndex);
                    return;
                }
            }

            colorGroups.Add(new ColorGroupData(uvPos));
            colorGroups[colorGroups.Count - 1].AddPosIndex(uvIndex);

            EditorUtility.SetDirty(this);
        }

        public void SetOccupiedPixelsIndexes(int[] indexes)
        {
            occupiedPixelsIndexes = indexes;

            EditorUtility.SetDirty(this);
        }

        public void SetColorGroupsUvPositions(Vector2[] positions)
        {
            targetMesh = FlatColorizerManager.GetFlatColorizedMesh(targetMesh);

            Vector2[] uv = targetMesh.uv;
            for (int i = 0; i < positions.Length; i++)
            {
                Vector2 curTargetPos = positions[i];
                if (colorGroups[i].pos != curTargetPos)
                {
                    colorGroups[i].pos = curTargetPos;

                    for (int e = 0; e < colorGroups[i].uvPosIndexes.Count; e++)
                    {
                        int uvIndex = colorGroups[i].uvPosIndexes[e];
                        uv[uvIndex] = curTargetPos;
                    }
                }
            }
            targetMesh.uv = uv;

            EditorUtility.SetDirty(this);
        }

        public void SetColor(int colorIndex, Color color)
        {
            colorGroups[colorIndex].color = color;

            int pixelIndex = occupiedPixelsIndexes[colorIndex];
            TextureManager.SetPixelColor(pixelIndex, color);

            EditorUtility.SetDirty(this);
        }

        public void SetData(Mesh mesh)
        {
            colorGroups = new List<ColorGroupData>();

            for (int i = 0; i < mesh.uv.Length; i++)
            {
                AddToColorGroups(i, mesh.uv[i]);
            }
            targetMesh = mesh;
            TextureManager.InitMeshData(this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
