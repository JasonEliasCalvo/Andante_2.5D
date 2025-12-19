using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RespawnManager : MonoBehaviour
{
    [Header("Respawn")]
    public List<Transform> respawnPoints;
    public GameObject player;
    private Transform lastCheckpoint;

    [Header("Respawn & Effects")]
    public AudioClip respawnSound;
    public GameObject respawnEffect;

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;  
    public float fadeDuration = 1f;

    private void Start()
    {
        if (respawnPoints.Count > 0)
        {
            lastCheckpoint = respawnPoints[0];
        }
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        lastCheckpoint = checkpoint; 
    }

    public void Respawn()
    {
        if (lastCheckpoint != null)
        {
            StartCoroutine(RespawnWithEffect());
        }
        else
        {
            Debug.LogWarning("No hay un checkpoint registrado.");
        }
    }

    private IEnumerator RespawnWithEffect()
    {
        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeOut()); 
        }

        yield return new WaitForSeconds(0.5f);

        if (respawnEffect != null)
        {
            GameObject effect = Instantiate(respawnEffect, lastCheckpoint.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        player.transform.position = new Vector3(lastCheckpoint.position.x, lastCheckpoint.position.y + 2f, lastCheckpoint.position.z);
        player.transform.rotation = lastCheckpoint.rotation;

        if (respawnSound != null)
        {
            //AudioSource.PlayClipAtPoint(respawnSound, lastCheckpoint.position);
        }

        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(FadeIn());
        }
    }

    private IEnumerator ShowFadeIn()
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(FadeIn());
    }

    public void ShowFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1;
    }

    private IEnumerator FadeIn()
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0;
    }
}
