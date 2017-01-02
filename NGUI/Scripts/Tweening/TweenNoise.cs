//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2014 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;

/// <summary>
/// Tween the object's local scale.
/// </summary>

[AddComponentMenu("NGUI/Tween/Tween Noise")]
public class TweenNoise : UITweener
{
    public float _amplitudeX = 0f;
    public float _frequencyX = 0f;
    float _phaseX = 0f;
    public float _amplitudeY = 0f;
    public float _frequencyY = 0f;
    float _phaseY = 0f;
    Vector3 _startPos = Vector3.zero;


	public Vector3 from = Vector3.one;
	public Vector3 to = Vector3.one;
	public bool updateTable = false;

	Transform mTrans;
	UITable mTable;

	public Transform cachedTransform { get { if (mTrans == null) mTrans = transform; return mTrans; } }

	public Vector3 value { get { return cachedTransform.localScale; } set { cachedTransform.localScale = value; } }

	[System.Obsolete("Use 'value' instead")]
	public Vector3 scale { get { return this.value; } set { this.value = value; } }
    void Awake()
    {
        _phaseX = Random.value;
        _phaseY = Random.value;
    }
	/// <summary>
	/// Tween the value.
	/// </summary>

	protected override void OnUpdate (float factor, bool isFinished)
	{
	/*	value = from * (1f - factor) + to * factor;

		if (updateTable)
		{
			if (mTable == null)
			{
				mTable = NGUITools.FindInParents<UITable>(gameObject);
				if (mTable == null) { updateTable = false; return; }
			}
			mTable.repositionNow = true;
		}
        */


        factor = factor * duration;

        Vector3 value = Vector3.zero;
        if (factor <= duration)
        {
            float result = Mathf.Sin(2.0f * 3.14159f * factor * _frequencyX + _phaseX) * _amplitudeX;
            value.x = result * (duration - factor) / duration;
        }
        if (factor <= duration)
        {
            float result = Mathf.Sin(2.0f * 3.14159f * factor * _frequencyY + _phaseY) * _amplitudeY;
            value.y = result * (duration - factor) / duration;
        }

        cachedTransform.localPosition = _startPos + value;
	}

	/// <summary>
	/// Start the tweening operation.
	/// </summary>

	static public TweenScale Begin (GameObject go, float duration, Vector3 scale)
	{
		TweenScale comp = UITweener.Begin<TweenScale>(go, duration);
		comp.from = comp.value;
		comp.to = scale;

		if (duration <= 0f)
		{
			comp.Sample(1f, true);
			comp.enabled = false;
		}
		return comp;
	}

	[ContextMenu("Set 'From' to current value")]
	public override void SetStartToCurrentValue () { from = value; }

	[ContextMenu("Set 'To' to current value")]
	public override void SetEndToCurrentValue () { to = value; }

	[ContextMenu("Assume value of 'From'")]
	void SetCurrentValueToStart () { value = from; }

	[ContextMenu("Assume value of 'To'")]
	void SetCurrentValueToEnd () { value = to; }
}
