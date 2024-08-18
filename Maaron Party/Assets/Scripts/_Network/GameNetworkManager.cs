using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameNetworkManager : NetworkManager
{
	public Transform spawnHolder;
	//GameObject ball;

	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		// add player at correct spawn position
		Transform start = spawnHolder;
		GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
		NetworkServer.AddPlayerForConnection(conn, player);

		// spawn ball if two players
		//if (numPlayers == 2)
		//{
		//	ball = Instantiate(spawnPrefabs.Find(prefab => prefab.name == "Ball"));
		//	NetworkServer.Spawn(ball);
		//}
	}

	public override void OnServerDisconnect(NetworkConnectionToClient conn)
	{
		// destroy ball
		//if (ball != null)
		//	NetworkServer.Destroy(ball);

		// call base functionality (actually destroys the player)
		base.OnServerDisconnect(conn);
	}
}
