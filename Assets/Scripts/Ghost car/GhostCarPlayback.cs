using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostCarPlayback : MonoBehaviour
{
    //Local variabels
    GhostCarData ghostCarData = new GhostCarData();
    List<GhostCarDataListItem> ghostCarDataList = new List<GhostCarDataListItem>();

    //Playback index
    int currentPlaybackIndex = 0;

    // Playback stored information
    float lastStoredTime = 0.1f;
    Vector2 lastStoredPostion = Vector2.zero;
    float lastStoredRotation = 0;
    Vector3 lastStoredLocalScale = Vector3.zero;

    //Duration of the data frame
    float duration = 0.1f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //We can only playback data if there is some data. 
        if (ghostCarDataList.Count == 0)
            return;

        if (Time.timeSinceLevelLoad >= ghostCarDataList[currentPlaybackIndex].timeSinceLevelLoaded)
        {
            lastStoredTime = ghostCarDataList[currentPlaybackIndex].timeSinceLevelLoaded;
            lastStoredPostion = ghostCarDataList[currentPlaybackIndex].position;
            lastStoredRotation = ghostCarDataList[currentPlaybackIndex].rotationZ;
            lastStoredLocalScale = ghostCarDataList[currentPlaybackIndex].localScale;

            //Step to the next item
            if (currentPlaybackIndex < ghostCarDataList.Count - 1)
                currentPlaybackIndex++;

            duration = ghostCarDataList[currentPlaybackIndex].timeSinceLevelLoaded - lastStoredTime;
        }

        //Calculate how much of the data frame that we have completed. 
        float timePassed = Time.timeSinceLevelLoad - lastStoredTime;
        float lerpPercentage = timePassed / duration;

        //Lerp everything
        transform.position = Vector2.Lerp(lastStoredPostion, ghostCarDataList[currentPlaybackIndex].position, lerpPercentage);
        transform.rotation = Quaternion.Lerp(Quaternion.Euler(0, 0, lastStoredRotation), Quaternion.Euler(0, 0, ghostCarDataList[currentPlaybackIndex].rotationZ), lerpPercentage);
        transform.localScale = Vector3.Lerp(lastStoredLocalScale, ghostCarDataList[currentPlaybackIndex].localScale, lerpPercentage);
    }

    public void LoadData(int playerNumber)
    {
        if (!PlayerPrefs.HasKey($"{SceneManager.GetActiveScene().name}_{playerNumber}_ghost"))
            Destroy(gameObject);
        else
        {
            string jsonEncodedData = PlayerPrefs.GetString($"{SceneManager.GetActiveScene().name}_{playerNumber}_ghost");

            ghostCarData = JsonUtility.FromJson<GhostCarData>(jsonEncodedData);
            ghostCarDataList = ghostCarData.GetDataList();

        }

    }
}
