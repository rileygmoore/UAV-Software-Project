using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UAVHandler : MonoBehaviour 
{
    // Public variables
    public GameObject UAVIdlePoint;
	public GameObject seedRefillLocation;
	public GameObject batteryRechargeLocation;

    // Private variables
	private int visitedWaypoints = 0;
	private float UAVSpeed = 2.5f;
	private GameObject target;
	private float defaultYPosition = 1.0f;
	private bool returningToPosition = false;
	private Vector3 actualTarget;
	private int currentUAVSeedCount = 199894;
    private GUIStyle style = new GUIStyle();
    private Vector3 idlePosition = new Vector3();
    private bool atIdlePosition;
	private bool readyToReturn = false;
	private bool readyToRefill = false;
	private bool atRefillLocation = false;
	private bool refill = false;
	private bool readyToPlant = true;
	private bool needsToRecharge = false;
	private float currentBatteryCharge = 11142.76f;
	private float batteryDischargeRate = 0.007f;
	private float playedTime = 0.0f;

    // Constants
	private const int UAV_SEED_CAPACITY = 199894;
	private const int ACRE_SEED_CAPACITY = 600000;
	private const int SEEDS_PER_PLANTING_MARKER = 89;
	private const float MAX_BATTERY_CHARGE = 11142.76f;
	private const float BATTERY_RECHARGE_THRESHOLD = 1114.28f;

    private void Start()
    {
        style.normal.textColor = Color.black;
        idlePosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        atIdlePosition = true;
    }

    void Update () 
	{
		playedTime += Time.deltaTime;
		Invoke("DischargeBattery", 1.0f * Time.deltaTime);

		if (currentUAVSeedCount == 0 || !readyToPlant) 
		{
			Invoke("Refill", 0.0f);
		} 
		else if (currentBatteryCharge <= BATTERY_RECHARGE_THRESHOLD || needsToRecharge) 
		{
			Invoke("Recharge", 0.0f);
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
					batteryDischargeRate = 0.0f;
					StartCoroutine("RefillSeeds");
					refill = true;
				}
			}

			// Move the UAV back to the traveling y-axis value to prepare for travel
			if (currentUAVSeedCount == (UAV_SEED_CAPACITY) && transform.position.y < defaultYPosition) 
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

	private void Recharge()
	{
		needsToRecharge = true;

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
				actualTarget = new Vector3(batteryRechargeLocation.transform.position.x,
					defaultYPosition, batteryRechargeLocation.transform.position.z);
				transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);
			}

			// Move the UAV downwards as necessary and begin the seed refilling process
			if (transform.position.x == batteryRechargeLocation.transform.position.x
				&& transform.position.z == batteryRechargeLocation.transform.position.z
				&& !refill) 
			{
				readyToReturn = false;
				actualTarget = new Vector3(batteryRechargeLocation.transform.position.x,
					0.4f, batteryRechargeLocation.transform.position.z);
				transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);

				if (transform.position.y == 0.4f) 
				{
					batteryDischargeRate = 0.0f;
					Invoke("RechargeBattery", 0.0f);
					refill = true;
				}
			}

			// Move the UAV back to the traveling y-axis value to prepare for travel
			if (currentBatteryCharge == (MAX_BATTERY_CHARGE) && transform.position.y < defaultYPosition) 
			{
				actualTarget = new Vector3(transform.position.x, defaultYPosition, transform.position.z);
				transform.position = Vector3.MoveTowards(transform.position, actualTarget, step);

				if (transform.position.y == defaultYPosition) 
				{
					needsToRecharge = false;
				}
			}
		}
	}

	private IEnumerator RefillSeeds()
	{
		yield return new WaitForSeconds(7.5f);
		currentUAVSeedCount = UAV_SEED_CAPACITY;
		readyToRefill = false;
		batteryDischargeRate = 0.007f;
	}

	private void RechargeBattery()
	{
		currentBatteryCharge = MAX_BATTERY_CHARGE;
	}

	private void DischargeBattery()
	{
		currentBatteryCharge -= batteryDischargeRate;
	}

    private void OnGUI()
    {
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Current UAV Seed Count: " + currentUAVSeedCount.ToString("n0"), style);
		GUI.Label(new Rect(0, 15, Screen.width, Screen.height), "Current UAV Battery Charge: " + ((currentBatteryCharge / MAX_BATTERY_CHARGE) * 100).ToString("n0"), style);
		GUI.Label(new Rect(0, 30, Screen.width, Screen.height), "Elapsed Time: " + (playedTime).ToString() + " seconds", style);
    }
}
