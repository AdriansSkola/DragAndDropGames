using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Assign 12 Vehicle GameObjects in Scene")]
    public GameObject[] vehicles;

    [Header("Assign 12 Empty Car Places (Car_place1 ... Car_place12)")]
    public GameObject[] carPlaces;

    [Header("Assign 12 Empty Car Spots (Car_spot1 ... Car_spot12)")]
    public GameObject[] carSpots;

    void Start()
    {
        // 🔍 Validācijas pārbaude
        if (!ValidateAssignments())
        {
            Debug.LogError("❌ VehicleSpawner nav pareizi aizpildīts Inspector!");
            return;
        }

        Debug.Log($"🚗 Vehicles: {vehicles.Length}, Places: {carPlaces.Length}, Spots: {carSpots.Length}");

        ShuffleCarSpots();        // 🔁 Samaina carSpots savā starpā
        AssignVehiclesToPlaces(); // 🚗 Novieto mašīnas uz random carPlaces
    }

    /// <summary>
    /// Pārbauda vai visi masīvi ir aizpildīti un satur pareizo elementu skaitu.
    /// </summary>
    bool ValidateAssignments()
    {
        if (vehicles == null || carPlaces == null || carSpots == null)
        {
            Debug.LogError("❌ Viens no masīviem (vehicles, carPlaces, carSpots) nav inicializēts!");
            return false;
        }

        if (vehicles.Length == 0 || carPlaces.Length == 0 || carSpots.Length == 0)
        {
            Debug.LogError("❌ Masīvi ir tukši — pārliecinies, ka Inspectorā ir pievienoti objekti!");
            return false;
        }

        // Pārbauda vai kāds no elementiem nav Missing
        for (int i = 0; i < vehicles.Length; i++)
        {
            if (vehicles[i] == null)
            {
                Debug.LogError($"❌ Vehicles[{i}] trūkst! Pārbaudi Inspectorā.");
                return false;
            }
        }

        for (int i = 0; i < carPlaces.Length; i++)
        {
            if (carPlaces[i] == null)
            {
                Debug.LogError($"❌ CarPlaces[{i}] trūkst! Pārbaudi Inspectorā.");
                return false;
            }
        }

        for (int i = 0; i < carSpots.Length; i++)
        {
            if (carSpots[i] == null)
            {
                Debug.LogError($"❌ CarSpots[{i}] trūkst! Pārbaudi Inspectorā.");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 🔁 Samaisa carSpots savā starpā, saglabājot viņu pozīcijas/rotācijas
    /// </summary>
    void ShuffleCarSpots()
    {
        List<Vector3> positions = new List<Vector3>();
        List<Quaternion> rotations = new List<Quaternion>();

        foreach (GameObject spot in carSpots)
        {
            RectTransform rt = spot.GetComponent<RectTransform>();
            positions.Add(rt.localPosition);
            rotations.Add(rt.localRotation);
        }

        // Fisher-Yates shuffle
        for (int i = 0; i < positions.Count; i++)
        {
            int randIndex = Random.Range(i, positions.Count);
            (positions[i], positions[randIndex]) = (positions[randIndex], positions[i]);
            (rotations[i], rotations[randIndex]) = (rotations[randIndex], rotations[i]);
        }

        // Pielieto samainītās vietas
        for (int i = 0; i < carSpots.Length; i++)
        {
            RectTransform rt = carSpots[i].GetComponent<RectTransform>();
            rt.localPosition = positions[i];
            rt.localRotation = rotations[i];
        }

        Debug.Log("✅ CarSpots randomized successfully!");
    }

    /// <summary>
    /// 🚗 Novieto mašīnas uz random carPlaces.
    /// </summary>
    void AssignVehiclesToPlaces()
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < carPlaces.Length; i++)
            indices.Add(i);

        // Samaisa carPlaces indeksus
        for (int i = 0; i < indices.Count; i++)
        {
            int randIndex = Random.Range(i, indices.Count);
            (indices[i], indices[randIndex]) = (indices[randIndex], indices[i]);
        }

        // Novieto mašīnas uz random carPlaces
        for (int i = 0; i < vehicles.Length; i++)
        {
            GameObject vehicle = vehicles[i];
            GameObject place = carPlaces[indices[i]];

            if (vehicle == null || place == null) continue;

            RectTransform vehicleRect = vehicle.GetComponent<RectTransform>();
            RectTransform placeRect = place.GetComponent<RectTransform>();

            vehicleRect.localPosition = placeRect.localPosition;

            float randomZRotation = Random.Range(-10f, 10f);
            vehicleRect.localRotation = Quaternion.Euler(0f, 0f, randomZRotation);
        }

        Debug.Log("🚙 Vehicles placed randomly on CarPlaces!");
    }
}
