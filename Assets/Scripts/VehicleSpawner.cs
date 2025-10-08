using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Assign 12 Vehicle GameObjects in Scene")]
    public GameObject[] vehicles;

    [Header("Assign 12 Empty Car Places (Car_place1 ... Car_place12)")]
    public GameObject[] carPlaces;

    [Header("Option A - Assign parent that contains many car_spot children")]
    public Transform carSpotsParent; // <-- ja norādīsi šo, skripts samaisīs TĀ parenta bērnus

    [Header("Option B - (fallback) Assign 12 Car Spots directly")]
    public GameObject[] carSpotsArray; // <-- ja negribi parent, vari norādīt spots kā array (fallback)

    void Start()
    {
        // Drošības pārbaudes
        if (carPlaces == null || carPlaces.Length == 0)
        {
            Debug.LogError("CarPlaces nav pievienots!");
            return;
        }

        if (vehicles == null || vehicles.Length == 0)
        {
            Debug.LogError("Vehicles nav pievienoti!");
            return;
        }

        // Izveido sarakstu ar spots (Transform), ņemot vērā preference: parent bērni > array
        List<Transform> spotsList = new List<Transform>();

        if (carSpotsParent != null && carSpotsParent.childCount > 0)
        {
            for (int i = 0; i < carSpotsParent.childCount; i++)
            {
                spotsList.Add(carSpotsParent.GetChild(i));
            }
        }
        else if (carSpotsArray != null && carSpotsArray.Length > 0)
        {
            foreach (var go in carSpotsArray)
            {
                if (go != null) spotsList.Add(go.transform);
            }
        }
        else
        {
            Debug.LogError("Nav pievienots neviens carSpots avots (carSpotsParent vai carSpotsArray)!");
            return;
        }

        // Pārliecināmies, ka spotsCount == carPlacesCount
        if (spotsList.Count != carPlaces.Length)
        {
            Debug.LogError($"CarSpots ({spotsList.Count}) un CarPlaces ({carPlaces.Length}) skaits atšķiras! Vajadzētu sakrist.");
            // tomēr turpināsim, bet izmantojot minSpotCount
        }

        // 1) Samaisam tikai spots bērnu pozīcijas/rotācijas starp sevi
        ShuffleChildPositions(spotsList);

        // 2) Novietojam carPlaces uz (tagad samaisītajiem) spots (pa indeksiem)
        AssignPlacesToSpots(spotsList);

        // 3) Random spawn vehicles to carPlaces (unique)
        AssignVehiclesRandomlyToCarPlaces();
    }

    // Shuffle positions and rotations among the given spot transforms (doesn't change hierarchy)
    void ShuffleChildPositions(List<Transform> spots)
    {
        int n = spots.Count;
        // Save original local positions & rotations
        Vector3[] positions = new Vector3[n];
        Quaternion[] rotations = new Quaternion[n];
        for (int i = 0; i < n; i++)
        {
            positions[i] = spots[i].localPosition;
            rotations[i] = spots[i].localRotation;
        }

        // Build shuffled index array
        int[] idx = new int[n];
        for (int i = 0; i < n; i++) idx[i] = i;

        // Fisher-Yates shuffle indices
        for (int i = 0; i < n; i++)
        {
            int r = Random.Range(i, n);
            int tmp = idx[i];
            idx[i] = idx[r];
            idx[r] = tmp;
        }

        // Assign shuffled positions/rotations back to the children (so positions are permuted among them)
        for (int i = 0; i < n; i++)
        {
            spots[i].localPosition = positions[idx[i]];
            spots[i].localRotation = rotations[idx[i]];
        }

        Debug.Log("CarSpots children positions shuffled.");
    }

    // Place carPlaces at spots (by index). If spots < carPlaces, uses min count.
    void AssignPlacesToSpots(List<Transform> spots)
    {
        int count = Mathf.Min(spots.Count, carPlaces.Length);
        for (int i = 0; i < count; i++)
        {
            RectTransform placeRect = carPlaces[i].GetComponent<RectTransform>();
            RectTransform spotRect = spots[i].GetComponent<RectTransform>();
            if (placeRect != null && spotRect != null)
            {
                placeRect.localPosition = spotRect.localPosition;
                placeRect.localRotation = spotRect.localRotation;
            }
            else
            {
                // fallback to world positions if RectTransform missing
                carPlaces[i].transform.position = spots[i].position;
                carPlaces[i].transform.rotation = spots[i].rotation;
            }
        }

        Debug.Log("CarPlaces positioned to (shuffled) CarSpots.");
    }

    // Assign vehicles to random, unique carPlaces (requires vehicles.Length <= carPlaces.Length)
    void AssignVehiclesRandomlyToCarPlaces()
    {
        if (vehicles.Length > carPlaces.Length)
        {
            Debug.LogError("Ir vairāk vehicles nekā carPlaces — nav pietiekami daudz vietu!");
            // still attempt to place as many as possible
        }

        // Build indices list for carPlaces
        List<int> indices = new List<int>();
        for (int i = 0; i < carPlaces.Length; i++) indices.Add(i);

        // Shuffle indices (Fisher-Yates)
        for (int i = 0; i < indices.Count; i++)
        {
            int r = Random.Range(i, indices.Count);
            int tmp = indices[i];
            indices[i] = indices[r];
            indices[r] = tmp;
        }

        // Place each vehicle to a unique random carPlace (first vehicles.Length entries of shuffled indices)
        int placeCount = Mathf.Min(vehicles.Length, carPlaces.Length);
        for (int i = 0; i < placeCount; i++)
        {
            GameObject vehicle = vehicles[i];
            GameObject place = carPlaces[indices[i]];
            if (vehicle == null || place == null) continue;

            RectTransform vehicleRect = vehicle.GetComponent<RectTransform>();
            RectTransform placeRect = place.GetComponent<RectTransform>();

            if (vehicleRect != null && placeRect != null)
            {
                vehicleRect.localPosition = placeRect.localPosition;
                // neliela random rotācija
                float randomZRotation = Random.Range(-10f, 10f);
                vehicleRect.localRotation = Quaternion.Euler(0f, 0f, randomZRotation);
            }
            else
            {
                vehicle.transform.position = place.transform.position;
                vehicle.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-10f, 10f));
            }
        }

        Debug.Log("Vehicles spawned randomly on carPlaces.");
    }
}
