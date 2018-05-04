using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UAVHandler : MonoBehaviour 
{
    // Public variables
    public GameObject UAVIdlePoint;

    // Private variables
	private int visitedWaypoints = 0;
	private float UAVSpeed = 3.0f;
	private GameObject target;
	private float defaultYPosition = 1.0f;
	private bool returningToPosition = false;
	private Vector3 actualTarget;
    private int currentUAVSeedCount = 199943;
    private GUIStyle style = new GUIStyle();
    private Vector3 idlePosition = new Vector3();
    private bool atIdlePosition;

    // Constants
    private const int UAVSeedCapacity = 199943;
    private const int acreSeedCapacity = 600000;
    private const int seedsPerPlantingMarker = 89;

    private void Start()
    {
        style.normal.textColor = Color.black;
        idlePosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        atIdlePosition = true;
    }

    void Update () 
	{
		if (Input.GetKeyDown(KeyCode.R))
        {
            InvokeRepeating("PlantingRoutine", 0.0f, Time.deltaTime);
        }
	}

    private void PlantingRoutine()
    {
        float step = UAVSpeed * Time.deltaTime;
        if (atIdlePosition)
        {
            actualTarget = new Vector3(transform.position.x, defaultYPosition, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);

            if (transform.position.y == defaultYPosition)
            {
                atIdlePosition = false;
            }
        }

        if (!returningToPosition && !atIdlePosition)
        {
            target = GeneratePlantingMarkers.plantingMarkerWaypoints[visitedWaypoints];
            actualTarget = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);
        }

        if ((transform.position.x == target.transform.position.x)
            && (transform.position.z == target.transform.position.z)
            && returningToPosition == false && !atIdlePosition)
        {
            actualTarget = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);

            returningToPosition = true;
        }

        if (returningToPosition && !atIdlePosition)
        {
            if (transform.position.y == actualTarget.y)
            {
                actualTarget = new Vector3(transform.position.x, defaultYPosition, transform.position.z);
            }

            if (transform.position.y != defaultYPosition)
            {
                transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);
            }
            else
            {
                returningToPosition = false;
            }

            if (returningToPosition == false)
            {
                GeneratePlantingMarkers.plantingMarkers[visitedWaypoints].GetComponent<Renderer>().material.color = new Color(0.0f, 255.0f, 0.0f);
                visitedWaypoints += 1;
                currentUAVSeedCount -= 89;
            }
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Current UAV Seed Count: " + currentUAVSeedCount.ToString("n0"), style);
    }
}