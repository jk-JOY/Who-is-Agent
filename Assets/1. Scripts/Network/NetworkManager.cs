using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static UIManager;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEngine.VFX;
using UnityEngineInternal;
using System.Runtime.InteropServices;

public enum PlayerType
{
    SPY,
}
public class NetworkManager : MonoBehaviourPunCallbacks
{
    PhotonView PV;

    public static NetworkManager NM;

    public static NetworkManager instance
    {
        get
        {
            // ���� �̱��� ������ ���� ������Ʈ�� �Ҵ���� �ʾҴٸ�
            if (m_instance == null)
            {
                // ������ GameManager ������Ʈ�� ã�� �Ҵ�
                m_instance = FindObjectOfType<NetworkManager>();
                //m_instance.Initialize();
            }

            // �̱��� ������Ʈ�� ��ȯ
            return m_instance;
        }
    }

    private static NetworkManager m_instance; // �̱����� �Ҵ�� static ����


    public void Awake() => NM = this;

    public List<Player> Players = new List<Player>();
    public Player MyPlayer;

    public PlayerType playerType;

    public bool isWaitingRoom = false;
    public bool isGameStart = false;
    public bool isResiWin;

    [Header("SpawnPoint")]
    public Transform mainSpawnPoint;

    public float baseTime;
    private float selectCountdown;

    public Text timeText;

    [Header("Job-Information")]
    //���� ���� �г�
    public GameObject infoPanel;
    
    [Header("InGamePanel")]
    public GameObject gamePanel;
    //�������� ���� Text
    public GameObject Resi_InfoText, Spy_InfoText;
    [Header("Win Panel")]
    //�¸� �г�
    public GameObject Resi_WinPanel, SPY_WinPanel;


    private void Start()
    {
        PV = photonView;

        MyPlayer = PhotonNetwork.Instantiate("Player", new Vector2(Random.Range(-5f,0f),Random.Range(-37f, -41f)), Quaternion.identity).GetComponent<Player>();
        SetRandColor();
        isWaitingRoom = true;
    }

    public void SetRandColor()
    {
        List<int> PlayerColors = new List<int>();
        for (int i = 0; i < Players.Count; i++)
            PlayerColors.Add(Players[i].colorIndex);

        while (true)
        {
            int rand = Random.Range(0, 5);
            if (!PlayerColors.Contains(rand))
            {
                MyPlayer.GetComponent<PhotonView>().RPC("SetColor", RpcTarget.AllBuffered, rand);
                break;
            }
        }
    }

    public void SortPlayers() => Players.Sort((p1, p2) => p1.actor.CompareTo(p2.actor));

    public Color GetColor(int colorIndex)
    {
        return UM.colors[colorIndex];
    }

    //�г��� �����ϴ� ShowPanel
    public void ShowPanel(GameObject curPanel)
    {
        infoPanel.SetActive(false);
        gamePanel.SetActive(false);

        Resi_WinPanel.SetActive(false);
        SPY_WinPanel.SetActive(false);

        curPanel.SetActive(true);
    }

    //���� �����ϴ� ShowMap
    public void ShowMap(GameObject curMap)
    {
        //WaitingMap.SetActive(false);
        //MainMap.SetActive(false);

        curMap.SetActive(true);
    }

    public void GameStart()
    {
        // ������ ���ӽ���
        SetSPY();
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        //�� ��Ÿ�� NULL?? ��ŸƮ�� �ȵǴ°�....
        PV.RPC("GameStartRPC", RpcTarget.AllViaServer);
    }


    void SetSPY()
    {
        List<Player> GachaList = new List<Player>(Players);

        if (playerType == PlayerType.SPY)
        {
            for (int i = 0; i < 1; i++) //  ������ 1��
            {
                int rand = Random.Range(0, GachaList.Count); // �÷��̾� ���� ����� ����
                Players[rand].GetComponent<PhotonView>().RPC("SetSpy", RpcTarget.AllViaServer, true);
                GachaList.RemoveAt(rand);
            }
        }
        //���߿� ������ 1�� �� �߰� �� �÷��̾���� �����Ӱ� ������ �� �ֵ��� ����
    }

    //------------------------------------------------------------------------------------------------------------------
    [PunRPC]
    void GameStartRPC()
    {
        StartCoroutine(GameStartCo());
    }


    IEnumerator GameStartCo()
    {
        ShowPanel(infoPanel);
        //ShowMap(MainMap);
        //WaitingMap.SetActive(false);

        if (MyPlayer.isSpy)
        {
            Spy_InfoText.SetActive(true);
        }
        else
        {
            Resi_InfoText.SetActive(true);
        }

        yield return new WaitForSeconds(3);
        isWaitingRoom = false;
        isGameStart = true; //���� ����

        MyPlayer.SetPos(mainSpawnPoint.position);

        MyPlayer.SetNickColor(); //������ �г��� ���� ����

        ShowPanel(gamePanel); //�����гξȿ� ����ִ� ��� HUD�� ����. 

        //���� �ȿ� ���� �κ��丮��   �ش� ������ �°� �̼� ������ �����ն��� �̰��� �����Ѵ�. 
        //StartCoroutine(LightCheckCo());  //�� ����

        selectCountdown = baseTime;
    }

    private void Update()
    {
        PlayTime();
    }

    //�� �÷��� Ÿ���� 600sec = 10min �̴�. 
    public void PlayTime()
    {
        if (Mathf.Floor(selectCountdown) <= 0)
        {
            Winner(false);
            // Count 0�϶� ������ �Լ� ����
        }
        else
        {
            selectCountdown -= Time.deltaTime;
            timeText.text = Mathf.Floor(selectCountdown).ToString();
        }
    }


    public int GetCrewCount()
    {
        int crewCount = 0;
        for (int i = 0; i < Players.Count; i++)
            if (!Players[i].isSpy) ++crewCount;
        return crewCount;
    }

    [PunRPC]
    public void WinCheck()
    {
        int crewCount = 0;
        int impoCount = 0;

        for (int i = 0; i < Players.Count; i++)
        {
            var Player = Players[i];
            if (Players[i].isDie) continue;
            if (Player.isSpy)
                ++impoCount;
            else
                ++crewCount;
        }

        if (impoCount == 0 && crewCount > 0) // ��� ������ ����
            Winner(true);
        else if (impoCount != 0 && impoCount > crewCount) // ������ ũ�纸�� ����
            Winner(false);
    }

    public void Winner(bool isCrewWin)
    {
        if (!isGameStart) return;

        if (isCrewWin)
        {
            print("���������� �¸�");
            ShowPanel(Resi_WinPanel);
            Invoke("WinnerDelay", 3);
        }
        else
        {
            print("������ �¸�");
            ShowPanel(SPY_WinPanel);
            Invoke("WinnerDelay", 3);
        }
    }


    void WinnerDelay()
    {
        Application.Quit();
    }
}




