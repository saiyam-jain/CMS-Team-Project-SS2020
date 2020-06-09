//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [Header("Settings")]
    public string Region = "eu";
    public TextMeshProUGUI connectStatus;

    public GameObject[] DisibleObjects;
    public GameObject[] EnableObjects;

    void Start()
    {
        ServerSettings settngs = PhotonNetwork.PhotonServerSettings; 
        PhotonNetwork.ConnectUsingSettings();
    }

    void Update()
    {
        connectStatus.text = "Status: " + PhotonNetwork.NetworkClientState;
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("We are now vconnected");
        foreach (GameObject obj in EnableObjects)
        {
            obj.SetActive(true);
        }
    }

    public void JoinOrCreateRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 10;
        ro.IsOpen = true;
        ro.IsVisible = true;
        int Id = Random.Range(0, 1000);
        PhotonNetwork.CreateRoom("Terminate" + Id, ro);
    }

    public override void OnJoinedRoom()
    {
        foreach (GameObject obj in DisibleObjects)
        {
            obj.SetActive(false);
        }
        GameObject player = PhotonNetwork.Instantiate("Player", new Vector3(0, 2, 0), Quaternion.identity, 0);

    }
}
