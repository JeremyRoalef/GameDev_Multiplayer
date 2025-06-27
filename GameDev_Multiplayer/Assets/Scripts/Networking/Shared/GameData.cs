using System;

//Enum for all the maps in the game
public enum Map
{
    Default
}

//Enum for all the game modes
public enum GameMode
{
    Default
}

//Enum for types of queuing
public enum GameQueue
{
    Solo,
    Team
}

//User data shared between clients when connecting
[Serializable]
public class UserData
{
    public string userName;
    public string userAuthId;
    public GameInfo userGamePreferences;
}

[Serializable]
public class GameInfo
{
    public Map map;
    public GameMode gameMode;
    public GameQueue gameQueue;

    public string ToMultiplayQueue()
    {
        return "";
    }
}
