using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectData : MonoBehaviour
{
    public int num;
    public int doorNum;
    public string oper;

    public bool boxTrigger = false;
    public bool boxStop = false;
    public Vector2 boxMoveDir = new Vector2(0, 0);
    private float boxMoveSpeed = 5;

    // Gate 스프라이트 (열림/닫힘)
    public Sprite gateClosedSprite;  // 닫힌 게이트 스프라이트
    public Sprite gateOpenSprite;    // 열린 게이트 스프라이트
    private SpriteRenderer spriteRenderer;

    private int tmpTurn = 0;

    public void Start()
    {
        tmpTurn = GameManager.playerTurn;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Gate 스프라이트를 열림/닫힘 상태로 변경
    public void SetGateOpen(bool isOpen)
    {
        if (spriteRenderer == null || gateClosedSprite == null || gateOpenSprite == null)
            return;

        spriteRenderer.sprite = isOpen ? gateOpenSprite : gateClosedSprite;
    }

    public void Update()
    {
        // ����
        if (tmpTurn != GameManager.playerTurn && !boxTrigger)
        {
            ResetStart();
            tmpTurn = GameManager.playerTurn;
        }
    }

    public void FixedUpdate()
    {
        if (boxTrigger)
        {
            BoxMove();
        }
    }

    void ResetStart()
    {
        boxStop = false;
        boxTrigger = false;
    }

    public void BoxMove()
    {
        RaycastHit2D hitWall = Physics2D.Raycast(transform.position, boxMoveDir, 0.6f, LayerMask.GetMask("Wall"));

        transform.Translate(boxMoveDir * boxMoveSpeed * Time.deltaTime);
        if (hitWall)
        {
            Debug.Log(hitWall);
            // �ڽ��� ���� �ε����� ���
            transform.position = new Vector2(hitWall.transform.position.x - boxMoveDir.x, hitWall.transform.position.y - boxMoveDir.y);
            boxStop = true;
            boxTrigger = false;
            boxMoveDir = Vector2.zero;
        }
    }
}
