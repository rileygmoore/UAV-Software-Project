using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UAVHandler : MonoBehaviour 
{
    // Public variables
    public GameObject UAVIdlePoint;
	public GameObject seedRefillLocation;

    // Private variables
	private int visitedWaypoints = 0;
	private float UAVSpeed = 2.5f;
	private GameObject target;
	private float defaultYPosition = 1.0f;
	private bool returningToPosition = false;
	private Vector3 actualTarget;
	private int currentUAVSeedCount = 199943;
    private GUIStyle style = new GUIStyle();
    private Vector3 idlePosition = new Vector3();
    private bool atIdlePosition;
	private bool readyToReturn = false;
	private bool readyToRefill = false;
	private bool atRefillLocation = false;
	private bool refill = false;
	private bool readyToPlant = true;

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
		if (currentUAVSeedCount == 0 || !readyToPlant) 
		{
			Invoke ("Refill", 0.0f);
		} 
		else 
		{
			// Reset the values for the seed refilling function
			readyToReturn = false;
			readyToRefill = false;
			atRefillLocation = false;
			refill = false;
			readyToPlant = true;

			// Set the speed for the UAV
			float step = UAVSpeed * Time.deltaTime;

			// Handle UAV when it's at its starting location
			if (atIdlePosition)
			{
				actualTarget = new Vector3(transform.position.x, defaultYPosition, transform.position.z);
				transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);

				if (transform.position.y == defaultYPosition)
				{
					atIdlePosition = false;
				}
			}

			// Handle UAV while it's traveling to its next waypoint
			if (!returningToPosition && !atIdlePosition)
			{
				target = GeneratePlantingMarkers.plantingMarkerWaypoints[visitedWaypoints];
				actualTarget = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
				transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);
			}

			// Once the UAV is at the waypoint's x and z coordinates, move it downwards
			if ((transform.position.x == target.transform.position.x)
				&& (transform.position.z == target.transform.position.z)
				&& returningToPosition == false && !atIdlePosition)
			{
				actualTarget = new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z);
				transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);

				if (transform.position == actualTarget) 
				{
					returningToPosition = true;
					currentUAVSeedCount -= 89;
					GeneratePlantingMarkers.plantingMarkers[visitedWaypoints].GetComponent<Renderer>().material.color = new Color(0.0f, 255.0f, 0.0f);
					visitedWaypoints += 1;
				}
			}

			// Move the UAV back to the traveling y-axis value once the planting is completed for the current marker
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
			}
		}
	}

	private void Refill()
	{
		readyToPlant = false;

		// Set UAVSpeed to the UAV's unburdened traveling speed
		float step = (UAVSpeed * 2.172f) * Time.deltaTime;

		// Move the UAV back to the traveling y-axis value to prepare for travel
		if (!atRefillLocation) 
		{
			actualTarget = new Vector3(transform.position.x, defaultYPosition, transform.position.z);
			transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);

			if (transform.position.y == defaultYPosition) 
			{
				readyToReturn = true;
				atRefillLocation = true;
			}
		} 
		else 
		{

			// Move the UAV to the seed refilling station
			if (readyToReturn) 
			{
				actualTarget = new Vector3(seedRefillLocation.transform.position.x,
					defaultYPosition, seedRefillLocation.transform.position.z);
				transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);
			}

			// Move the UAV downwards as necessary and begin the seed refilling process
			if (transform.position.x == seedRefillLocation.transform.position.x
				&& transform.position.z == seedRefillLocation.transform.position.z
				&& !refill) 
			{
				readyToReturn = false;
				actualTarget = new Vector3(seedRefillLocation.transform.position.x,
					0.4f, seedRefillLocation.transform.position.z);
				transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);

				if (transform.position.y == 0.4f) 
				{
					StartCoroutine("RefillSeeds");
					refill = true;
				}
			}

			// Move the UAV back to the traveling y-axis value to prepare for travel
			if (currentUAVSeedCount == (UAVSeedCapacity) && transform.position.y < defaultYPosition) 
			{
				actualTarget = new Vector3(transform.position.x, defaultYPosition, transform.position.z);
				transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);

				if (transform.position.y == defaultYPosition) 
				{
					readyToPlant = true;
				}
			}
		}
	}

	private IEnumerator RefillSeeds()
	{
		yield return new WaitForSeconds(1.0f);
		currentUAVSeedCount = UAVSeedCapacity;
		readyToRefill = false;
	}

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Current UAV Seed Count: " + currentUAVSeedCount.ToString("n0"), style);
    }
}