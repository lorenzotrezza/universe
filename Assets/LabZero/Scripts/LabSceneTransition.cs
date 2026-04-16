using UnityEngine.SceneManagement;

public static class LabSceneTransition
{
    public const string LobbyScene = "LabZero_Prototype";
    public const string WarehouseScene = "LabWarehouse";

    public static void LoadWarehouse()
    {
        SceneManager.LoadScene(WarehouseScene, LoadSceneMode.Single);
    }

    public static void RestartWarehouse()
    {
        SceneManager.LoadScene(WarehouseScene, LoadSceneMode.Single);
    }

    public static void ReturnToLobby()
    {
        SceneManager.LoadScene(LobbyScene, LoadSceneMode.Single);
    }
}
