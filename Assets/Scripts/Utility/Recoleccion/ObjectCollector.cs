using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class ObjectCollector : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float raycastDistance = 10f;
    public LayerMask collectableLayer;

    [Header("UI Fill Circle")]
    public RawImage fillCircle;
    public float fillSpeed = 1f;
    public float fillThreshold = 1f;
    
    private Material fillMaterialInstance;

    [Header("Audio Settings")]
    public AudioClip collectSound;

    [Header("Effects Settings")]
    public GameObject collectParticlePrefab;
    public float particleLifetime = 2f;

    private GameObject lastHitObject;
    private float fillAmount = 0f;
    private bool isFilling = false;
    private bool audioPlayed = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (fillCircle != null)
        {
            // Instanciar material para NO modificar el material original
            fillMaterialInstance = Instantiate(fillCircle.material);
            fillCircle.material = fillMaterialInstance;
            fillCircle.gameObject.SetActive(false);
            fillMaterialInstance.SetFloat("_FillAmount", 0f);
        }
    }

    void Update()
    {
        HandleRaycast();
    }

    void HandleRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, collectableLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Evitar interacción con objetos ya recolectados
            Collectable c = hitObject.GetComponent<Collectable>();
            if (c != null && CollectableManager.Instance != null &&
                CollectableManager.Instance.IsCollected(c.id))
            {
                ResetFillProgress();
                return;
            }

            if (hitObject != lastHitObject)
            {
                ResetFillProgress();
                lastHitObject = hitObject;
                audioPlayed = false;
                isFilling = true;
            }

            if (isFilling)
            {
                FillCircle();

                if (fillAmount >= fillThreshold && !audioPlayed)
                {
                    TryCollect(hitObject);
                    audioPlayed = true;
                }
            }
        }
        else
        {
            ResetFillProgress();
        }
    }

    void FillCircle()
    {
        fillAmount += fillSpeed * Time.deltaTime;
        fillAmount = Mathf.Clamp01(fillAmount);

        if (fillCircle != null)
        {
            if (!fillCircle.gameObject.activeSelf)
                fillCircle.gameObject.SetActive(true);

            fillMaterialInstance.SetFloat("_FillAmount", fillAmount);
        }
    }

    void ResetFillProgress()
    {
        fillAmount = 0f;

        if (fillCircle != null)
        {
            fillMaterialInstance.SetFloat("_FillAmount", 0f);
            fillCircle.gameObject.SetActive(false);
        }

        lastHitObject = null;
        isFilling = false;
    }

    void TryCollect(GameObject obj)
    {
        if (obj == null) return;

        PlayCollectSound();

        Collectable c = obj.GetComponent<Collectable>();
        if (c != null)
        {
            // Lógica del Collectable: HUD, partículas y Destroy
            c.Collect(collectParticlePrefab, particleLifetime);
        }
        else
        {
            if (collectParticlePrefab != null)
            {
                GameObject part = Instantiate(collectParticlePrefab, obj.transform.position, Quaternion.identity);
                Destroy(part, particleLifetime);
            }
            Destroy(obj);
        }

        ResetFillProgress();
    }

    void PlayCollectSound()
    {
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
    }

    void OnDrawGizmos()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, raycastDistance, collectableLayer))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, hit.point);
            Gizmos.DrawSphere(hit.point, 0.05f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * raycastDistance);
        }
    }
}
