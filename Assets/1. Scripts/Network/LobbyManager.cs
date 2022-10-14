//using Photon.Pun; // 유니티용 포톤 컴포넌트들
//using Photon.Realtime; // 포톤 서비스 관련 라이브러리
//using UnityEngine;
//using UnityEngine.UI;

//// 마스터(매치 메이킹) 서버와 룸 접속을 담당
//public class LobbyManager : MonoBehaviourPunCallbacks
//{

//    private string gameVersion = "1"; // 게임 버전

//    public Text connectionInfoText; // 네트워크 정보를 표시할 텍스트
//    public Button joinButton; // 룸 접속 버튼

//    // 게임 실행과 동시에 마스터 서버 접속 시도
//    private void Start()
//    {
//        // 접속에 필요한 정보(게임 버전) 설정
//        PhotonNetwork.GameVersion = gameVersion;
//        Screen.SetResolution(800, 400, false);
//        // 설정한 정보를 가지고 마스터 서버 접속 시도
//        PhotonNetwork.ConnectUsingSettings();

//        // 룸 접속 버튼을 잠시 비활성화
//        joinButton.interactable = false;
//        // 접속을 시도 중임을 텍스트로 표시
//        connectionInfoText.text = "Connection Status: Connect to Master";
//    }


//    // 마스터 서버 접속 성공시 자동 실행
//    public override void OnConnectedToMaster()
//    {
//        // 룸 접속 버튼을 활성화
//        joinButton.interactable = true;
//        // 접속 정보 표시
//        connectionInfoText.text = "Connected to Master - Online";
//    }


//    // 마스터 서버 접속 실패시 자동 실행
//    public override void OnDisconnected(DisconnectCause cause)
//    {
//        // 룸 접속 버튼을 비활성화
//        joinButton.interactable = false;
//        // 접속 정보 표시
//        connectionInfoText.text = "Not Connected to Master - OffLine\n Retry....";

//        // 마스터 서버로의 재접속 시도
//        PhotonNetwork.ConnectUsingSettings();
//    }



//    // 룸 접속 시도
//    public void Connect()
//    {
//        // 중복 접속 시도를 막기 위해, 접속 버튼 잠시 비활성화
//        joinButton.interactable = false;

//        // 마스터 서버에 접속중이라면
//        if (PhotonNetwork.IsConnected)
//        {
//            // 룸 접속 실행
//            connectionInfoText.text = "Connection Status : Connecting...";
//            PhotonNetwork.JoinRandomRoom();
//        }
//        else
//        {
//            // 마스터 서버에 접속중이 아니라면, 마스터 서버에 접속 시도
//            connectionInfoText.text = "Not Connected to Master - OffLine\n Retry....";
//            // 마스터 서버로의 재접속 시도
//            PhotonNetwork.ConnectUsingSettings();
//        }
//    }


//    // (빈 방이 없어)랜덤 룸 참가에 실패한 경우 자동 실행
//    public override void OnJoinRandomFailed(short returnCode, string message)
//    {
//        // 접속 상태 표시
//        connectionInfoText.text = "No EMPTY Room, Let's Create Room...";
//        // 최대 4명을 수용 가능한 빈방을 생성
//        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
//    }


//    // 룸에 참가 완료된 경우 자동 실행
//    public override void OnJoinedRoom()
//    {
//        // 접속 상태 표시
//        connectionInfoText.text = "Joined";
//        // 모든 룸 참가자들이 Main 씬을 로드하게 함
//        PhotonNetwork.LoadLevel("Game");
//    }

//}

using Photon.Pun; // 유니티용 포톤 컴포넌트들
using Photon.Realtime; // 포톤 서비스 관련 라이브러리
using UnityEngine;
using UnityEngine.UI;

// 마스터(매치 메이킹) 서버와 룸 접속을 담당
public class LobbyManager : MonoBehaviourPunCallbacks
{
    PhotonView PV;
    
    private string gameVersion = "1"; // 게임 버전
    public Text connectionInfoText; // 네트워크 정보를 표시할 텍스트
    public Button joinButton; // 룸 접속 버튼

    // 게임 실행과 동시에 마스터 서버 접속 시도
    private void Start()
    {
        // 접속에 필요한 정보(게임 버전) 설정
        PhotonNetwork.GameVersion = gameVersion;
        Screen.SetResolution(800, 400, false);
        PV = photonView;
    }

    public void Connect(InputField NickInput)
    {
        if (string.IsNullOrWhiteSpace(NickInput.text)) return;
        PhotonNetwork.LocalPlayer.NickName = NickInput.text;

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        connectionInfoText.text = "Connected to Master - Online";
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.JoinOrCreateRoom("Room2", new RoomOptions { MaxPlayers = 4 }, null);
    }

    public override void OnJoinedRoom()
    {
        // 접속 상태 표시
        connectionInfoText.text = "Joined";
        // 모든 룸 참가자들이 Main 씬을 로드하게 함
        PhotonNetwork.LoadLevel("Game");
    }
}
