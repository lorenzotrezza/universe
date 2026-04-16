using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SharedData", menuName = "ScriptableObjects/SharedData", order = 2)]
public class SharedDataScriptable : ScriptableObject
{
    public PlayerDataClass LocalPlayer;
    public List<PlayerDataClass> Players = new List<PlayerDataClass>();

    private void OnEnable()
    {
        LocalPlayer = null;
        Players.Clear();
    }
}