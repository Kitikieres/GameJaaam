using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] private Slider slider;
    [SerializeField] float initialTime = 10f;
    [SerializeField] float initialTime2 = 0f;
    float remainingTime;
    float secondTimerTime;
    bool secondTimerActive = false;

    void Start()
    {
        remainingTime = initialTime;
       
    }

    void Update()
    {
        if (!secondTimerActive)
        {
           
            remainingTime -= Time.deltaTime;
            if (remainingTime < 0) remainingTime = 0;

            timerText.text = Mathf.FloorToInt(remainingTime).ToString("00");
            slider.maxValue = remainingTime;
        }
        else
        {
            secondTimerTime -= Time.deltaTime;
            if (secondTimerTime < 0) secondTimerTime = 0;

            timerText.text = Mathf.FloorToInt(secondTimerTime).ToString("00");
        }
    }
    public void ActivateSecondTimer()
    {
        if (secondTimerActive) return;

        
        secondTimerTime = remainingTime;

        secondTimerActive = true;
        Debug.Log("yeiiii");
    }

}



