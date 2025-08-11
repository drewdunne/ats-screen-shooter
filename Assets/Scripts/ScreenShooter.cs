using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShooter : MonoBehaviour
{
    [SerializeField]
    public GameObject BulletHole;

    [SerializeField]
    [Tooltip("Optional: Parent transform for bullet holes. If set, bullet holes will be instantiated as children of this transform instead of the pool.")]
    public Transform BulletHoleParent;

    private GameObject bulletHolePool;

    public void CreateShot(Vector2 screenPoint) {
        Debug.Log($"[BulletHole Debug] CreateShot called at screen point: {screenPoint}");
        
        // Check current app mode
        AppModeManager modeManager = FindObjectOfType<AppModeManager>();
        if (modeManager != null)
        {
            Debug.Log($"[BulletHole Debug] Current Mode: {modeManager.GetCurrentMode()}");
        }
        
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        Debug.Log($"[BulletHole Debug] Ray origin: {ray.origin}, direction: {ray.direction}");

        // Check what we SHOULD be hitting - find all objects along the ray
        RaycastHit[] allHits = Physics.RaycastAll(ray, 100f);
        Debug.Log($"[BulletHole Debug] RaycastAll found {allHits.Length} objects along ray:");
        foreach (var testHit in allHits)
        {
            Debug.Log($"  - {testHit.collider.gameObject.name} at distance {testHit.distance}, layer: {testHit.collider.gameObject.layer}");
        }
        
        // Check specifically for B27 target
        GameObject b27 = GameObject.Find("B27 Paper Target w Stand 6.5ft version");
        if (b27 != null)
        {
            Debug.Log($"[BulletHole Debug] B27 Target found: Active={b27.activeInHierarchy}, ActiveSelf={b27.activeSelf}, Position={b27.transform.position}");
            
            // Check parent GameObjects
            Transform parent = b27.transform.parent;
            if (parent != null)
            {
                Debug.Log($"[BulletHole Debug] B27 Parent: {parent.name}, Active={parent.gameObject.activeInHierarchy}");
            }
            
            // Get colliders including inactive ones
            Collider[] b27Colliders = b27.GetComponentsInChildren<Collider>(true); // true = include inactive
            Debug.Log($"[BulletHole Debug] B27 has {b27Colliders.Length} colliders in children (including inactive)");
            foreach (var col in b27Colliders)
            {
                Debug.Log($"  - {col.gameObject.name}: Enabled={col.enabled}, Layer={col.gameObject.layer}, Active={col.gameObject.activeInHierarchy}, IsTrigger={col.isTrigger}");
                if (col is MeshCollider mc)
                {
                    Debug.Log($"    MeshCollider details: Convex={mc.convex}, HasMesh={mc.sharedMesh != null}");
                    if (mc.sharedMesh != null)
                    {
                        Debug.Log($"    Mesh name: {mc.sharedMesh.name}");
                    }
                }
                else if (col is BoxCollider bc)
                {
                    Debug.Log($"    BoxCollider size: {bc.size}, center: {bc.center}");
                }
            }
        }
        else
        {
            Debug.LogError("[BulletHole Debug] B27 Paper Target NOT FOUND!");
        }

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit)) {
            Debug.Log($"[BulletHole Debug] Raycast HIT! Object: {hit.collider.gameObject.name}, Point: {hit.point}, Normal: {hit.normal}");
            Debug.Log($"[BulletHole Debug] Hit layer: {hit.collider.gameObject.layer}, Tag: {hit.collider.gameObject.tag}");
            
            // Check if we hit a ReactiveTarget
            ReactiveTarget target = hit.collider.GetComponentInParent<ReactiveTarget>();
            if (target != null)
            {
                Debug.Log($"[BulletHole Debug] Hit a ReactiveTarget, calling OnHit");
                target.OnHit(hit.point, hit.normal);
            }
            else
            {
                Debug.Log($"[BulletHole Debug] Did NOT hit a ReactiveTarget (this is fine for Qualification targets)");
            }
            
            // Parent bullet holes to the hit object so they move with it
            Transform parentTransform = null;
            
            // Check if we hit a target that should be the parent
            if (hit.collider.name == "Body1" || hit.collider.gameObject.name.Contains("Target"))
            {
                // Parent to the actual target object
                parentTransform = hit.collider.transform;
                Debug.Log($"[BulletHole Debug] Parenting bullet hole to hit target: {hit.collider.name}");
            }
            else if (BulletHoleParent != null)
            {
                // Use the manually set parent if available
                parentTransform = BulletHoleParent;
                Debug.Log($"[BulletHole Debug] Using BulletHoleParent: {BulletHoleParent.name}");
            }
            else
            {
                // Fall back to the pool
                parentTransform = bulletHolePool.transform;
                Debug.Log($"[BulletHole Debug] Using bulletHolePool (no target hit)");
            }
            
            if (BulletHole != null)
            {
                GameObject bulletHoleInstance = Instantiate(
                    BulletHole,
                    hit.point + hit.normal * .01f,
                    Quaternion.FromToRotation(Vector3.up, hit.normal),
                    parentTransform
                );
                Debug.Log($"[BulletHole Debug] Bullet hole created: {bulletHoleInstance.name} at position {bulletHoleInstance.transform.position}");
            }
            else
            {
                Debug.LogError("[BulletHole Debug] BulletHole prefab is NULL! Cannot create bullet hole!");
            }
        }
        else
        {
            Debug.LogWarning($"[BulletHole Debug] Raycast MISSED - no object hit");
        }
    }

    public void ClearBulletHoles() {
        // Clear bullet holes from the pool
        foreach (Transform transform in bulletHolePool.transform) {
            UnityEngine.Object.Destroy(transform.gameObject);
        }
        
        // Also clear bullet holes from the parent if set
        if (BulletHoleParent != null) {
            // Find all BulletHole children and destroy them
            foreach (Transform child in BulletHoleParent) {
                if (child.name.Contains("BulletHole")) {
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        bulletHolePool = new GameObject();
        bulletHolePool.name = "BulletHolePool";
        
        // Debug check for BulletHole prefab
        if (BulletHole == null)
        {
            Debug.LogError("[BulletHole Debug] BulletHole prefab is NOT assigned in ScreenShooter Inspector!");
        }
        else
        {
            Debug.Log($"[BulletHole Debug] BulletHole prefab is assigned: {BulletHole.name}");
        }
        
        if (BulletHoleParent != null)
        {
            Debug.Log($"[BulletHole Debug] BulletHoleParent is set to: {BulletHoleParent.name}");
        }
        else
        {
            Debug.Log("[BulletHole Debug] BulletHoleParent is not set, using bulletHolePool");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
