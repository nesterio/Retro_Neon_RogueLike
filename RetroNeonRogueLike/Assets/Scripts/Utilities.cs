using System;
using System.Collections;
using UnityEngine;

public static class Utilities
{
    public static void RunAsync(Action action, bool waitForNextFrame = true, float timeToWait = 0f) 
        => MainManager.Instance.StartCoroutine
            (RunAsyncEnumerator(action, waitForNextFrame, timeToWait));

    private static IEnumerator RunAsyncEnumerator(Action action, bool waitForNextFrame = true, float timeToWait = 0f)
    {
        if (waitForNextFrame)
            yield return new WaitForEndOfFrame();

        if (timeToWait != 0f)
            yield return new WaitForSeconds(timeToWait);
        
        action?.Invoke();
    }
}
