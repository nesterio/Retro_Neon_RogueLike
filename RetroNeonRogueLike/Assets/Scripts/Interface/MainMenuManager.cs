using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;

    [SerializeField] private Menu[] menus;
    private Dictionary<string, Menu> menusDictionary = new Dictionary<string, Menu>();

    void Awake() 
    {
        Instance = this;

        foreach (Menu menu in menus)
        {
            menusDictionary.Add(menu.name, menu);
        }
    }

    public void OpenMenu(string _name) 
    {
        try
        {
            foreach (var menu in menusDictionary)
                CloseMenu(menu.Value.name);

            menusDictionary[_name].menuObject.SetActive(true);
        }
        catch 
        {
            Debug.Log("Menu with name" + _name + " does not exist!");
        }
        
    }

    public void CloseMenu(string _name)
    {
        try
        {
            menusDictionary[_name].menuObject.SetActive(false);
        }
        catch
        {
            Debug.Log("Menu with name" + _name + " does not exist!");
        }
    }

    public void ExitGame() 
    {
        Application.Quit();
    }

}

[System.Serializable]
public struct Menu 
{
    public string name;

    public GameObject menuObject;

    public bool isOpen
    {
        get { return menuObject.activeSelf; }
    }
}
