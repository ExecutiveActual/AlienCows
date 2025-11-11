using System.Collections;
using UnityEngine;

public class NightEnd_ : MonoBehaviour
{
    [Tooltip("Reference to the WorldClock_ script in the scene.")]
    public WorldClock_ worldClock;

    [Tooltip("Speed multiplier for time fast-forward effect (higher = faster skip).")]
    public float fastForwardSpeed = 30f;

    private bool isSkipping = false;

    public void SleepTillMorning()
    {
        if (worldClock == null)
        {
            Debug.LogError("NightEnd_: No WorldClock_ assigned!");
            return;
        }

        if (worldClock.isDay)
        {
            Debug.Log("NightEnd_: It's already daytime.");
            return;
        }

        StartCoroutine(FastForwardToMorning());
    }

    private IEnumerator FastForwardToMorning()
    {
        if (isSkipping) yield break;
        isSkipping = true;

        float totalCycleSeconds = worldClock.totalCycleMinutes * 60f;
        float daySeconds = worldClock.dayMinutes * 60f;

        Debug.Log("NightEnd_: Fast forwarding to morning...");

        while (worldClock.currentTime < daySeconds)
        {
            worldClock.currentTime += Time.deltaTime * fastForwardSpeed;
            worldClock.ForceUpdateVisuals();
            yield return null;
        }

        worldClock.currentTime = 0f;
        worldClock.isDay = true;
        worldClock.transitioningToNight = false;

        if (worldClock.messageText != null)
        {
            worldClock.messageText.gameObject.SetActive(false);
            worldClock.messageText.text = "";
        }

        Debug.Log("NightEnd_: Morning reached successfully!");
        isSkipping = false;
    }
}
