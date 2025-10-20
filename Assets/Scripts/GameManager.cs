using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Util;

public class GameManager : MonoBehaviour
{
    // ���� �����ϰ� �ִ� �������� �� �ó����� ����
    static public int currentScenario = 1;
    static public int currentStage = 1;
    static public int maxScenario = 3;
    static public int maxStaage = 15;
    static public int playerTurn = 0;
    static public float gridSize = 1;
    static public bool isClear = false;

    public GameObject Canvas; // �÷��̾� ȭ�麸�� �Ʒ��� ������ �Ǵ� UI
    public GameObject UpperCanvas; // �÷��̾� ȭ�麸�� ���� ������ �Ǵ� UI
    public GameObject UiCamera;

    public static Player player;
    public static MapCreate mapCreate;

    private TouchUtil.ETouchState touchState;
    private Vector2 touchPos;

    private void Awake()
    {
        currentScenario = 1;
        currentStage = 1;
        SpawnInitialized();
    }

    private void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        mapCreate = transform.GetComponent<MapCreate>();
        isClear = false;
    }

    private void Update()
    {
        TouchUtil.TouchSetUp(ref touchState, ref touchPos);

        if (Managers.Text.isTalk)
        {
            if(touchState == TouchUtil.ETouchState.Ended)
            {
                Managers.Text.OnTouch();
            }
        }
    }

    void SpawnInitialized()
    {
        Canvas canvas = Instantiate(Canvas).GetComponent<Canvas>();
        Instantiate(UpperCanvas).GetComponent<Canvas>().worldCamera = Camera.main;
        canvas.worldCamera = Instantiate(UiCamera).GetComponent<Camera>();
    }

    // ���� ��ũ��Ʈ Initialized �Լ��� ��Ƽ� �����ų����
    public static void StageClear()
    {
        isClear = false;
        currentStage++;
        PlayerPrefs.SetInt("" + currentScenario.ToString() + "-" + currentStage.ToString(), 1);
        mapCreate.Initialize("SN_" + currentScenario.ToString() + "_ST_" + currentStage.ToString());
        Managers.Text.StartTalk();
    }
}
