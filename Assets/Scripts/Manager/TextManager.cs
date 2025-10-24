using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextManager
{
    // 현재 대화 데이터
    private List<string> currentTexts = new List<string>();
    private List<int> currentSpeakers = new List<int>();  // TalkNum (화자 구분)
    private int currentTextCount = 0;

    // 스테이지 클리어 여부
    private bool isStageCleared;

    // UI 및 출력 제어
    private TMP_Text textPrintBox;
    private float textPrintSpeed = 0.03f;  // 텍스트 출력 속도 (0.1f → 0.05f로 2배 빠르게)
    private bool isPrint = false;
    private bool isSkipText = false;

    public bool isTalk;

    public TextManager()
    {
        isTalk = false;
        isPrint = false;
        isSkipText = false;
        currentTextCount = 0;
    }

    // 현재 스테이지 클리어 여부 확인
    private bool CheckStageCleared()
    {
        string key = GameManager.currentScenario + "-" + GameManager.currentStage;

        // PlayerPrefs 값의 의미:
        // - 0: 잠김
        // - 1: 해금됨 (플레이 가능, 스토리 스킵 불가)
        // - 2: 클리어 완료 (스토리 스킵 가능)
        return PlayerPrefs.GetInt(key, 0) == 2;
    }

    // 스토리 텍스트 선택 및 로드
    private bool SelectPrintText()
    {
        // 이미 로드된 텍스트가 있으면 그대로 사용
        if (currentTexts.Count > 0) return true;

        // 클리어 여부 확인
        isStageCleared = CheckStageCleared();

        // 스테이지 ID 생성
        string stageId = "SN_" + GameManager.currentScenario + "_ST_" + GameManager.currentStage;

        List<Dictionary<string, object>> storyData = null;

        // 시작 or 클리어에 따라 다른 파일 로드
        if (!GameManager.isClear)
        {
            // 시작 텍스트 로드
            storyData = CSVReader.Read(stageId + "_Start");
        }
        else
        {
            // 클리어 텍스트 로드
            storyData = CSVReader.Read(stageId + "_End");
        }

        // 데이터가 없거나 비어있으면 스토리 없음
        if (storyData == null || storyData.Count == 0)
        {
            return false;  // 스토리 없이 바로 진행
        }

        // 텍스트와 화자 정보 추출
        for (int i = 0; i < storyData.Count; i++)
        {
            currentTexts.Add(storyData[i]["Talk"].ToString());
            currentSpeakers.Add(int.Parse(storyData[i]["TalkNum"].ToString()));
        }

        return true;
    }

    // 대화 시작
    public bool StartTalk(bool isSkip = false, float TextPrintSpeed = 0.1f)
    {
        textPrintSpeed = TextPrintSpeed;

        // textPrintBox 초기화를 제일 먼저 실행 (ClearTextBox 호출 전에 필요)
        if (textPrintBox == null)
            textPrintBox = GameObject.Find("TalkText").GetComponent<TMP_Text>();

        if (isSkipText)
        {
            ClearTextBox();
            return false;
        }

        // 텍스트 선택 (없으면 false 반환)
        if (!SelectPrintText())
        {
            ClearTextBox();
            return false;
        }

        isTalk = true;

        if (isSkip)
        {
            textPrintBox.text = currentTexts[0].ToString();
            isSkipText = true;
            return false;
        }

        if (!isPrint)
            CoroutineHelper.StartCoroutine(printText());
        else
            textPrintSpeed = 0;  // 즉시 완성

        return true;
    }

    // 터치 입력 처리
    public void OnTouch()
    {
        if (!isTalk) return;  // 대화 중이 아니면 무시

        if (isPrint)  // 텍스트 출력 중
        {
            if (isStageCleared)  // 클리어한 스테이지만 즉시 완성
            {
                textPrintSpeed = 0;
            }
            // 클리어 안한 스테이지는 아무것도 안함 (끝까지 기다려야 함)
        }
        else  // 텍스트 출력 완료
        {
            // 다음 대사 또는 종료
            StartTalk();
        }
    }

    // 텍스트 박스 초기화
    public void ClearTextBox()
    {
        isSkipText = false;
        isTalk = false;
        if (textPrintBox != null) textPrintBox.text = "";

        // 대화 데이터 초기화 - 다음 스토리를 위해 반드시 필요
        currentTexts.Clear();
        currentSpeakers.Clear();
        currentTextCount = 0;

        if (GameManager.isClear)
        {
            GameManager.isClear = false;
            GameManager.StageClear();
        }
    }

    // 텍스트 출력 코루틴
    IEnumerator printText()
    {
        isPrint = true;
        textPrintBox.text = "";

        string text = currentTexts[currentTextCount].ToString();
        int speaker = currentSpeakers[currentTextCount];

        // TODO: 나중에 화자별 색상 처리
        // if (speaker == 0) textPrintBox.color = Color.white;
        // else if (speaker == 1) textPrintBox.color = Color.cyan;

        int charIndex = 0;

        while (charIndex < text.Length)
        {
            // 즉시 완성 모드 (textPrintSpeed가 0으로 변경됨)
            if (textPrintSpeed == 0)
            {
                textPrintBox.text = text;
                break;
            }

            textPrintBox.text += text[charIndex].ToString();
            charIndex++;

            yield return new WaitForSeconds(textPrintSpeed);
        }

        // 다음 대사로
        currentTextCount++;

        // 모든 대사 출력 완료
        if (currentTexts.Count == currentTextCount)
        {
            currentTexts.Clear();
            currentSpeakers.Clear();
            currentTextCount = 0;
            isSkipText = true;
        }

        isPrint = false;
        textPrintSpeed = 0.03f;  // 속도 리셋
    }
}
