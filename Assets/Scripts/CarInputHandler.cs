using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInputHandler : MonoBehaviour
{
    public int playerNumber = 1;
    public bool isUIInput = false;

    Vector2 inputVector = Vector2.zero;

    //Components
    TopDownCarController topDownCarController;

    //Awake is called when the script instance is being loaded.
    void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();
    }

    // Update is called once per frame and is frame dependent
    void Update()
    {
        if (isUIInput)
        {
            // La entrada UI se maneja a través de SetInput
        }
        else
        {
            inputVector = Vector2.zero;

            // Usar los controles estándar de Unity
            inputVector.x = Input.GetAxis("Horizontal");
            inputVector.y = Input.GetAxis("Vertical");

            // Para soporte multijugador futuro, puedes descomentar y modificar este código
            /*
            switch (playerNumber)
            {
                case 1:
                    // Jugador 1 usa las flechas del teclado
                    inputVector.x = Input.GetAxis("Horizontal");
                    inputVector.y = Input.GetAxis("Vertical");
                    break;
                case 2:
                    // Jugador 2 usa WASD
                    inputVector.x = Input.GetAxis("Horizontal2");
                    inputVector.y = Input.GetAxis("Vertical2");
                    break;
            }
            */
        }

        //Send the input to the car controller.
        topDownCarController.SetInputVector(inputVector);
    }

    public void SetInput(Vector2 newInput)
    {
        inputVector = newInput;
    }
}
