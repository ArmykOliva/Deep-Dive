using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBorder : MonoBehaviour
{
	public Transform place;
	public float radius;

	void OnDrawGizmos()
	{
		if (place != null)
		{
			Gizmos.color = Color.white; // Set the color for the gizmos
			DrawGizmoCircle(place.position, radius);
		}
	}

	void DrawGizmoCircle(Vector3 center, float radius)
	{
		int numSegments = 360; // The number of line segments that make up the circle
		float deltaTheta = (2f * Mathf.PI) / numSegments; // The angle between each segment
		float theta = 0f;

		Vector3 prevPoint = center + new Vector3(radius, 0f, 0f); // Starting point

		for (int i = 0; i < numSegments; i++)
		{
			theta += deltaTheta;
			float x = radius * Mathf.Cos(theta);
			float y = radius * Mathf.Sin(theta);
			Vector3 nextPoint = center + new Vector3(x, y, 0f);
			Gizmos.DrawLine(prevPoint, nextPoint); // Draw a line between the previous point and the next
			prevPoint = nextPoint; // Update the previous point
		}
	}

	// Function to get a random point on the circle that is further than originalPoint + 1
	public Vector3 GetRandomPointOnCircle(Vector3 originalPoint,float minDistance)
	{
		Vector3 randomPoint = Vector3.zero;
		float distance;

		do
		{
			// Generate a random angle between 0 to 360 degrees (0 to 2 * PI radians)
			float angle = Random.Range(0f, Mathf.PI * 2f);

			// Generate a random radius between minDistance and the maximum radius
			float randomRadius = Random.Range(minDistance, radius);

			// Convert the angle and random radius to a point inside the circle (x, y)
			randomPoint.x = place.position.x + randomRadius * Mathf.Cos(angle);
			randomPoint.y = place.position.y + randomRadius * Mathf.Sin(angle);
			randomPoint.z = place.position.z; // Keep the z-coordinate the same

			// Calculate the distance from the original point to the new random point
			distance = Vector2.Distance(new Vector2(randomPoint.x, randomPoint.y), new Vector2(originalPoint.x, originalPoint.y));

		} while (distance <= minDistance); // Ensure the point is further than originalPoint + 1

		return randomPoint;
	}

	public Transform CreateRandomTransportPointOnCircle(Vector3 originalPoint, float minDistance)
	{
		// Get a random point on the circle that meets the distance criteria
		Vector3 point = GetRandomPointOnCircle(originalPoint, minDistance);

		// Create a new GameObject at the random point
		GameObject transportPoint = new GameObject("RandomTransportPoint");
		transportPoint.transform.position = point;

		// Optionally, if you want to parent it to the current game object to keep the hierarchy clean
		transportPoint.transform.parent = this.transform;

		// Return the Transform of the new GameObject
		return transportPoint.transform;
	}
}
