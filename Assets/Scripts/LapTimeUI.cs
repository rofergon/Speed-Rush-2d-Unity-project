using UnityEngine;
using TMPro;

public class LapTimeUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI currentLapTimeText;
    public TextMeshProUGUI lastLapTimesText;
    public GameObject mintPopupPanel;
    public TextMeshProUGUI mintPopupText;

    [Header("UI Messages")]
    [SerializeField] private string mintPopupTitle = "Would you like to mint your lap times?";

    private void Start()
    {
        if (mintPopupPanel != null)
            mintPopupPanel.SetActive(false);
    }

    public void UpdateCurrentLapTime(string time)
    {
        if (currentLapTimeText != null)
            currentLapTimeText.text = time;
    }

    public void UpdateLastLapTimes(string times)
    {
        if (lastLapTimesText != null)
            lastLapTimesText.text = times;
    }

    public void ShowMintPopup(string lapTimes)
    {
        if (mintPopupPanel != null)
        {
            mintPopupPanel.SetActive(true);
            if (mintPopupText != null)
            {
                mintPopupText.text = $"{mintPopupTitle}\n\n{lapTimes}";
            }
        }
    }

    public void HideMintPopup()
    {
        if (mintPopupPanel != null)
            mintPopupPanel.SetActive(false);
    }
} 