using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 시작 씬에서 모든 클리어 데이터를 초기화하는 스크립트
// 사용법: 이 스크립트를 시작 씬의 빈 GameObject에 붙이고 게임을 실행하면 자동으로 초기화됨
// 추후에 배포에서는 삭제할 것
public class ClearDataResetter : MonoBehaviour
{
    // 최대 시나리오와 스테이지 수 (GameManager와 동일하게 설정)
    private int maxScenario = 3;
    private int maxStage = 15;

    void Start()
    {
        ResetAllClearData();
    }

    // 모든 스테이지 클리어 상태 초기화
    private void ResetAllClearData()
    {
        // 모든 시나리오와 스테이지의 클리어 데이터 삭제
        for (int scenario = 1; scenario <= maxScenario; scenario++)
        {
            for (int stage = 1; stage <= maxStage; stage++)
            {
                string key = scenario + "-" + stage;
                if (PlayerPrefs.HasKey(key))
                {
                    PlayerPrefs.DeleteKey(key);
                }
            }
        }
        PlayerPrefs.Save(); // 즉시 저장
        Debug.Log("All clear data has been reset!");
    }
}
