using UnityEngine;
using TMPro;

public class timer2 : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;

    [SerializeField] float initialTime;   // tiempo inicial del primer contador
    float remainingTime;

    float secondTimerTime;                // tiempo del segundo contador
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
            int seconds = Mathf.FloorToInt(remainingTime);

           
            if (seconds <= 20)
            {
               
                float timeReduced =  remainingTime;


                secondTimerTime = 0f + timeReduced;

                secondTimerActive = true;
                return;
            }

            timerText.text = seconds.ToString("00");
        }
     
        else
        {
            secondTimerTime -= Time.deltaTime;
            int seconds = Mathf.FloorToInt(secondTimerTime);

            if (seconds <= 0)
            {
                seconds = 0;
                secondTimerActive = false; 
            }

            timerText.text = seconds.ToString("00");
        }
    }
}

