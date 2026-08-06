using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonoBehaviour.Invoke/CancelInvoke の置き換え。
/// キー単位で遅延実行を管理し、同じキーの予約は自動的に上書き(前のものはキャンセル)される。
/// 文字列(nameof)ではなくActionを直接渡すため、リネーム時はコンパイルエラーで検知できる。
/// </summary>
public class CustomerActionTimer
{
    private readonly MonoBehaviour runner;
    private readonly Dictionary<string, Coroutine> activeTimers = new();

    public CustomerActionTimer(MonoBehaviour runner)
    {
        this.runner = runner;
    }

    /// <summary>
    /// key に紐づく遅延実行を予約する。同じkeyで既に予約中のものがあればキャンセルしてから予約し直す。
    /// </summary>
    public void Schedule(string key, float delay, Action action)
    {
        Cancel(key);
        activeTimers[key] = runner.StartCoroutine(RunAfterDelay(key, delay, action));
    }

    /// <summary>指定したkeyの予約をキャンセルする。</summary>
    public void Cancel(string key)
    {
        if (activeTimers.TryGetValue(key, out var co) && co != null)
        {
            runner.StopCoroutine(co);
        }
        activeTimers.Remove(key);
    }

    /// <summary>全ての予約をキャンセルする(オブジェクト破棄時などに呼ぶ)。</summary>
    public void CancelAll()
    {
        foreach (var co in activeTimers.Values)
        {
            if (co != null) runner.StopCoroutine(co);
        }
        activeTimers.Clear();
    }

    private IEnumerator RunAfterDelay(string key, float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        activeTimers.Remove(key);
        action?.Invoke();
    }
}