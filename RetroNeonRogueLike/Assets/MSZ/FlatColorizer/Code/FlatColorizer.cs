using UnityEditor;
using UnityEngine;

namespace MSZ.FlatColorizer
{
    public class FlatColorizer : MonoBehaviour
    {
#if UNITY_EDITOR

        private MeshRenderer _renderer;
        private MeshFilter _meshFilter;
        private FlatColoredMeshData _meshData;

        private void AddMaterial()
        {
            if (_renderer == null)
                _renderer = GetComponent<MeshRenderer>();


            Material sharedMat = FlatColorizerManager.GetSharedMaterial();

            if (_renderer.sharedMaterial != sharedMat)
                _renderer.sharedMaterials = new Material[] { sharedMat };
        }

        private void AddMesh()
        {
            if (_meshFilter == null)
                _meshFilter = GetComponent<MeshFilter>();

            if (_meshFilter.sharedMesh == null)
            {
                Debug.LogError("there is no mesh to flat colorize");
                return;
            }

            Mesh flatColoredMesh = FlatColorizerManager.GetFlatColorizedMesh(_meshFilter.sharedMesh);
            if (_meshFilter.sharedMesh != flatColoredMesh)
                _meshFilter.sharedMesh = flatColoredMesh;
        }



        public int GetColorCount()
        {
            if (_meshFilter.sharedMesh == null)
                return 0;

            if (_meshData == null)
                _meshData = FlatColorizerManager.GetMeshData(_meshFilter.sharedMesh);

            return _meshData.colorGroups.Count;
        }

        public Color GetColorAtIndex(int index)
        {
            if (_meshFilter.sharedMesh == null)
                return Color.white;

            if (_meshData == null)
                _meshData = FlatColorizerManager.GetMeshData(_meshFilter.sharedMesh);

            return _meshData.colorGroups[index].color;
        }

        public void UpdateColor(int colorIndex, Color color)
        {
            if (_meshFilter.sharedMesh == null)
                return;

            _meshData.SetColor(colorIndex, color);
        }

        public void FlatColorize()
        {
            AddMaterial();
            AddMesh();
        }

#endif
    }
}
