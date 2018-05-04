using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratePlantingMarkers : MonoBehaviour
{
	// Initial planting marker and planting marker waypoint for the assigned field
    public GameObject plantingMarker;
	public GameObject plantingMarkerWaypoint;

	// Game object that stores the planting markers and waypoints to reduce clutter
	public Transform markerParent;

	// Arrays storing the planting markers and waypoints
	public static GameObject[] plantingMarkers = new GameObject[6675];
	public static GameObject[] plantingMarkerWaypoints = new GameObject[6675];

	// Variables necessary for the waypoint creation
	private int count = 0;
	private int flipMultiplier = 1;

	void Start ()
    {
		// Set the position of the initial planting marker and its waypoint
        plantingMarker.transform.position = new Vector3(transform.position.x + 67.82f, 
			transform.position.y + 0.4f, transform.position.z + 6.32f);
		plantingMarkerWaypoint.transform.position = new Vector3(transform.position.x + 67.82f, 
			transform.position.y + 0.4f, transform.position.z + 6.32f);

		// Store the position values of the initial planting marker for future use
		float startingXPosition = plantingMarker.transform.position.x;
		float startingYPosition = plantingMarker.transform.position.y;
		float startingZPosition = plantingMarker.transform.position.z;

		// Generate all necessary planting markers and waypoints for the assigned field
		for (int j = 0; j < 25; j++)
		{
			for (int i = 0; i < 267; i++) 
			{
				Vector3 newPosition = new Vector3(startingXPosition, 
					startingYPosition, startingZPosition);

				plantingMarkers [count] = Instantiate (plantingMarker, newPosition, Quaternion.identity, markerParent);
				plantingMarkerWaypoints [count] = Instantiate (plantingMarkerWaypoint, newPosition, Quaternion.identity, markerParent);

				startingXPosition -= 0.51f * flipMultiplier;
				count += 1;
			}

			startingZPosition -= 0.51f;
			flipMultiplier *= -1;
		}
	}
}