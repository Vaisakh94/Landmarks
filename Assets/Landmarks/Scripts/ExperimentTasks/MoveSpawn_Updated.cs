using UnityEngine;
using System.Collections;
using Landmarks.Scripts.Progress;
using UnityStandardAssets.Characters.FirstPerson;

public class MoveSpawn_Updated : ExperimentTask {
    
    [HideInInspector] public GameObject start;

    [Header("Task-specific Properties")]
    public GameObject destination;
    public string destinationListName;
	public ObjectList destinations;
    public bool useLocalRotation = true;

	public bool swap;
	private static Vector3 position;
	private static Vector3 rotation;

    public bool randomRotation;
	public bool scaledPlayer = false;
    public bool ignoreY = false;

    public bool useSnappoint; 

    // //variables used for block repetition
    // public bool blockRepeat;
    // public static int repetition;
    // private int count = 1;

	public override void startTask () {
        TASK_START();
	}
    public override void TASK_START() {
    if (!manager) Start(); // Ensure manager is initialized
    base.startTask();

    // If the Destinations slot is empty but we have a name to search for
    if (destinations == null && !string.IsNullOrEmpty(destinationListName)) 
    {
        GameObject foundList = GameObject.Find(destinationListName);
        if (foundList != null)
        {
            destinations = foundList.GetComponent<ObjectList>();
            Debug.Log("MoveSpawn_Updated: Successfully linked to " + destinationListName);
        }
        else 
        {
            Debug.LogWarning("MoveSpawn_Updated: Could not find list named " + destinationListName);
        }
    }

    // 2. TARGET SELECTION
    if (destinations) {
        destination = destinations.currentObject();
        
        // Handle SnapPoint logic
        if (useSnappoint && destination.GetComponentInChildren<LM_SnapPoint>() != null)
        {
            destination = destination.GetComponentInChildren<LM_SnapPoint>().gameObject;
        }

        // 3. APPLY POSITION & ROTATION
        position = destination.transform.position;
        // Use eulerAngles to avoid Quaternion conversion errors
        rotation = destination.transform.eulerAngles; 

        // Teleport the player
        avatar.transform.position = position;
        log.log("TASK_POSITION\t" + avatar.name + "\t" + this.GetType().Name + "\t" + avatar.transform.transform.position.ToString("f1"), 1);

        if (useLocalRotation) 
            avatar.transform.localRotation = destination.transform.localRotation;
        else 
            //avatar.transform.rotation = destination.transform.rotation;
            avatar.transform.eulerAngles = rotation;

            // This tells the FirstPersonController to update its internal mouse rotation logic to match the new transform rotation we just applied.
            FirstPersonController fps = avatar.GetComponent<FirstPersonController>();
            if (fps != null) 
            {
                fps.ResetMouselook();
            }
        
            log.log("TASK_ROTATE\t" + avatar.name + "\t" + this.GetType().Name + "\t" + avatar.transform.localEulerAngles.ToString("f1"), 1);

         // Refresh physics to prevent character controller "snapping" back
            Physics.SyncTransforms();
        
        // Final framework logic
            LM_Progress.Instance.ResumeLastPlayerPositionToNavStart(avatar.transform);
    }
}
	// public override void TASK_START() {
	// 	base.startTask();

    //     // Get the current target object from the ObjectList
	// 	if (destinations) {
	// 		destination = destinations.currentObject();
    //         Debug.Log("Printing Destinations ------" + destination);
	// 		if (useSnappoint && destination.GetComponentInChildren<LM_SnapPoint>() != null)
	// 		{
	// 			Debug.Log("Using Snappoint instead of the destination object itself");
	// 			destination = destination.GetComponentInChildren<LM_SnapPoint>().gameObject;
	// 		}

    //         position = destination.transform.position;
	// 	    rotation = destination.transform.eulerAngles;

    //         if (ignoreY) {
    //         position.y = avatar.transform.position.y;
    //         }

    //         if (randomRotation) {
    //         rotation.y = Random.Range(0, 360);
    //         }

    //         avatar.transform.eulerAngles = rotation;
    //         log.log("TASK_ROTATE\t" + avatar.name + "\t" + this.GetType().Name + "\t" + avatar.transform.localEulerAngles.ToString("f1"), 1);

    //         if (useLocalRotation) avatar.transform.localRotation = destination.transform.localRotation;
    //         else avatar.transform.rotation = destination.transform.rotation;
            
    //         avatar.transform.position = position;
            
    //         log.log("TASK_POSITION\t" + avatar.name + "\t" + this.GetType().Name + "\t" + avatar.transform.transform.position.ToString("f1"), 1);
        
    //         LM_Progress.Instance.ResumeLastPlayerPositionToNavStart(avatar.transform);

    //         // if (swap)
    //         // {
    //         // destination.transform.position = position;
    //         // if (useLocalRotation) destination.transform.localRotation = rotation;
    //         // else destination.transform.rotation = rotation;
    //         // }
	// 	}
    

	// 	// if (destination) {
            
    //     //     // 2. Try to find a component of type LM_SnapPoint in the children of the destination object
    //     //     // This looks through the parent object (e.g., "Pizza") for the child marker
    //     //     var snapPointComponent = destination.GetComponentInChildren<LM_SnapPoint>();

    //         //Transform spawnTransform;

    //         // if (snapPointComponent != null) 
    //         // {
    //         //     // If the component is found, use that child's transform
    //         //     spawnTransform = snapPointComponent.transform;
    //         //     Debug.Log("Spawning at SnapPoint found in: " + destination.name);
    //         // }
    //         // else 
    //         // {
    //         //     // Fallback: If no LM_SnapPoint is found, use the parent's transform so the game doesn't crash
    //         //     spawnTransform = destination.transform;
    //         //     Debug.LogWarning("No LM_SnapPoint found in " + destination.name + ". Using parent transform.");
    //         // }

    //         // 3. Set the position and rotation based on the SnapPoint (or fallback)
	// 		// position = spawnTransform.position;
	// 		// rotation = spawnTransform.eulerAngles;

    //         // if (ignoreY) {
    //         //     position.y = avatar.transform.position.y;
    //         // }

    //         // if (randomRotation) {
    //         //     rotation.y = Random.Range(0, 360);
    //         // }

    //         // 4. Move the player (avatar) to the determined location
    //         // avatar.transform.position = position;
    //         // avatar.transform.eulerAngles = rotation;

    //         // Physics refresh: ensure the character controller doesn't override the move
    //         Physics.SyncTransforms();
	// }

    //     // if (swap) {
    //     //     destination.transform.position = position;
    //     //     destination.transform.eulerAngles = rotation;
    //     // }
	

     public override bool updateTask () {
	     return true;
	 }
	 public override void endTask() {




	 	TASK_END();
	}

	 public override void TASK_END() {
	 	base.endTask();
	 	if ( destinations ) {

             if (canIncrementLists)
             {
                 destinations.incrementCurrent();
                 destination = destinations.currentObject();
             }
            
	 	}
	}

    // //Method to get implemented when the Block Increment boolean in the GUI is selected
    // public void BlockIncrementation()
    // {
    //     //Debug.Log("count before increment: " + count);

    //     //If the player has done # of repetitions equal to the parent task Repetition Value then set count back to 0 & move the Spawn Location to the next in the list
    //     if (count == repetition)
    //     {
    //         count = 1;
    //         destinations.incrementCurrent();
    //     }
    //     else //increment count
    //     {
    //         count++;
    //     }
    //     //Debug.Log("count after increment: " + count);
    // }
}