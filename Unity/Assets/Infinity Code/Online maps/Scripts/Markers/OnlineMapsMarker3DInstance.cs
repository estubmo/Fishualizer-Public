/*     INFINITY CODE 2013-2015      */
/*   http://www.infinity-code.com   */

using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 3D marker instance class.
/// </summary>
[Serializable]
[AddComponentMenu("")]
public class OnlineMapsMarker3DInstance : OnlineMapsMarkerInstanceBase
{
    private Vector2 _position;
    private float _scale;

    private int lastTouchCount;
    private bool isPressed;

    private long[] lastClickTimes = { 0, 0 };
    private IEnumerator longPressEnumenator;

    private void Awake()
    {
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
        Collider cl = collider;
#else
        Collider cl = GetComponent<Collider>();
#endif

        if (cl == null) cl  = gameObject.AddComponent<BoxCollider>();
        cl.isTrigger = true;
    }

    private void OnPress()
    {
        OnlineMapsControlBase.instance.InvokeBasePress();

        isPressed = true;
        if (marker.OnPress != null) marker.OnPress(marker);

        lastClickTimes[0] = lastClickTimes[1];
        lastClickTimes[1] = DateTime.Now.Ticks;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            OnlineMapsControlBase.instance.dragMarker = marker;
            OnlineMapsControlBase.instance.isMapDrag = false;
        }

        longPressEnumenator = WaitLongPress();
        StartCoroutine("WaitLongPress");
    }

    private void OnRelease()
    {
        OnlineMapsControlBase.instance.InvokeBaseRelease();

        if (marker.OnRelease != null) marker.OnRelease(marker);

        if (longPressEnumenator != null)
        {
            StopCoroutine("WaitLongPress");
            longPressEnumenator = null;
        }

        OnlineMapsControlBase.instance.dragMarker = null;

        if (!isPressed) return;

        if (DateTime.Now.Ticks - lastClickTimes[0] < 5000000)
        {
            if (marker.OnDoubleClick != null) marker.OnDoubleClick(marker);
            lastClickTimes[0] = 0;
            lastClickTimes[1] = 0;
        }
        else if (marker.OnClick != null) marker.OnClick(marker);
    }

    private void Start()
    {
        _position = marker.position;
        _scale = marker.scale;
        transform.localScale = new Vector3(_scale, _scale, _scale);
    }

    private void Update()
    {
        UpdateBaseProps();
        UpdateDefaultMarkerEvens();
    }

    private void UpdateBaseProps()
    {
        if (_position != marker.position)
        {
            OnlineMaps map = OnlineMaps.instance;

            double tlx, tly, brx, bry;
            map.GetTopLeftPosition(out tlx, out tly);
            map.GetBottomRightPosition(out brx, out bry);

            marker.Update(tlx, tly, brx, bry, map.zoom);
        }

        if (_scale != marker.scale)
        {
            _scale = marker.scale;
            transform.localScale = new Vector3(_scale, _scale, _scale);
        }
    }

    private void UpdateDefaultMarkerEvens()
    {

		return;
		if (!(marker as OnlineMapsMarker3D).allowDefaultMarkerEvents) return;

        int touchCount = 0;

#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        touchCount = Input.touchCount;
#else
        touchCount = Input.GetMouseButton(0) ? 1 : 0;
#endif
        if (lastTouchCount == touchCount) return;

        RaycastHit hitInfo;
        bool hit = GetComponent<Collider>().Raycast(OnlineMapsControlBase3D.instance.activeCamera.ScreenPointToRay(Input.mousePosition), out hitInfo, OnlineMapsUtils.maxRaycastDistance);

        if (touchCount == 0)
        {
            if (hit) OnRelease();
            isPressed = false;
        }
        else if (hit)
        {
            OnPress();
        }

        lastTouchCount = touchCount;
    }

    private IEnumerator WaitLongPress()
    {
        yield return new WaitForSeconds(OnlineMapsControlBase.longPressDelay);

        if (marker.OnLongPress != null) marker.OnLongPress(marker);
        longPressEnumenator = null;
    }
}