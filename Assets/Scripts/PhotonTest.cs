using Photon.Pun;
using Photon.Realtime;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PhotonTest : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private PhotonView _photonView = null;

    [SerializeField]
    private GameObject _ball = null;

    [Header("- Text -")]
    [SerializeField]
    private Text _connectText = null;

    [SerializeField]
    private Text _playerListText = null;

    [Header("- Button -")]
    [SerializeField]
    private Button _connectButton = null;
    
    [SerializeField]
    private Button _createRoomButton = null;

    [SerializeField]
    private Button _joinRoomButton = null;

    [SerializeField]
    private Button _startButton = null;

    [SerializeField]
    private Button _createBallButton = null;

    private bool _isConnected = false;
    private bool _isPlay;

    private int _playerIndex;
    private int _currentIndex;
    private int _playerCount;

    private void Start()
    {
        _connectButton.OnClickAsObservable()
            .Subscribe(_ => ChangeConnect())
            .AddTo(this);

        _createRoomButton.OnClickAsObservable()
            .Subscribe(_ => CreateRoom())
            .AddTo(this);

        _joinRoomButton.OnClickAsObservable()
            .Subscribe(_ => JoinRoom())
            .AddTo(this);

        _startButton.OnClickAsObservable()
            .Subscribe(_ => 
            {
                _photonView.RPC("Setup", RpcTarget.All);
            })
            .AddTo(this);

        _createBallButton.OnClickAsObservable()
            .Where(_ => _currentIndex == _playerIndex)
            .Subscribe(_ => RequestCreateBall())
            .AddTo(this);

        PhotonNetwork.LocalPlayer.NickName = "Player_" + Random.Range(1, 100);
    }

    [PunRPC]
    private void Setup()
    {
        if (_isPlay)
        {
            return;
        }

        var player = PhotonNetwork.LocalPlayer;
        var playerList = PhotonNetwork.PlayerList;

        for (var i = 0; i < playerList.Length; ++i)
        {
            if (player == playerList[i])
            {
                _playerIndex = i;
                Debug.Log("player index = " + i);
                break;
            }
        }

        _playerCount = playerList.Length;
        _isPlay = true;
    }

    [PunRPC]
    private void CreateBall(float x, float y)
    {
        var obj = Instantiate(_ball);

        obj.transform.localPosition = new Vector2(x, y);

        ++_currentIndex;
        if (_currentIndex > _playerCount - 1)
        {
            _currentIndex = 0;
        }

        Debug.Log("current index = " + _currentIndex);
    }

    private void UpdatePlayerName()
    {
        var playerList = PhotonNetwork.PlayerList;
        _playerListText.text = string.Empty;

        foreach (var player in playerList)
        {
            _playerListText.text += player.NickName + "\n";
        }
    }
    
    private void RequestCreateBall()
    {
        var x = Random.Range(-30f, 30f);
        var y = Random.Range(-15f, 15f);
        
        _photonView.RPC("CreateBall", RpcTarget.All, x, y);
    }

    private void ChangeConnect()
    {
        if (_isConnected)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    private void CreateRoom()
    {
        PhotonNetwork.CreateRoom("room", new RoomOptions(), TypedLobby.Default);
    }

    private void JoinRoom()
    {
        PhotonNetwork.JoinRoom("room");
    }

    public override void OnConnectedToMaster()
    {
        _isConnected = true;
        _connectText.text = "Connect";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        _isConnected = false;
        _connectText.text = "Disconnect";
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("create room");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("join room");

        UpdatePlayerName();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("left room");

        UpdatePlayerName();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("other enter room");

        UpdatePlayerName();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("other left room");

        UpdatePlayerName();
    }
}
