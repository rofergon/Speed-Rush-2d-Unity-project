using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[Serializable]
public class CarNFTStats
{
    public float speed;
    public float acceleration;
    public float handling;
    public float drift;
    public float turn;
    public float maxSpeed;
}

public class PreRaceCanvas : MonoBehaviour
{
    // Constantes para normalización de estadísticas
    private const float BASE_SPEED = 90f;        // Velocidad máxima base
    private const float BASE_ACCELERATION = 30f;  // Aceleración máxima base
    private const float BASE_HANDLING = 3.5f;     // Manejo máximo base
    private const float BASE_DRIFT = 0.95f;       // Drift máximo base
    private const float BASE_TURN = 3.5f;         // Turn máximo base
    
    // Multiplicadores para ajustar los valores del NFT
    private const float SPEED_MULTIPLIER = 10f;      // Mantiene la velocidad similar
    private const float ACCELERATION_MULTIPLIER = 4f; // Aumentado para mejor aceleración
    private const float HANDLING_MULTIPLIER = 8f;    // Aumentado significativamente para mejor manejo

    // Valores mínimos para asegurar jugabilidad base
    private const float MIN_SPEED_PERCENT = 40f;     // Velocidad mínima 40%
    private const float MIN_ACCELERATION_PERCENT = 30f; // Aceleración mínima 30%
    private const float MIN_HANDLING_PERCENT = 50f;    // Manejo mínimo 50%

    [Header("UI Elements")]
    public Image carPreviewImage;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI accelerationText;
    public TextMeshProUGUI handlingText;
    public Button startRaceButton;
    public GameObject preRacePanel;
    public GameObject gameplayCanvas;

    [Header("Car References")]
    public TopDownCarController carController;
    public CarSkinLoader carSkinLoader;

    private void Start()
    {
        if (preRacePanel) preRacePanel.SetActive(true);
        if (gameplayCanvas) gameplayCanvas.SetActive(false);

        if (startRaceButton)
        {
            startRaceButton.onClick.AddListener(StartRace);
        }

        if (carController)
        {
            carController.enabled = false;
        }
    }

    // Normaliza un valor del NFT al rango deseado
    private float NormalizeNFTValue(float nftValue, float baseValue, float multiplier, float minPercent)
    {
        // Asegurarse que el valor está entre 0 y 10
        float clampedValue = Mathf.Clamp(nftValue, 0f, 10f);
        
        // Calcular el porcentaje base desde el valor mínimo
        float basePercent = minPercent + (clampedValue * multiplier);
        
        // Asegurar que no exceda el 100%
        return Mathf.Clamp(basePercent, minPercent, 100f);
    }

    public void OnStatsReceived(string statsJson)
    {
        try
        {
            CarNFTStats stats = JsonUtility.FromJson<CarNFTStats>(statsJson);
            UpdateCarStats(stats);
            
            if (carController)
            {
                // Normalizar y convertir los valores a porcentajes para el controlador
                float normalizedSpeed = NormalizeNFTValue(stats.speed, BASE_SPEED, SPEED_MULTIPLIER, MIN_SPEED_PERCENT);
                float normalizedAcceleration = NormalizeNFTValue(stats.acceleration, BASE_ACCELERATION, ACCELERATION_MULTIPLIER, MIN_ACCELERATION_PERCENT);
                float normalizedHandling = NormalizeNFTValue(stats.handling, BASE_HANDLING, HANDLING_MULTIPLIER, MIN_HANDLING_PERCENT);

                Debug.Log($"Valores normalizados - Speed: {normalizedSpeed}%, Acceleration: {normalizedAcceleration}%, Handling: {normalizedHandling}%");

                carController.UpdateStatsWithPercentages(
                    normalizedSpeed,
                    normalizedAcceleration,
                    normalizedHandling
                );
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing car stats: {e.Message}");
        }
    }

    private void UpdateCarStats(CarNFTStats stats)
    {
        if (speedText)
        {
            speedText.text = $"Speed: {stats.speed}";
        }

        if (accelerationText)
        {
            accelerationText.text = $"Acceleration: {stats.acceleration}";
        }

        if (handlingText)
        {
            handlingText.text = $"Handling: {stats.handling}";
        }
    }

    public void StartRace()
    {
        if (preRacePanel) preRacePanel.SetActive(false);
        if (gameplayCanvas) gameplayCanvas.SetActive(true);
        if (carController) carController.enabled = true;
    }

    public void UpdateCarPreview(Sprite carSprite)
    {
        if (carPreviewImage && carSprite)
        {
            carPreviewImage.sprite = carSprite;
            carPreviewImage.preserveAspect = true;
        }
    }
} 