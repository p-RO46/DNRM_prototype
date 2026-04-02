using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LampUI : MonoBehaviour
{
    [SerializeField] private List<GameObject> lamps;
    [SerializeField] private List<TextMeshProUGUI> names;

    [Serializable]
    public class NumberOfDrugAdmin
    {
        public string name;
        public int count;
    }

    [Serializable]
    public class Progress
    {
        public List<NumberOfDrugAdmin> number_of_drug_administered;
        public string current_conversation_partner;
        public int current_round;
    }

    [SerializeField] Color lampOn = new Color(1f, 0.85f, 0.02f, 1f);
    [SerializeField] Color lampOff = new Color(0.9f, 0.9f, 0.92f, 1f);

    public void LampStateUpdate()
    {
        // 1. 읽어오기
        string jsonString = Resources.Load<TextAsset>("progress").text;

        // 2. 역직렬화 (JSON 문자열 -> C# 객체)
        Progress progressData = JsonConvert.DeserializeObject<Progress>(jsonString);

        int lampNum = 0;

        // 3. 데이터 확인
        foreach (var number in progressData.number_of_drug_administered)
        {
            if(number.count == 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    Image lampImage = lamps[lampNum].transform.GetChild(i).GetComponent<Image>();
                    lampImage.color = lampOff;

                    names[lampNum].color = lampOff;
                }
            }
            else if(number.count > 0)
            {
                for(int i = 0; i< number.count; i++)
                {
                    Image lampImage = lamps[lampNum].transform.GetChild(i).GetComponent<Image>();
                    lampImage.color = lampOn;
                }
            }
            lampNum ++;
        }
    }
}
