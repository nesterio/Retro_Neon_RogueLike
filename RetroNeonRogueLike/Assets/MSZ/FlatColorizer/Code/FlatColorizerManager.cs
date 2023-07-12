using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MSZ.FlatColorizer
{
    public static class FlatColorizerManager
    {
#if UNITY_EDITOR
        private static string _flatColored = "FlatColored_";

        private static string _rootFilePath = "Assets/FlatColorizerData";
        private static string _materialFilePath = _rootFilePath + "/Material";
        private static string _meshFilePath = _rootFilePath + "/Mesh";
        private static string _meshDataFilePath = _rootFilePath + "/MeshData";

        private static string _sharedMaterialName = "SharedMat.mat";
        private static string _sharedTextureName = "Texture.asset";

        private static void EnsureFileCreation(string folderPath)
        {
            // Check if the folder exists, and create it if it doesn't
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }
        }

        private static Material CreateMaterial()
        {
            EnsureFileCreation(_materialFilePath);

            // Create a new material
            Material newMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            newMaterial.mainTexture = GetTexture();
            newMaterial.SetFloat("_Smoothness", 0.25f);

            // Save the material to a file
            string materialPath = $"{_materialFilePath}/{_sharedMaterialName}";
            AssetDatabase.CreateAsset(newMaterial, materialPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("new material created");
            return newMaterial;
        }

        private static Texture2D CreateTexture()
        {
            EnsureFileCreation(_materialFilePath);

            // Create a new texture
            Texture2D texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            // Save the texture to a file
            string texturePath = $"{_materialFilePath}/{_sharedTextureName}";
            AssetDatabase.CreateAsset(texture, texturePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("new texture created");
            return texture;
        }

        private static Mesh CreateFlatColoredMesh(Mesh mesh)
        {
            EnsureFileCreation(_meshFilePath);

            // Create a new mesh
            Mesh flatColoredMesh = new Mesh();
            flatColoredMesh.vertices = mesh.vertices;
            flatColoredMesh.triangles = mesh.triangles;
            flatColoredMesh.uv = mesh.uv;
            flatColoredMesh.normals = mesh.normals;
            flatColoredMesh.colors = mesh.colors;
            flatColoredMesh.tangents = mesh.tangents;

            // Save the mesh to a file
            string meshPath = $"{_meshFilePath}/{_flatColored}{mesh.name}.asset";
            AssetDatabase.CreateAsset(flatColoredMesh, meshPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            CreateFlatColoredMeshData(flatColoredMesh);

            Debug.Log("new mesh created");
            return flatColoredMesh;
        }

        private static FlatColoredMeshData CreateFlatColoredMeshData(Mesh flatColoredMesh)
        {
            EnsureFileCreation(_meshDataFilePath);

            // Create a new mesh data
            FlatColoredMeshData flatColoredMeshData = ScriptableObject.CreateInstance<FlatColoredMeshData>();


            // Save the mesh data to a file
            string meshDataPath = $"{_meshDataFilePath}/{flatColoredMesh.name}_data.asset";
            AssetDatabase.CreateAsset(flatColoredMeshData, meshDataPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            flatColoredMeshData.SetData(flatColoredMesh);

            Debug.Log("new FlatColoredMeshData created");
            return flatColoredMeshData;
        }

        public static Material GetSharedMaterial()
        {
            Material sharedMat = AssetDatabase.LoadAssetAtPath<Material>($"{_materialFilePath}/{_sharedMaterialName}");
            if (sharedMat == null)
                sharedMat = CreateMaterial();

            return sharedMat;
        }

        public static Texture2D GetTexture()
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{_materialFilePath}/{_sharedTextureName}");
            if (texture == null)
                texture = CreateTexture();

            return texture;
        }

        public static Mesh GetFlatColorizedMesh(Mesh mesh)
        {
            if (mesh.name.Contains(_flatColored))
                return mesh;

            string meshName = mesh.name.Contains(_flatColored) ? mesh.name : _flatColored + mesh.name;

            Mesh flatColoredMesh = AssetDatabase.LoadAssetAtPath<Mesh>($"{_meshFilePath}/{meshName}.asset");
            if (flatColoredMesh == null)
                flatColoredMesh = CreateFlatColoredMesh(mesh);

            return flatColoredMesh;
        }

        public static FlatColoredMeshData GetMeshData(Mesh mesh)
        {
            FlatColoredMeshData meshData = AssetDatabase.LoadAssetAtPath<FlatColoredMeshData>($"{_meshDataFilePath}/{mesh.name}_data.asset");
            if (meshData == null)
                Debug.LogError($"{mesh.name}_data not found");

            return meshData;
        }

        public static FlatColoredMeshData[] GetAllMeshesData()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(FlatColoredMeshData)}", new[] { _meshDataFilePath });

            List<FlatColoredMeshData> meshesData = new List<FlatColoredMeshData>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                FlatColoredMeshData data = AssetDatabase.LoadAssetAtPath<FlatColoredMeshData>(path);

                meshesData.Add(data);
            }

            return meshesData.ToArray();
        }
#endif

    }
}
