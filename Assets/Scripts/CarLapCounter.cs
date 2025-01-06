using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class CarLapCounter : MonoBehaviour
{
    public Text carPositionText;
    public LapTimeUI lapTimeUI;

    int passedCheckPointNumber = 0;
    float timeAtLastPassedCheckPoint = 0;
    float currentLapStartTime = 0;
    float[] lastLapTimes = new float[2];
    int lastLapIndex = 0;

    int numberOfPassedCheckpoints = 0;
    int lapsCompleted = 0;
    const int lapsToComplete = 2;
    bool isRaceCompleted = false;
    int carPosition = 0;
    bool isHideRoutineRunning = false;
    float hideUIDelayTime;
    bool hasRaceStarted = false;

    TopDownCarController carController;
    HashSet<int> checkpointsPassedThisLap = new HashSet<int>();
    int totalCheckpoints = 0;

    //Events
    public event Action<CarLapCounter> OnPassCheckpoint;

    void Start()
    {
        carController = GetComponent<TopDownCarController>();
        if (lapTimeUI != null)
            lapTimeUI.UpdateCurrentLapTime("Press accelerate to start!");
        else
            Debug.LogWarning("LapTimeUI no está asignado en CarLapCounter");

        if (carPositionText == null)
            Debug.LogWarning("carPositionText no está asignado en CarLapCounter");

        // Contar el número total de checkpoints en la escena
        CheckPoint[] allCheckpoints = FindObjectsOfType<CheckPoint>();
        totalCheckpoints = allCheckpoints.Length;
        Debug.Log($"Total checkpoints found in scene: {totalCheckpoints}");
        
        // Listar todos los checkpoints encontrados
        foreach (var cp in allCheckpoints)
        {
            Debug.Log($"Checkpoint {cp.checkPointNumber} found, isFinishLine: {cp.isFinishLine}");
        }
    }

    void Update()
    {
        if (!hasRaceStarted)
        {
            if (carController != null && Input.GetAxis("Vertical") > 0)
            {
                hasRaceStarted = true;
                currentLapStartTime = Time.time;
                Debug.Log("Race started!");
            }
        }
        else if (!isRaceCompleted)
        {
            UpdateLapTimeDisplay();
        }
    }

    void UpdateLapTimeDisplay()
    {
        if (lapTimeUI != null)
        {
            float currentTime = Time.time - currentLapStartTime;
            lapTimeUI.UpdateCurrentLapTime($"Current Lap: {FormatTime(currentTime)}");
        }
    }

    string FormatTime(float timeInSeconds)
    {
        int minutes = (int)(timeInSeconds / 60);
        int seconds = (int)(timeInSeconds % 60);
        int milliseconds = (int)((timeInSeconds * 100) % 100);
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    public void SetCarPosition(int position)
    {
        carPosition = position;
        if (carPositionText != null)
            carPositionText.text = position.ToString();
    }

    public int GetNumberOfCheckpointsPassed()
    {
        return numberOfPassedCheckpoints;
    }

    public float GetTimeAtLastCheckPoint()
    {
        return timeAtLastPassedCheckPoint;
    }

    void ShowMintPopup()
    {
        if (lapTimeUI != null)
        {
            string timesText = "Last lap times:\n";
            for (int i = 0; i < lastLapTimes.Length; i++)
            {
                if (lastLapTimes[i] > 0)
                    timesText += $"Lap {i + 1}: {FormatTime(lastLapTimes[i])}\n";
            }
            lapTimeUI.ShowMintPopup(timesText);
        }
    }

    public void OnMintConfirmed()
    {
        Debug.Log("Minting lap times to leaderboard...");
        
        // Crear un string con los tiempos de vuelta
        string timesData = "";
        for (int i = 0; i < lastLapTimes.Length; i++)
        {
            if (lastLapTimes[i] > 0)
                timesData += $"Lap {i + 1}: {FormatTime(lastLapTimes[i])}|";
        }
        
        // Enviar datos al frontend
        SendLapTimesToFrontend(timesData);
        
        if (lapTimeUI != null)
            lapTimeUI.HideMintPopup();
    }

    private void SendLapTimesToFrontend(string lapTimesData)
    {
        // Llamar a una función JavaScript usando JSInterop
        Application.ExternalCall("onLapTimesMinted", lapTimesData);
    }

    public void OnMintCanceled()
    {
        if (lapTimeUI != null)
            lapTimeUI.HideMintPopup();
    }

    IEnumerator ShowPositionCO(float delayUntilHidePosition)
    {
        if (carPositionText == null)
        {
            Debug.LogWarning("Intentando mostrar posición pero carPositionText es null");
            yield break;
        }

        hideUIDelayTime += delayUntilHidePosition;
        carPositionText.text = carPosition.ToString();
        carPositionText.gameObject.SetActive(true);

        if (!isHideRoutineRunning)
        {
            isHideRoutineRunning = true;
            yield return new WaitForSeconds(hideUIDelayTime);
            carPositionText.gameObject.SetActive(false);
            isHideRoutineRunning = false;
        }
    }

    void OnTriggerEnter2D(Collider2D collider2D)
    {
        if (collider2D.CompareTag("CheckPoint"))
        {
            if (isRaceCompleted || !hasRaceStarted)
            {
                Debug.Log($"Checkpoint ignorado - Carrera completada: {isRaceCompleted}, Carrera iniciada: {hasRaceStarted}");
                return;
            }

            CheckPoint checkPoint = collider2D.GetComponent<CheckPoint>();
            if (checkPoint == null)
            {
                Debug.LogWarning("Object tagged as CheckPoint but missing CheckPoint component!");
                return;
            }

            Debug.Log($"Triggered checkpoint {checkPoint.checkPointNumber}, isFinishLine: {checkPoint.isFinishLine}");
            Debug.Log($"Current checkpoints passed this lap: {string.Join(", ", checkpointsPassedThisLap)}");

            // Si es la línea de meta
            if (checkPoint.isFinishLine)
            {
                Debug.Log($"Llegada a meta - Checkpoints pasados: {checkpointsPassedThisLap.Count}, Total necesario: {totalCheckpoints - 1}");
                // Solo completar la vuelta si hemos pasado por todos los checkpoints
                if (checkpointsPassedThisLap.Count >= totalCheckpoints - 1)
                {
                    float lapTime = Time.time - currentLapStartTime;
                    lastLapTimes[lastLapIndex] = lapTime;
                    lastLapIndex = (lastLapIndex + 1) % 2;
                    
                    string timesText = "Last lap times:\n";
                    for (int i = 0; i < lastLapTimes.Length; i++)
                    {
                        if (lastLapTimes[i] > 0)
                            timesText += $"Lap {i + 1}: {FormatTime(lastLapTimes[i])}\n";
                    }
                    if (lapTimeUI != null)
                        lapTimeUI.UpdateLastLapTimes(timesText);
                    
                    currentLapStartTime = Time.time;
                    lapsCompleted++;
                    checkpointsPassedThisLap.Clear();

                    Debug.Log($"Completed lap {lapsCompleted}");

                    if (lapsCompleted >= lapsToComplete)
                    {
                        isRaceCompleted = true;
                        ShowMintPopup();
                    }
                }
                else
                {
                    Debug.Log($"Crossed finish line but missing checkpoints! Only passed {checkpointsPassedThisLap.Count} out of {totalCheckpoints - 1}");
                }
            }
            else
            {
                // Registrar este checkpoint como pasado
                checkpointsPassedThisLap.Add(checkPoint.checkPointNumber);
                numberOfPassedCheckpoints++;
                timeAtLastPassedCheckPoint = Time.time;
                Debug.Log($"Checkpoint {checkPoint.checkPointNumber} registrado. Total en esta vuelta: {checkpointsPassedThisLap.Count}");
            }

            OnPassCheckpoint?.Invoke(this);

            if (isRaceCompleted)
                StartCoroutine(ShowPositionCO(100));
            else if(checkPoint.isFinishLine) 
                StartCoroutine(ShowPositionCO(1.5f));
        }
    }
}
