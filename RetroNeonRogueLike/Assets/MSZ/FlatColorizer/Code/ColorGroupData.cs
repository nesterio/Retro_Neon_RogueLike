using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MSZ.FlatColorizer
{
    [System.Serializable]
    public class ColorGroupData
    {
#if UNITY_EDITOR
        public List<int> uvPosIndexes = new List<int>();
        public Vector2 pos;
        public Color color;

        public ColorGroupData(Vector2 targetPos)
        {
            uvPosIndexes = new List<int>();
            pos = targetPos;
            color = Color.white;
        }

        public void AddPosIndex(int index)
        {
            if (uvPosIndexes.Contains(index))
                Debug.Log($"the index {index} is already added");

            uvPosIndexes.Add(index);
        }
#endif
    }
}
