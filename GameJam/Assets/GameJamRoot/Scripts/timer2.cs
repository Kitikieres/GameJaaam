using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float initialTime = 60f;
    [SerializeField] float initialTime2 = 60f;
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
            timerText.text = Mathf.FloorToInt(remainingTime).ToString("00");
        }
        else
        {
            secondTimerTime -= Time.deltaTime;

            if (secondTimerTime < 0)
                secondTimerTime = 0;

            timerText.text = Mathf.FloorToInt(secondTimerTime).ToString("00");
        }
    }

<<<<<<< HEAD
   
=======
  
>>>>>>> 24e17b2716e46da9c8bf7bb7f5a15e67ff3aca96
    public void ActivateSecondTimer()
    {
        if (secondTimerActive) return;

        
        secondTimerTime = remainingTime;

        secondTimerActive = true;
        Debug.Log("yeiiii");
    }

}



