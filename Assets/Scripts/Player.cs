using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Xml;
using UnityEngine.SceneManagement;
using LittleRay;

// Full map state snapshot for undo
public class revertObject
{
    // Player state
    public Vector2 playerPos;
    public int formulaCount;
    public Dictionary<int, ObjectData> formula = new Dictionary<int, ObjectData>();
    public int formulaTotalNum;

    // All numbers state (GameObject -> value)
    public Dictionary<GameObject, int> allNumbersState = new Dictionary<GameObject, int>();

    // All objects active state (GameObject -> active)
    public Dictionary<GameObject, bool> objectsActiveState = new Dictionary<GameObject, bool>();

    // Box positions
    public Dictionary<GameObject, Vector2> boxPositions = new Dictionary<GameObject, Vector2>();
}


public class Player : MonoBehaviour
{
    // ȭ�� ��ġ �Լ�
    public enum ETouchState { None, Begin, Move, End };
    public ETouchState playerTouch = ETouchState.None;
    private Vector2 touchPosition = new Vector2(0, 0);
    private Vector2 startPos = new Vector2(0, 0);
    public Vector2 moveDir = new Vector2(0, 0);
    private Vector2 calculMoveDir = new Vector2(0, 0);
    public List<Vector2> moveDirs = new List<Vector2>();

    // �ϴ� �ΰ��� Player ����
    public float playerMoveSpeed;
    private bool moveStart = false;
    private bool snapshotSavedThisTurn = false;  // 이번 턴에 스냅샷 저장 완료 여부

    // Undo system: stores map snapshots per turn
    public Dictionary<int, revertObject> backUpRevert = new Dictionary<int, revertObject>();

    // ���� ����
    public Dictionary<int, ObjectData> formula = new Dictionary<int, ObjectData>();
    public TMP_Text[] formulaUi = new TMP_Text[3];
    public int formulaTotalNum = 0;
    private int formulaCount = 0;

    public GameManager gameManager;

    public void Start()
    {
        InputGameObject();
        Initialized();
    }

    public void Update()
    {
        TouchSetup();
        // ��ȭ �������� ��� �÷��̾� �⹰ ������ ���߱� ������ �̵� o
        if (!Managers.Text.isTalk)
        {
            PlayerMoveDIr();
            MoveKeyBind();
        }
        
        // �ӽ� Ű ���ε� ���� ���� ����
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CheckFormula();
        }
    }

    private void FixedUpdate()
    {
        if (moveDirs.Count != 0) PlayerMove();
    }

    // �ʱ� ���۽� ������ �־��ֱ�
    private void InputGameObject()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        int count = 0;
        foreach (Transform formulaInfoUi in GameObject.Find("FormulaBackGround").transform)
        {
            formulaUi[count] = formulaInfoUi.GetComponent<TMP_Text>();
            count++;
        }

        GameObject.Find("BackStartButton").GetComponent<Button>().onClick.AddListener(() => BackStartButtonClick());
        GameObject.Find("ReStartButton").GetComponent<Button>().onClick.AddListener(() => ReStartButtonClick());
        GameObject.Find("HomeButton").GetComponent<Button>().onClick.AddListener(() => HomeButtonClick());
    }

    // �ʱ� �����Լ� ���������� ����ɶ����� ������� ����
    public void Initialized()
    {
        formula.Clear();
        formulaUi[0].text = "";
        formulaUi[1].text = "";
        formulaUi[2].text = "";
        formulaTotalNum = 0;
        backUpRevert.Clear();
        GameManager.playerTurn = 0;
        snapshotSavedThisTurn = false;  // 플래그 초기화
    }

    // ȭ�� ��ġ �Լ� ���콺 Ŭ���� ���� playerTouch ����
    void TouchSetup()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) { if (EventSystem.current.IsPointerOverGameObject() == false) { playerTouch = ETouchState.Begin; } }
        else if (Input.GetMouseButton(0)) { if (EventSystem.current.IsPointerOverGameObject() == false) { playerTouch = ETouchState.Move; } }
        else if (Input.GetMouseButtonUp(0)) { if (EventSystem.current.IsPointerOverGameObject() == false) { playerTouch = ETouchState.End; } }
        else playerTouch = ETouchState.None;
        touchPosition = Input.mousePosition;
        //Debug.Log(playerTouch);
