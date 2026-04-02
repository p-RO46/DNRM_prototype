using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI simulation;
    [SerializeField] private TextMeshProUGUI summarization;
    [SerializeField] private TextMeshProUGUI somethingelse;

    void OnEnable()
    {
        StartCoroutine(LoadingAnim());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator LoadingAnim()
    {
        int times = 1;
        string simul = "SIMULATION";
        string summ = "SUMMARIZATION";
        string somee = "SOMETHING ELSE";

        simulation.text = simul;
        summarization.text = summ;
        somethingelse.text = somee;

        while (true)
        {
            if (times >= 4)
            {
                times = 0;
                simulation.text = simul;
                summarization.text = summ;
                somethingelse.text = somee;
            }

            if(times != 0)
            {
                simulation.text = simulation.text + ".";
                summarization.text = summarization.text + ".";
                somethingelse.text = somethingelse.text + ".";
            }

            times++;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
