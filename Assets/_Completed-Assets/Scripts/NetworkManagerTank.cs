using Mirror;
using UnityEngine;

public class NetworkManagerTank : NetworkManager
{
    public Transform[] spawnPosition;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // add player at correct spawn position
        Transform start = numPlayers == 0 ? spawnPosition[0] : spawnPosition[1];
        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
        //var tm = player.GetComponent<TankManager>();
        //tm.m_PlayerColor = numPlayers == 0 ? Color.blue : Color.red;
        //tm.m_SpawnPoint = start;
        //tm.Setup();

        NetworkServer.AddPlayerForConnection(conn, player);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
    }
}