#else
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId) == true) return;
            if (touch.phase == TouchPhase.Began) playerTouch = ETouchState.Begin;
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) playerTouch = ETouchState.Move;
            else if (touch.phase == TouchPhase.Ended) if (playerTouch != ETouchState.None) playerTouch = ETouchState.End;
            touchPosition = touch.position;
        }
        else playerTouch = ETouchState.None;
#endif
    }

    // Ű���� �׽�Ʈ ȯ�� �Լ� ���� ���� ����
    void MoveKeyBind()
    {
        if (playerTouch == ETouchState.None)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                moveStart = true;
                moveDirs.Add(new Vector2(0, 1));
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                moveStart = true;
                moveDirs.Add(new Vector2(-1, 0));
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                moveStart = true;
                moveDirs.Add(new Vector2(0, -1));
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                moveStart = true;
                moveDirs.Add(new Vector2(1, 0));
            }
        }
    }

    void PlayerMoveDIr()
    {
        if (playerTouch == ETouchState.Begin)
        {
            startPos = touchPosition;
        }

        else if (playerTouch == ETouchState.Move)
        {
            calculMoveDir = touchPosition - startPos;
            calculMoveDir = new Vector2(Mathf.Floor(calculMoveDir.x), Mathf.Floor(calculMoveDir.y));
        }

        else if (playerTouch == ETouchState.End)
        {
            if (new Vector2(Mathf.Floor(calculMoveDir.x), Mathf.Floor(calculMoveDir.y)) == new Vector2(0, 0)) return;

            else
            {
                if (Mathf.Abs(calculMoveDir.x) > Mathf.Abs(calculMoveDir.y)) calculMoveDir.y = 0;
                else calculMoveDir.x = 0;

                calculMoveDir.Normalize();
                moveDirs.Add(calculMoveDir);
                moveStart = true;
            }
        }
    }

    // player�� ������ �� ���� ���θ��� ������ ���� ���
    void PlayerMove()
    {
        // layerMask�� ���� ��ó���� ������ϴ� �ݰ� ���
        int layerMask = (1 << LayerMask.NameToLayer("Wall")) + (1 << LayerMask.NameToLayer("Item"));
        moveDir = moveDirs[0];

        // 이동 시작 시 스냅샷 저장 (딱 한 번만)
        if (moveStart == true && !snapshotSavedThisTurn)
        {
            SaveMapSnapshot();
            snapshotSavedThisTurn = true;  // 저장 완료 표시
        }

        PlayerGetItem();
        RaycastHit2D hitWall = Physics2D.Raycast(transform.position, moveDir, 0.6f, layerMask); // ��
        RaycastHit2D hitDoor = Physics2D.Raycast(transform.position, moveDir, 0.6f, LayerMask.GetMask("Door")); // ��
        RaycastHit2D hitGate = Physics2D.Raycast(transform.position, moveDir, 0.6f, LayerMask.GetMask("Gate")); // 게이트
        RaycastHit2D hitTrigger = Physics2D.Raycast(transform.position, moveDir, 0.6f, LayerMask.GetMask("Trigger")); // �ڽ�

        // player�� ������ �����̰� �ϴ� ��
        transform.Translate(moveDir * playerMoveSpeed * Time.deltaTime);

        // box�� �ε����� �� ����� ��ũ��Ʈ
        if (hitTrigger)
        {
            if (hitTrigger.transform.tag == "Box" && moveStart == true)
            {
                ObjectData box = hitTrigger.transform.GetComponent<ObjectData>();

                if (box.boxStop == true)
                {
                    moveStart = false;
                    transform.position = new Vector2(hitTrigger.transform.position.x - moveDir.x, hitTrigger.transform.position.y - moveDir.y);
                    box.boxStop = false;
                }
                else
                {
                    if (box.boxTrigger == false)
                    {
                        box.boxMoveDir = moveDir;
                        box.boxTrigger = true;
                    }
                }
            }
        }

        if (hitWall)
        {
            // �� ó��
            if (hitWall.transform.tag == "Wall" || hitWall.transform.tag == "Operator" && formulaCount % 3 != 1 || hitWall.transform.tag == "Number" && formulaCount % 3 == 1)
            {
                moveStart = false;
                transform.position = new Vector2(hitWall.transform.position.x - moveDir.x, hitWall.transform.position.y - moveDir.y);
            }
        }

        // ���������� �������� �� ������ �������� �ʾ��� ���
        if (hitDoor)
        {
            if (hitDoor.transform.GetComponent<ObjectData>().num != formulaTotalNum || formulaCount % 3 != 1)
            {
                moveStart = false;
                transform.position = new Vector2(hitDoor.transform.position.x - moveDir.x, hitDoor.transform.position.y - moveDir.y);
            }
        }

        // 게이트 처리 - 조건 만족 시 통과, 불만족 시 막힘
        if (hitGate)
        {
            // 현재 숫자가 게이트 숫자와 일치하지 않거나 계산이 완료되지 않았으면 막힘
            if (hitGate.transform.GetComponent<ObjectData>().num != formulaTotalNum || formulaCount % 3 != 1)
            {
                moveStart = false;
                transform.position = new Vector2(hitGate.transform.position.x - moveDir.x, hitGate.transform.position.y - moveDir.y);
            }
            // 조건 만족 시 통과 (아무것도 안 함)
        }

        // Turn ended
        if (moveStart == false)
        {
            GameManager.playerTurn++;
            moveStart = true;
            snapshotSavedThisTurn = false;  // 다음 턴을 위해 플래그 리셋
            moveDirs.RemoveAt(0);

            // 모든 Gate 스프라이트 업데이트
            UpdateGateSprites();
        }

    }

    // player�� ���ĵ��� ����� ��� �Ǵ� ���� ����� ��츦 ������ ��� ���ĵ鸸 �־�δ� ������
    void PlayerGetItem()
    {
        int layerMask = (1 << LayerMask.NameToLayer("Item")) + (1 << LayerMask.NameToLayer("Door"));
        RaycastHit2D hitItem = Physics2D.Raycast(transform.position, moveDir, 0.3f, layerMask);

        if (hitItem)
        {
            ObjectData OD = hitItem.transform.GetComponent<ObjectData>();

            // AllOperator 처리 (맵의 모든 숫자에 연산 적용)
            if (hitItem.transform.tag == "AllOperator")
            {
                ApplyAllOperator(OD.oper, OD.num);
                hitItem.transform.gameObject.SetActive(false);
                return;
            }

            // Number item
            else if (hitItem.transform.tag == "Number" && formulaCount % 3 == 0 || formulaCount % 3 == 2)
            {
                formula.Add(formulaCount, OD);
                formulaUi[formulaCount % 3].text = "" + OD.num;
                if (formulaCount % 3 == 0) formulaTotalNum = formula[formulaCount].num;
                formulaCount++;
                hitItem.transform.gameObject.SetActive(false);
            }

            // Operator item
            else if (hitItem.transform.tag == "Operator" && formulaCount % 3 == 1)
            {
                formula.Add(formulaCount, OD);
                formulaUi[1].text = OD.oper;
                formulaCount++;
                hitItem.transform.gameObject.SetActive(false);
            }

            else if(hitItem.transform.tag == "Door" && formulaCount % 3 == 1)
            {
                if(hitItem.transform.GetComponent<ObjectData>().num == formulaTotalNum)
                {
                    // stageClear
                    GameManager.isClear = true;
                    Managers.Text.StartTalk();
                    Destroy(hitItem.transform.gameObject);
                }
                else
                {
                    return;
                }
            }

            // Trap 처리 - 밟으면 스테이지 재시작
            else if(hitItem.transform.tag == "Trap")
            {
                gameManager.GetComponent<MapCreate>().RestartStage();
                return;
            }

            // ���Ķ��� ��� ĭ�� �� ä���� �ִ� ��� ����
            if(formulaCount % 3 == 0)
            {
                PlayerCalculate();
            }
        }
        else
        {
            return;
        }
    }

    // ���� ��� ���� + ������ + ���� ������ ������ ������ �� ������ִ� �Լ�
    void PlayerCalculate()
    {
        formula.Add(formulaCount, gameObject.AddComponent<ObjectData>());
        switch (formula[formulaCount - 2].oper)
        {
            case "-":
                formula[formulaCount].num = formula[formulaCount - 3].num - formula[formulaCount - 1].num;
                break;
            case "+":
                formula[formulaCount].num = formula[formulaCount - 3].num + formula[formulaCount - 1].num;
                break;
            case "/":
                formula[formulaCount].num = formula[formulaCount - 3].num / formula[formulaCount - 1].num;
                break;
            case "*":
                formula[formulaCount].num = formula[formulaCount - 3].num * formula[formulaCount - 1].num;
                break;
            default:
                Debug.LogError("Playe.cs ���� �� PlayerCalculate ���� �ش� ������ ����");
                break;
        }
        // ���� �ʱ�ȭ
        formulaUi[0].text = "" + formula[formulaCount].num;
        formulaUi[1].text = "";
        formulaUi[2].text = "";

        formulaTotalNum = formula[formulaCount].num;
        formulaCount++;
    }

    // Debug formula check
    void CheckFormula()
    {
        for(int count = 0; count < formulaCount; count++)
        {
            if(count % 2 == 0) Debug.Log("iter count " + count + " : " + formula[count].num);
            else if (count % 2 == 1) Debug.Log("iter count " + count + " : " + formula[count].oper);
        }
    }

    // Save full map state snapshot
    void SaveMapSnapshot()
    {
        revertObject snapshot = new revertObject();

        // Save player state
        snapshot.playerPos = transform.position;
        snapshot.formulaCount = this.formulaCount;
        snapshot.formula = new Dictionary<int, ObjectData>(this.formula);
        snapshot.formulaTotalNum = this.formulaTotalNum;

        // Save all numbers state
        GameObject[] allNumbers = GameObject.FindGameObjectsWithTag("Number");
        foreach (GameObject numberObj in allNumbers)
        {
            ObjectData data = numberObj.GetComponent<ObjectData>();
            snapshot.allNumbersState[numberObj] = data.num;
            snapshot.objectsActiveState[numberObj] = numberObj.activeSelf;
        }

        // Save all operators state
        GameObject[] allOperators = GameObject.FindGameObjectsWithTag("Operator");
        foreach (GameObject opObj in allOperators)
        {
            snapshot.objectsActiveState[opObj] = opObj.activeSelf;
        }

        // Save all AllOperators state
        GameObject[] allAllOperators = GameObject.FindGameObjectsWithTag("AllOperator");
        foreach (GameObject allOpObj in allAllOperators)
        {
            snapshot.objectsActiveState[allOpObj] = allOpObj.activeSelf;
        }

        // Save box positions
        GameObject[] allBoxes = GameObject.FindGameObjectsWithTag("Box");
        foreach (GameObject boxObj in allBoxes)
        {
            snapshot.boxPositions[boxObj] = boxObj.transform.position;
        }

        backUpRevert[GameManager.playerTurn] = snapshot;
        Debug.Log($"Map snapshot saved for turn {GameManager.playerTurn}");
    }

    // all operator 연산 적용 함수
    void ApplyAllOperator(string oper, int value)
    {
        // 씬에 있는 모든 숫자 아이템 찾기
        GameObject[] allNumbers = GameObject.FindGameObjectsWithTag("Number");

        int affectedCount = 0;
        foreach (GameObject numberObj in allNumbers)
        {
            // 이미 수집된 아이템(비활성화된 것)은 연산 적용하지 않음
            if (!numberObj.activeSelf) continue;

            ObjectData numberData = numberObj.GetComponent<ObjectData>();
            int originalValue = numberData.num;

            // 연산자에 따라 계산
            switch (oper)
            {
                case "+":
                    numberData.num += value;
                    break;
                case "-":
                    numberData.num -= value;
                    break;
                case "*":
                    numberData.num *= value;
                    break;
                case "/":
                    if (value != 0)
                        numberData.num /= value;
                    else
                        Debug.LogWarning("AllOperator: Division by zero");
                    break;
                default:
                    Debug.LogError($"AllOperator: Unknown operator '{oper}'");
                    break;
            }

            // UI 텍스트 업데이트
            TMP_Text numberText = numberObj.transform.GetChild(0).GetComponent<TMP_Text>();
            numberText.text = numberData.num.ToString();

            affectedCount++;
            Debug.Log($"AllOperator: Changed number from {originalValue} to {numberData.num}");
        }

        Debug.Log($"AllOperator {oper}{value} applied to {affectedCount} numbers");
    }

    void HomeButtonClick()
    {
        SceneManager.LoadScene(1);
    }

    // �� �ٽ� ���ε� Initialize �����ؾ���
    void ReStartButtonClick()
    {
        GameObject.Find("GameManager").GetComponent<MapCreate>().Initialize("SN_" + GameManager.currentScenario.ToString() + "_ST_" + GameManager.currentStage.ToString());
        Initialized();
    }

    // Restore full map state from snapshot
    void BackStartButtonClick()
    {
        // 이동 중일 때는 Undo 불가 (완전히 멈췄을 때만 가능)
        if (moveDirs.Count > 0)
        {
            Debug.LogWarning("Cannot undo while moving!");
            return;
        }

        if(backUpRevert.Count == 0 || GameManager.playerTurn == 0) return;

        // Safety check: ensure snapshot exists
        if (!backUpRevert.ContainsKey(GameManager.playerTurn - 1))
        {
            Debug.LogWarning($"Undo failed: No snapshot for turn {GameManager.playerTurn - 1}");
            return;
        }

        revertObject snapshot = backUpRevert[GameManager.playerTurn - 1];

        // Restore player state
        transform.position = snapshot.playerPos;
        this.formulaCount = snapshot.formulaCount;
        this.formula = new Dictionary<int, ObjectData>(snapshot.formula);
        this.formulaTotalNum = snapshot.formulaTotalNum;

        // Restore formula UI - show current formula in progress
        int currentFormulaBase = (snapshot.formulaCount / 3) * 3;  // Current formula start index
        for(int i = 0; i < 3; i++)
        {
            int formulaIndex = currentFormulaBase + i;
            if(formulaIndex < snapshot.formulaCount && snapshot.formula.ContainsKey(formulaIndex))
            {
                if(i == 1)
                    formulaUi[i].text = snapshot.formula[formulaIndex].oper;
                else
                    formulaUi[i].text = snapshot.formula[formulaIndex].num.ToString();
            }
            else
            {
                formulaUi[i].text = "";
            }
        }

        // Restore all numbers
        foreach(var pair in snapshot.allNumbersState)
        {
            GameObject numberObj = pair.Key;
            int value = pair.Value;
            if(numberObj != null)
            {
                numberObj.GetComponent<ObjectData>().num = value;
                numberObj.transform.GetChild(0).GetComponent<TMP_Text>().text = value.ToString();
            }
        }

        // Restore all objects active state
        foreach(var pair in snapshot.objectsActiveState)
        {
            GameObject obj = pair.Key;
            bool active = pair.Value;
            if(obj != null)
            {
                obj.SetActive(active);
            }
        }

        // Restore box positions
        foreach(var pair in snapshot.boxPositions)
        {
            GameObject boxObj = pair.Key;
            Vector2 pos = pair.Value;
            if(boxObj != null)
            {
                boxObj.transform.position = pos;
            }
        }

        GameManager.playerTurn--;
        backUpRevert.Remove(GameManager.playerTurn);

        // Clear remaining move commands to prevent unintended actions
        moveDirs.Clear();
        moveStart = false;

        Debug.Log($"Map restored to turn {GameManager.playerTurn}");
    }

    // 모든 Gate의 스프라이트를 현재 formulaTotalNum에 맞게 업데이트
    void UpdateGateSprites()
    {
        // 씬에 있는 모든 Gate 찾기
        GameObject[] allGates = GameObject.FindGameObjectsWithTag("Gate");

        foreach (GameObject gateObj in allGates)
        {
            ObjectData gateData = gateObj.GetComponent<ObjectData>();
            if (gateData == null) continue;

            // 조건 확인: 현재 숫자가 게이트 숫자와 일치하고 계산이 완료되었는지
            bool isOpen = (gateData.num == formulaTotalNum && formulaCount % 3 == 1);

            // 스프라이트 변경
            gateData.SetGateOpen(isOpen);
        }
    }
}