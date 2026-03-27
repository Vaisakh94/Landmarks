using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif 

//This script ensures that the objects on the shelves of the supermarket spawn and are arranged randomly automatically. 
// [ExecuteAlways] makes the script run in the Scene View without pressing Play.
[ExecuteAlways]
public class Objects_On_Shelf_Randomizer : MonoBehaviour
{
    [Header("Experimental Control")]
    public int randomSeed = 1;             // Change number for each shelf; each number represents a specific random arrangement of objects on the shelves

    [Header("Item Settings")]
    public GameObject[] itemPool;           // The list of possible 3D products to spawn.
    public int itemsPerRow = 5;             // How many items sit side-by-side on each row.
    public int rows = 4;                    // How many vertical layers (planks) to fill.
    
    [Header("Spacing Settings")]
    public float horizontalSpacing = 0.3f;  // Gap between items in a row.
    public float verticalSpacing = 0.4f;    // Height gap between planks.
    
    [Header("Placement Alignment")]
    public Vector3 startOffset;             // Manual nudge to align items with the mesh.
    public Vector3 rotationOffset;          // Set rotation values in the Inspector 
    // OnValidate runs every time you change a number in the Inspector window.
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {   
            #if UNITY_EDITOR
            // We tell the Editor: "As soon as you're done with this frame, update the shelf."
            UnityEditor.EditorApplication.delayCall += PopulateShelf;
            #endif
        }
    }

    public void PopulateShelf()
    {
        // Safety check: if the script is deleted or the pool is empty, stop.
        if (this == null || itemPool == null || itemPool.Length == 0) return;

        // Step 1: Delete any existing random items so we don't stack them forever.
            ClearOldItems();

        // Step 2: Set the "Random State." 
        // Using Position (x + z) ensures Shelf A looks different from Shelf B,
        // but because of the Seed, Shelf A will ALWAYS look the same for every user.
        Random.InitState(randomSeed + (int)transform.position.x + (int)transform.position.z);

        // Step 3: The "Nested Loop" (Filling the Grid)
        for (int r = 0; r < rows; r++)              // Loop through each Row (Height)
        {
            for (int i = 0; i < itemsPerRow; i++)   // Loop through each Item in that row (Width)
            {
                // Grab a random 3D model from your array.
                GameObject prefab = itemPool[Random.Range(0, itemPool.Length)];
                if (prefab == null) continue;
                 
                // MATH BREAKDOWN:
                // transform.position = The center of the shelf object.
                // startOffset = Your manual correction.
                // (transform.right * i * horizontalSpacing) = Moves item along the shelf's "Red" axis.
                // (transform.up * r * verticalSpacing) = Moves item along the shelf's "Green" axis.
                Vector3 spawnPos = transform.position + startOffset 
                                   + (transform.right * (i * horizontalSpacing)) 
                                   + (transform.up * (r * verticalSpacing));
                
                // Get the shelf's current rotation
                Quaternion shelfRot = transform.rotation;

                //Add your custom correction (e.g., 90 on Y)
                Quaternion correction = Quaternion.Euler(rotationOffset);

                //Combine them (The order of multiplication MATTERS in math!)
                //This ensures the item stays aligned to the shelf but turns on its own feet.
                Quaternion finalRotation = shelfRot * correction;

                GameObject newItem = null;

                // Step 4: Spawning the item.
                #if UNITY_EDITOR
                // In Editor Mode, we use PrefabUtility so the items stay linked to their source files.
                newItem = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                newItem.transform.position = spawnPos;
                newItem.transform.rotation = finalRotation; // Apply rotation LAST
                //newItem.transform.localScale = Vector3.one; // FORCED FIX FOR SQUISHING
                #else
                // In the actual Game/Build, we use standard Instantiate.
                newItem = Instantiate(prefab, spawnPos, finalRotation);
                #endif

                // Step 5: Organization and Cleanup.
                newItem.name = prefab.name + " (RandomItem)";
                newItem.transform.SetParent(this.transform);    // Put the item inside the shelf object.
                newItem.transform.localScale = Vector3.one;    // Forced fix for squishing 
                
                // HideFlags prevents these temporary items from saving into your scene file,
                // which keeps your file size small and prevents "double spawning."
                //newItem.gameObject.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            }
        }
    }

    // Helper function to find and remove items tagged as "(RandomItem)".
    void ClearOldItems()
    {
        // We create a list first so we aren't deleting while looping
    List<GameObject> toDestroy = new List<GameObject>();
    foreach (Transform child in transform)
    {
        if (child.name.Contains("(RandomItem)"))
        {
            toDestroy.Add(child.gameObject);
        }
    }

    foreach (GameObject obj in toDestroy)
    {
        // Use DestroyImmediate in Editor, and regular Destroy in Game
        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
    }
    } 
}
