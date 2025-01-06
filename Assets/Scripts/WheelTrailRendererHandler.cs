using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelTrailRendererHandler : MonoBehaviour
{
    public bool isOverpassEmitter = false;

    //Components
    TopDownCarController topDownCarController;
    TrailRenderer trailRenderer;
    CarLayerHandler carLayerHandler;

    //Awake is called when the script instance is being loaded.
    void Awake()
    {
        //Get the top down car controller
        topDownCarController = GetComponentInParent<TopDownCarController>();

        carLayerHandler = GetComponentInParent<CarLayerHandler>();

        //Get the trail renderer component.
        trailRenderer = GetComponent<TrailRenderer>();

        //Set the trail renderer to not emit in the start. 
        trailRenderer.emitting = false;
    }


    // Update is called once per frame
    void Update()
    {
        trailRenderer.emitting = false;

        //If the car tires are screeching then we'll emitt a trail.
        if (topDownCarController.IsTireScreeching(out float lateralVelocity, out bool isBraking))
        {
            if (carLayerHandler.IsDrivingOnOverpass() && isOverpassEmitter)
                trailRenderer.emitting = true;

            if (!carLayerHandler.IsDrivingOnOverpass() && !isOverpassEmitter)
                trailRenderer.emitting = true;
        }


    }
}
