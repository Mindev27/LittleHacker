using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class MakeJsonMapData : MonoBehaviour
{
    // CSV ����
    private string csvFilePath = "SN_1_ST_1"; // �����ʿ�
    // JSON ���� ���� ���
    private string jsonFilePath = "Assets/Resources/Map_data1.json"; // �����ʿ�

    void Start()
    {
        SaveMapDataToJson();
    }

    void SaveMapDataToJson()
    {
        // CSV ���� �б�
        List<Dictionary<string, object>> csvData = CSVReader.Read(csvFilePath);

        // JSON���� ��ȯ�� ��ü ����
        MapData mapData = new MapData();
        mapData.Walls = new List<List<int>>();
        mapData.Numbers = new List<List<string>>();
        mapData.Operators = new List<List<string>>();
        mapData.PlayerPosition = new Vector2();
        mapData.DoorPosition = new Vector2();
        mapData.DoorValue = new int();

        int rowIndex = 0;
        foreach (var row in csvData)
        {
            List<int> wallRow = new List<int>();
            List<string> numberRow = new List<string>();
            List<string> operatorRow = new List<string>();

            int colIndex = 0;
            foreach (var col in row)
            {
                string value = col.Value.ToString();
                if (value == "1")  // ��
                {
                    wallRow.Add(1);
                    numberRow.Add("");
                    operatorRow.Add("");
                }
                else if (value == "0")  // ���
                {
                    wallRow.Add(0);
                    numberRow.Add("");
                    operatorRow.Add("");
                }
                else if (value.StartsWith("3_"))  // ����
                {
                    wallRow.Add(0);
                    numberRow.Add(value.Split('_')[1]);
                    operatorRow.Add("");
                }
                else if (value.StartsWith("4_") || value.StartsWith("5_") || value.StartsWith("6_") || value.StartsWith("7_"))  // ������
                {
                    wallRow.Add(0);
                    numberRow.Add("");
                    operatorRow.Add(value.Split('_')[1]);
                }
                else if (value == "2")  // �÷��̾� ��ġ
                {
                    wallRow.Add(0);
                    numberRow.Add("");
                    operatorRow.Add("");
                    mapData.PlayerPosition = new Vector2(rowIndex, colIndex);
                }
                else if(value.StartsWith("8_"))   // �� ��ġ
                {
                    wallRow.Add(0);
                    numberRow.Add("");
                    operatorRow.Add("");
                    mapData.DoorPosition = new Vector2(rowIndex, colIndex);
                    mapData.DoorValue = int.Parse(value.Split('_')[1]);
                }
                colIndex++;
            }

            mapData.Walls.Add(wallRow);
            mapData.Numbers.Add(numberRow);
            mapData.Operators.Add(operatorRow);

            rowIndex++;
        }

        // Json���� Vector2�� ������� ������ ���ܼ� ������ �����������
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        // JSON ���Ϸ� ����
        string json = JsonConvert.SerializeObject(mapData, Formatting.Indented, settings);

        /* string json = JsonUtility.ToJson(mapData, true); �̰ž�������� ���� ����ȭ �ȵȴ��ؼ� �Ⱦ������Դϴ�
         * json������ ���ٷ� �� ��µǾ ���̻ڱ��ؿ�
         */

        File.WriteAllText(jsonFilePath, json);
    }
}

[System.Serializable]
public class MapData
{
    public List<List<int>> Walls { get; set; }
    public List<List<string>> Numbers { get; set; }
    public List<List<string>> Operators { get; set; }
    public Vector2 PlayerPosition;
    public Vector2 DoorPosition;
    public int DoorValue;
}