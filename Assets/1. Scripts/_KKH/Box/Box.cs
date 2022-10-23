using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Box : MonoBehaviour
{
    public int boxNum = 0;

    private Inventory inven;

    private BoxUI boxInvenUI;
    public BoxUI BoxInvenUI => boxInvenUI;

    private PhotonView pv = null;

    public int ItemID = 999999;

    private const int boxSize = 3;

    private void Awake()
    {
        boxInvenUI = transform.GetComponentInChildren<BoxUI>();
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        inven = new Inventory(SlotType.Box, boxSize);
        boxInvenUI.InitializeInventory(inven, boxNum);
    }

    public void CreateItem()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (ItemID == 999999) return;

        pv.RPC("PunAddItem", RpcTarget.AllBuffered, (ItemIDCode)ItemID);
    }

    /// <summary>
    /// RPC�� ���� �ڽ��� �������� �߰�
    /// </summary>
    /// <param name="itemIDCode">�߰��� ������ ���̵�</param>
    [PunRPC]
    private void PunAddItem(ItemIDCode itemIDCode)
    {
        if (inven.AddItem(itemIDCode))
        {
            LogManager.Log($"{itemIDCode}������ ����");
        }
    }
}