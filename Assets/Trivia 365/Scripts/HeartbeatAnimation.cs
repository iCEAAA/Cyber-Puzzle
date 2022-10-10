using System.Collections;
using UnityEngine;
#if USE_DOTWEEN
using DG.Tweening;
#endif

public class HeartbeatAnimation : MonoBehaviour {

	#if USE_DOTWEEN

	public Vector3 MinimumScale = new Vector3(0.8f, 0.8f, 0.8f);

	public Vector3 MaximumScale = new Vector3 (1, 1, 1);

	public float DurationInSecs = 1;

	Coroutine co;
	Transform thisTransform;

	void OnEnable ()
	{
		thisTransform = transform;
		co = StartCoroutine (ScaleAnimation ());
	}

	private IEnumerator ScaleAnimation()
	{
		thisTransform.DOScale (MinimumScale, DurationInSecs).OnComplete(()=>{
			thisTransform.DOScale(MaximumScale, DurationInSecs).OnComplete(()=>{
				if(gameObject.activeInHierarchy) co = StartCoroutine (ScaleAnimation ());
				else StopCoroutine(co);
			});
		});

		yield return null;
	}
	
	void OnDisable () 
	{
		StopCoroutine (co);

		thisTransform.localScale = Vector3.one;
	}
	#endif
}