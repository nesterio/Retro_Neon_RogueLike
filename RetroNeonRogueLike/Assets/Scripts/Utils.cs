using UnityEngine;

public static class Utils
{
    public static GameObject FindGameObjectByName(string name, Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (string.Equals(child.name, name))
                return child.gameObject;
        }
        
        Debug.LogError($"Couldn't find a gameObject named: '{name}' inside the: '{parent.name}'");
        return null;
    }
}
