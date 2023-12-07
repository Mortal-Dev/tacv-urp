using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    public TMP_Text fpsText;

    public float updateInterval;

    private float timePast;

    private readonly List<float> pastTimesteps = new List<float>();

    private void Start()
    {
        fpsText = GetComponent<TMP_Text>();

        Application.targetFrameRate = 72;
        QualitySettings.vSyncCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (timePast < updateInterval)
        {
            pastTimesteps.Add(Time.deltaTime);

            timePast += Time.deltaTime;

            return;
        }

        timePast = 0;

        float pastTimestepsTotal = 0f;

        for (int i = 0; i < pastTimesteps.Count; i++)
        {
            pastTimestepsTotal += pastTimesteps[i];
        }

        pastTimestepsTotal /= pastTimesteps.Count;

        fpsText.text = ((int)(1f / pastTimestepsTotal)).ToString();

        pastTimesteps.Clear();
    }
}
