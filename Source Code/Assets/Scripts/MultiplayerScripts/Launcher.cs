﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Realtime;
using System.Linq;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Launcher : MonoBehaviourPunCallbacks
{
    [Header("UI Management")]
    public Button MultiplayerButton, errorReturnButton;
    public GameObject ConnectionObject;

    [Header("Creating Rooms & Finding Rooms")]
    [SerializeField] TMP_InputField roomNameInputField;
    public TMP_Text errorText;
    public GameObject FindRoomMenu;
    public GameObject CreateRoomMenu;

    [Header("Room Menu Stuff")]
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject playerListItemPrefab;

    [Header("Host Stuff")]
    [SerializeField] GameObject[] hostControls;

    [Header("Miscallenous")]
    [SerializeField] Animator animatedPanelSceneSwitcher;
    [SerializeField] TMP_Text playerCount;
    [SerializeField] TMP_Text playerVersionWelcome;
    PhotonView PV;

    //for festivity
    [SerializeField] public Image[] ChristmasTrees;



    public static Launcher instance;
    int Map;
    string gameVersion;


    private void Awake()
    {
        instance = this;
        Map = 6;
        gameVersion = playerVersionWelcome.text;
        PV = GetComponent<PhotonView>();
    }

     void Start()
     {
           ConnectToPhoton();
     }

    // Start is called before the first frame update
    void ConnectToPhoton()
    {
        ConnectingToPhoton();
        PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    void Update()
    {
        playerCount.text = "Players online: " + "\n" + PhotonNetwork.CountOfPlayers;
    }


    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("joined photon. and uh.. how much average ping photon?" + "lets see... about... " + PhotonNetwork.GetPing() + "... ping!");
        ConnectedToPhoton();
        if(PlayerPrefs.HasKey("Username"))
        {
            RefreshMenuUsername();

        } else
        {
            CreateMenuUsername();
        }

        PhotonNetwork.AutomaticallySyncScene = true;

    }

    public void RefreshMenuUsername()
    {
        PhotonNetwork.NickName = PlayerPrefs.GetString("Username");

        playerVersionWelcome.text = gameVersion + " - Welcome, " + PhotonNetwork.NickName + ".";
    }

    public void CreateMenuUsername()
    {
        PhotonNetwork.NickName = "Player " + Random.Range(0, 9999).ToString("0000");
        PlayerPrefs.SetString("Username", PhotonNetwork.NickName);
        playerVersionWelcome.text = gameVersion + " - Welcome, " + PhotonNetwork.NickName + ".";
        PlayerPrefs.Save();
    }

    public void SinglePlayerCampaign()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.LeaveRoom();

        }

        StartCoroutine(LoadScene(2));

    }

    IEnumerator LoadScene(int sceneToLoad)
    {
        TransitionManager.instance.Close();
        yield return new WaitForSeconds(0.75f);
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(sceneToLoad);
        RoomManager.instance.OnDestroy();
    }

    public void LocalScene(string sceneToSwitchTo)
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.LeaveRoom();

        }

        StartCoroutine(LoadScene1(sceneToSwitchTo));
    }

    IEnumerator LoadScene1(string sceneToLoad2)
    {
        TransitionManager.instance.Close();
        yield return new WaitForSeconds(0.75f);
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(sceneToLoad2);
        RoomManager.instance.OnDestroy();
    }



    public void SelectMap(int mapSceneToSelect)
    {
        Map = mapSceneToSelect;
    }
    

    public void ConnectingToPhoton()
    {
            MultiplayerButton.interactable = false;
            ConnectionObject.SetActive(true);
    }

    public void ConnectedToPhoton()
    {
            MultiplayerButton.interactable = true;
            ConnectionObject.SetActive(false);
    }


    public void CreateRoom()
    {
        if(string.IsNullOrEmpty(roomNameInputField.text))
        {
            PhotonNetwork.CreateRoom(PhotonNetwork.NickName + "'s Room");
            MenuManager.Instance.OpenMenu("Loading");
        }

        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.Instance.OpenMenu("Loading");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu("Room Menu");



        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count(); i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(players[i]);
        }

        foreach (GameObject hostVisuals in hostControls)
        {
            hostVisuals.SetActive(PhotonNetwork.IsMasterClient);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        foreach (GameObject hostVisuals in hostControls)
        {
            hostVisuals.SetActive(PhotonNetwork.IsMasterClient);
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorReturnButton.enabled = true;
        errorText.text = "error: " + message + ". Error Code: " + returnCode;
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("Loading");
    }


    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("Find Room");
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("Loading");
        FindRoomMenu.SetActive(false);
        CreateRoomMenu.SetActive(false);

    }

    public void StartGame()
    {
        StartCoroutine(PhotonLoadYes(Map));
    }

    IEnumerator PhotonLoadYes(int map)
    {
        TransitionManager.instance.Close();
        yield return new WaitForSeconds(0.78f);
        PhotonNetwork.LoadLevel(map);

    }


    



    public void AppQuit()
    {
        StartCoroutine(QuitAppYes());
    }

    IEnumerator QuitAppYes()
    {
        TransitionManager.instance.Close();
        yield return new WaitForSeconds(0.78f);
        Application.Quit();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        {
            foreach (Transform transform in roomListContent)
            {
                Destroy(transform.gameObject);
            }

            for (int i = 0; i < roomList.Count; i++)
            {
                if (roomList[i].RemovedFromList)
                    continue;
                Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().Setup(roomList[i]);
            }
        }
    }

    public void UnfinishedSecret(TMP_Text text)
    {
        AchievementManager.instance.AchievementGet("Unfinished Secret");
        text.text = "Unfinished Secret...";
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(newPlayer);
    }

    //a type of menu room kick system. I think.

    //nope, it's a gameMode system.
    [Header("Gamemode System")]
    public Animator gameModeAnimator;
    public TMP_Text lobbyTypeText;

    public void OnRoomSettingsWanted()
    {
        gameModeAnimator.Play("RoomOptions");
    }

    public void OnMainContentWanted()
    {
        gameModeAnimator.Play("MainContent");
    }

    public void ToggleManhuntLobby(Toggle toggle)
    {
        if(toggle.isOn)
        {
            SetGamemode("True");

        } else
        {
            SetGamemode("False");
        }
    }

    void SetGamemode(string isTrueOrFalse)
    {
        if (PV.IsMine)
        {
            Hashtable gamemode = new Hashtable();
            gamemode.Add("Is Manhunt Lobby", isTrueOrFalse);
            PhotonNetwork.LocalPlayer.SetCustomProperties(gamemode);
            Debug.Log((string)gamemode["Is Manhunt Lobby"]);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {

            if((string)changedProps["Is Manhunt Lobby"] == "True")
            {
                lobbyTypeText.text = "This is a Manhunt lobby";

            }
            else if ((string)changedProps["Is Manhunt Lobby"] == "False")
            {
                lobbyTypeText.text = "This is a normal lobby";

            } else
            {
                Debug.LogWarning("A player property has changed, but we can't get it.");
            }

    }





















    //section for menu manager
}
