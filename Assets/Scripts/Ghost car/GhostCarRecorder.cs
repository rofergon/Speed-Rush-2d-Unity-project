using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostCarRecorder : MonoBehaviour
{
    public Transform carSpriteObject;
    public GameObject ghostCarPlaybackPrefab;

    //Local variables
    GhostCarData ghostCarData = new GhostCarData();

    bool isRecording = true;

    //Other components
    Rigidbody2D carRigidbody2D;
    CarInputHandler carInputHandler;

    private void Awake()
    {
        carRigidbody2D = GetComponent<Rigidbody2D>();
        carInputHandler = GetComponent<CarInputHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Create a ghost car
        GameObject ghostCar = Instantiate(ghostCarPlaybackPrefab);

        //Load the data for the current player
        ghostCar.GetComponent<GhostCarPlayback>().LoadData(carInputHandler.playerNumber);

        StartCoroutine(RecordCarPositionCO());
        StartCoroutine(SaveCarPositionCO());
    }

    IEnumerator RecordCarPositionCO()
    {
        while (isRecording)
        {
            if (carSpriteObject != null)
                ghostCarData.AddDataItem(new GhostCarDataListItem(carRigidbody2D.position, carRigidbody2D.rotation, carSpriteObject.localScale, Time.timeSinceLevelLoad));

            yield return new WaitForSeconds(0.15f);
        }
    }

    IEnumerator SaveCarPositionCO()
    {
        yield return new WaitForSeconds(5);

        SaveData();

    }

    void SaveData()
    {
        string jsonEncodedData = JsonUtility.ToJson(ghostCarData);

        Debug.Log($"Saved ghost data {jsonEncodedData}");

        if (carInputHandler != null)
        {
            PlayerPrefs.SetString($"{SceneManager.GetActiveScene().name}_{carInputHandler.playerNumber}_ghost", jsonEncodedData);
            PlayerPrefs.Save();
        }

        //Stop recording as we have already saved the data
        isRecording = false;

    }

}
