using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class MakeJsonMapData : MonoBehaviour
{   
    private string csvDirectoryPath;     // CSV ���ϵ��� �ִ� ���� ���
    private string jsonDirectoryPath;    // JSON ���ϵ��� ������ ���� ���

    void Start()
    {
        csvDirectoryPath = "Assets/Resources/MapDatasCSV";
        jsonDirectoryPath = "Assets/Resources/MapDatasJSON";

        InitializeJsonData(); // �����ϴ� Json ���ϵ� ����� (�ʿ��ϸ� �ּ� ó��)

        // ���丮���� ��� CSV ���� ����� ��������
        string[] csvFiles = Directory.GetFiles(csvDirectoryPath, "*.csv");

        foreach (string csvFile in csvFiles)
        {
            // Debug.Log("csvFile: " + csvFile);

            // ������ ��ü ��ο��� ���� �̸��� ���� (Ȯ���� ����)
            string fileName = Path.GetFileNameWithoutExtension(csvFile);

            // Debug.Log("CSV File Name: " + fileName);

            // JSON ������ ������ ��� ����
            string jsonFilePath = jsonDirectoryPath + "/" + fileName + ".json";

            // Debug.Log("JSON File Name: " + jsonFilePath);

            SaveMapDataToJson(fileName, jsonFilePath);
        }
    }

    // JSON ���ϵ��� ����� ������ ��� ������ �����
    void InitializeJsonData()
    {
        
        if (Directory.Exists(jsonDirectoryPath))
        {
            string[] files = Directory.GetFiles(jsonDirectoryPath);
            foreach (string file in files)
            {
                File.Delete(file);
                Debug.Log("DoneFileDelete : " + file); // ���°� ������ Ȯ���ϴ� �ڵ�
            }
        }
    }

    void SaveMapDataToJson(string csvFilePath, string jsonFilePath)
    {
        // CSV ���� �б�
        List<Dictionary<string, object>> csvData = CSVReader.Read(csvFilePath);

        // JSON���� ��ȯ�� ��ü ����
        MapData mapData = new MapData();
        mapData.Walls = new List<List<int>>();
        mapData.Numbers = new List<List<string>>();
        mapData.Operators = new List<List<string>>();
        mapData.Boxes = new List<List<int>>();
        mapData.AllOperators = new List<List<string>>();
        mapData.Traps = new List<List<int>>();
        mapData.Gates = new List<List<string>>();
        mapData.PlayerPosition = new Vector2();
        mapData.DoorPosition = new Vector2();
        mapData.DoorValue = new int();

        int rowIndex = 0;
        foreach (var row in csvData)
        {
            List<int> wallRow = new List<int>();
            List<string> numberRow = new List<string>();
            List<string> operatorRow = new List<string>();
            List<int> boxRow = new List<int>();
            List<string> allOperatorRow = new List<string>();
            List<int> trapRow = new List<int>();
            List<string> gateRow = new List<string>();

            int colIndex = 0;
            foreach (var col in row)
            {
                string value = col.Value.ToString();
                if (value == "1")  // ��
                {
                    wallRow.Add(1);
                    numberRow.Add("");
                    operatorRow.Add("");
                    boxRow.Add(0);
                    trapRow.Add(0);
                    gateRow.Add("");
                    allOperatorRow.Add("");
                }
                else if (value == "0")  // ���
                {
                    wallRow.Add(0);
                    numberRow.Add("");
                    operatorRow.Add("");
                    boxRow.Add(0);
                    trapRow.Add(0);
                    gateRow.Add("");
                    allOperatorRow.Add("");
                }
                else if (value.StartsWith("3_"))  // ����
                {
                    wallRow.Add(0);
                    numberRow.Add(value.Split('_')[1]);
                    operatorRow.Add("");
                    boxRow.Add(0);
                    trapRow.Add(0);
                    gateRow.Add("");
                    allOperatorRow.Add("");
                }
                else if (value.StartsWith("4_") || value.StartsWith("5_") || value.StartsWith("6_") || value.StartsWith("7_"))  // ������
                {
                    wallRow.Add(0);
                    numberRow.Add("");
                    operatorRow.Add(value.Split('_')[1]);
                    boxRow.Add(0);
                    trapRow.Add(0);
                    gateRow.Add("");
                    allOperatorRow.Add("");
                }
                else if (value == "2")  // �÷��̾� ��ġ
                {
                    wallRow.Add(0);
                    numberRow.Add("");
                    operatorRow.Add("");
                    boxRow.Add(0);
                    trapRow.Add(0);
                    gateRow.Add("");
                    allOperatorRow.Add("");
                    mapData.PlayerPosition = new Vector2(colIndex, rowIndex);
                }
                else if(value.StartsWith("8_"))   // �� ��ġ
                {
                    wallRow.Add(0);
                    numberRow.Add("");
                    operatorRow.Add("");
                    boxRow.Add(0);
                    trapRow.Add(0);
                    gateRow.Add("");
                    allOperatorRow.Add("");
                    mapData.DoorPosition = new Vector2(colIndex, rowIndex);
                    mapData.DoorValue = int.Parse(value.Split('_')[1]);
                }
                else if(value.StartsWith("9_"))     // ������ ��ġ
                {
                    wallRow.Add(0);
                    numberRow.Add("");
                    operatorRow.Add("");
                    boxRow.Add(1);
                    trapRow.Add(0);
                    gateRow.Add("");
                    allOperatorRow.Add("");
                }
                else if (value == "T")     // Ʈ���� ��ġ
                {
                    wallRow.Add(0);
                    numberRow.Add("");
                    operatorRow.Add("");
                    boxRow.Add(0);
                    trapRow.Add(1);
                    gateRow.Add("");
                    allOperatorRow.Add("");
                }
                else if(value.StartsWith("G_"))     // ����Ʈ�� ��ġ
                {
                    wallRow.Add(0);
                    numberRow.Add("");
                    operatorRow.Add("");
                    boxRow.Add(0);
                    trapRow.Add(0);
                    gateRow.Add(value.Split('_')[1]);
                    allOperatorRow.Add("");
                }
                else if(value.StartsWith("A_"))
                {
                    wallRow.Add(0);
                    numberRow.Add("");
                    operatorRow.Add("");
                    boxRow.Add(0);
                    trapRow.Add(0);
                    gateRow.Add("");
                    allOperatorRow.Add((value.Split('_')[1] + value.Split('_')[2]));
                }
                colIndex++;
            }

            mapData.Walls.Add(wallRow);
            mapData.Numbers.Add(numberRow);
            mapData.Operators.Add(operatorRow);
            mapData.Boxes.Add(boxRow);
            mapData.Traps.Add(trapRow);
            mapData.Gates.Add(gateRow);
            mapData.AllOperators.Add(allOperatorRow);

            rowIndex++;
        }

        // Json���� Vector2�� ������� ������ ���ܼ� ������ �����������
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        // JSON ���Ϸ� ����
        string json = JsonConvert.SerializeObject(mapData, Formatting.Indented, settings);

        /* string json = JsonUtility.ToJson(mapData, true); �̰ž��������� ���� ����ȭ �ȵȴ��ؼ� �Ⱦ������Դϴ�
         * json������ ���ٷ� �� ��µǾ ���̻ڱ��ؿ�
         */

        File.WriteAllText(jsonFilePath, json);

        Debug.Log("DoneFileWrite : " + jsonFilePath); // ���°� ������ Ȯ���ϴ� �ڵ�
    }
}

[System.Serializable]
public class MapData
{
    public List<List<int>> Walls { get; set; }
    public List<List<string>> Numbers { get; set; }
    public List<List<string>> Operators { get; set; }
    public List<List<int>> Boxes { get; set; }
    public List<List<string>> AllOperators { get; set; }
    public List<List<int>> Traps { get; set; }
    public List<List<string>> Gates { get; set; }
    public Vector2 PlayerPosition;
    public Vector2 DoorPosition;
    public int DoorValue;
}