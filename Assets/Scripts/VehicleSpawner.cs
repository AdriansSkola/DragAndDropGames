using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Assign 12 Vehicle GameObjects in Scene")]
    public GameObject[] vehicles;

    [Header("Assign 12 Empty Car Places (Car_place1 ... Car_place12)")]
    public GameObject[] carPlaces;

    void Start()
    {
        Debug.Log("Vehicles count: " + vehicles.Length);
        Debug.Log("Car places count: " + carPlaces.Length);

        AssignVehiclesToRandomPlaces();
    }

    void AssignVehiclesToRandomPlaces()
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < carPlaces.Length; i++)
        {
            indices.Add(i);
        }

        for (int i = 0; i < indices.Count; i++)
        {
            int randIndex = Random.Range(i, indices.Count);
            int temp = indices[i];
            indices[i] = indices[randIndex];
            indices[randIndex] = temp;
        }

        // Samaisīts randoms
        for (int i = 0; i < vehicles.Length; i++)
        {
            GameObject vehicle = vehicles[i];
            GameObject place = carPlaces[indices[i]];

            RectTransform vehicleRect = vehicle.GetComponent<RectTransform>();
            RectTransform placeRect = place.GetComponent<RectTransform>();

            // Set position
            vehicleRect.localPosition = placeRect.localPosition;

            // Random rotācija
            float randomZRotation = Random.Range(-10f, 10f);
            vehicleRect.localRotation = Quaternion.Euler(0f, 0f, randomZRotation);
        }
    }
}
