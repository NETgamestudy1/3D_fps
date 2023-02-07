using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

public class Player{
    public PlayerRef playerRef;
    public NetworkObject playerObject;

    public Player(){}

    public Player(PlayerRef player, NetworkObject obj){
        playerRef = player;
        playerObject = obj;
    }
}

public class NetworkCallback : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner runner;
    public static NetworkCallback nc;
    public List<Player> runningPlayers = new List<Player>();
    public NetworkPrefabRef playerPrefab;
    private float yaw;
    public float Yaw{
        get { return yaw; }
        set{
            yaw = value;
            if(yaw < 0){
                yaw = 360f;
            }
            if(yaw > 360){
                yaw = 0;
            }
        }
    }
    private float pitch;
    public float Pitch{
        get { return pitch; }
        set{
            pitch = value;
        }
    }

    private void Awake() {
        if (nc == null){
            nc = this;
            runner = gameObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;
        }
        else if (nc != this){
            Destroy(gameObject);
        }
    }

    private async void RunGame(GameMode mode){
        var gameArgs = new StartGameArgs();
        gameArgs.GameMode = mode;
        gameArgs.SessionName = "Test";
        gameArgs.PlayerCount = 10;

        await runner.StartGame(gameArgs);
        runner.SetActiveScene(1);
    }

    private void OnGUI() {
        if (GUI.Button(new Rect(0, 0, 160, 80), "Host")){
            RunGame(GameMode.Host);
        }
        if (GUI.Button(new Rect(0, 85, 160, 80), "Client")){
            RunGame(GameMode.Client);
        }
    }
    
    void Update()
    {
        yaw += Input.GetAxis("Mouse X");
        pitch -= Input.GetAxis("Mouse Y");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var myInput = new NetworkInputData();
        myInput.buttons.Set(Buttons.forward, Input.GetKey(KeyCode.W));
        myInput.buttons.Set(Buttons.back, Input.GetKey(KeyCode.S));
        myInput.buttons.Set(Buttons.right, Input.GetKey(KeyCode.D));
        myInput.buttons.Set(Buttons.left, Input.GetKey(KeyCode.A));
        myInput.buttons.Set(Buttons.jump, Input.GetKey(KeyCode.Space));

        myInput.pitch = Pitch;
        myInput.yaw = Yaw;

        input.Set(myInput);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if(!this.runner.IsServer) return;

        runningPlayers.Add(new Player(player, null));
        
        foreach(var players in runningPlayers){
            if(players.playerObject != null) continue;

            var obj = this.runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, players.playerRef);
            players.playerObject = obj;
            var cc = obj.GetComponent<CharacterController>();
            cc.enabled = false;
            obj.transform.position = new Vector3(0, 10, 0);
            cc.enabled = true;
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!this.runner.IsServer) return;

        foreach (var players in runningPlayers)
        {
            if(players.playerRef.Equals(player)){
                this.runner.Despawn(players.playerObject);
                runningPlayers.Remove(players);
            }
            break;
        }
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    { 
        if(!this.runner.IsServer) return;

        foreach(var players in runningPlayers){
            var obj = this.runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, players.playerRef);
            players.playerObject = obj;
            var cc = obj.GetComponent<CharacterController>();
            cc.enabled = false;
            obj.transform.position = new Vector3(0, 10, 0);
            cc.enabled = true;
        }
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        if(!this.runner.IsServer) return;
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
}
