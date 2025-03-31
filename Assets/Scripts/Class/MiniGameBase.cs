using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class MiniGameBase : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float timeLimit = 10f;
    [SerializeField] protected float successThreshold = 0.7f;
    
    protected float currentTime;
    protected bool isRunning = false;
    protected Action<bool> onCompleted;
    
    public virtual void Initialize(float timeLimit, float successThreshold, Action<bool> onCompleted)
    {
        this.timeLimit = timeLimit;
        this.successThreshold = successThreshold;
        this.onCompleted = onCompleted;
        
        currentTime = timeLimit;
        isRunning = true;
        StartCoroutine(RunTimer());
    }
    
    protected abstract void OnSuccess();
    protected abstract void OnFailure();
    
    protected virtual IEnumerator RunTimer()
    {
        while (isRunning && currentTime > 0)
        {
            yield return null;
            currentTime -= Time.deltaTime;
            
            if (currentTime <= 0)
            {
                OnTimeUp();
            }
        }
    }
    
    protected virtual void OnTimeUp()
    {
        isRunning = false;
        OnFailure();
    }
    
    public virtual void StopGame()
    {
        isRunning = false;
        StopAllCoroutines();
    }
}