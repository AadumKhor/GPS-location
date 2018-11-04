using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LocationState
{
    Disabled,
    Timeout,
    Enabled,
    Failed
}

public class GPSLocation : MonoBehaviour {
    public static int SCREEN_DENSITY;
    private GUIStyle debugStyle;
    //aprox radius of earth in km
    const float EARTH_RADIUS = 6371;
    private LocationState state;
    private float latitude;
    private float longitude;
    private float dist;

	// Use this for initialization
	IEnumerator Start () {
		if(Screen.dpi > 0f)
        {
            SCREEN_DENSITY = (int)(Screen.dpi / 160f);
        }
        else
        {
            SCREEN_DENSITY = (int)(Screen.currentResolution.height / 600);
        }

        debugStyle = new GUIStyle();
        debugStyle.fontSize = 16 * SCREEN_DENSITY;
        debugStyle.normal.textColor = Color.white;

        state = LocationState.Disabled;
        latitude = 0f;
        longitude = 0f;
        dist = 0f;

        if (Input.location.isEnabledByUser)
        {
            Input.location.Start();
            int waitTime = 15;
            while(Input.location.status == LocationServiceStatus.Initializing && waitTime == 0)
            {
                yield return new WaitForSeconds(1);
                waitTime--;

            }
            if (waitTime == 0)
            {
                state = LocationState.Timeout;
            }
            else if(Input.location.status == LocationServiceStatus.Failed)
            {
                state = LocationState.Failed;
            }
            else
            {
                state = LocationState.Enabled;
                latitude = Input.location.lastData.latitude;
                longitude = Input.location.lastData.longitude;
            }
        }

	}

    IEnumerator OnApplicationPause(bool pauseState)
    {
        if (pauseState)
        {
            Input.location.Stop();
            state = LocationState.Disabled;

        }
        else
        {
            Input.location.Start();
            int waitTime = 15;
            while (Input.location.status == LocationServiceStatus.Initializing && waitTime == 0)
            {
                yield return new WaitForSeconds(1);
                waitTime--;
            }
            if (waitTime == 0)
            {
                state = LocationState.Timeout;
            }
            else if (Input.location.status == LocationServiceStatus.Failed)
            {
                state = LocationState.Failed;
            }
            else
            {
                state = LocationState.Enabled;
                latitude = Input.location.lastData.latitude;
                longitude = Input.location.lastData.longitude;
            }
        }
    }

     void OnGUI()
    {
        Rect guiBox = new Rect(40, 20, Screen.width - 80, Screen.height - 40);

        GUI.skin.box.fontSize = 32 * SCREEN_DENSITY;
        GUI.Box(guiBox, "GPS Demo");

        float buttonHeight = guiBox.height / 7;

        switch (state)
        {
            case LocationState.Enabled:
                GUILayout.Label("Latitude" + latitude.ToString(), debugStyle, GUILayout.Width(Screen.width));
                GUILayout.Label("Longitude" + longitude.ToString(), debugStyle, GUILayout.Height(Screen.height));

                Rect distRectBox = new Rect(guiBox.x + 40, guiBox.y + guiBox.height, guiBox.width - 80, buttonHeight);
                GUI.skin.label.fontSize = SCREEN_DENSITY * 40;
                GUI.skin.label.alignment = TextAnchor.UpperCenter;
                GUI.Label(distRectBox, "Distance :" + dist.ToString() + "m");
                break;

            case LocationState.Disabled:
                Rect disRectBox = new Rect(guiBox.x + 40, guiBox.y * 2, guiBox.width - 80, buttonHeight *2);
                GUI.skin.label.fontSize = SCREEN_DENSITY * 40;
                GUI.skin.label.alignment = TextAnchor.UpperCenter;
                GUI.Label(disRectBox, "GPS disabled, turn on location service!");
                break;

            case LocationState.Failed:
                Rect faiiledRectBox = new Rect(guiBox.x + 40, guiBox.y + guiBox.height, guiBox.width - 80, buttonHeight*2);
                GUI.skin.label.fontSize = SCREEN_DENSITY * 40;
                GUI.skin.label.alignment = TextAnchor.UpperCenter;
                GUI.Label(faiiledRectBox, "Location services failed!");
                break;

            case LocationState.Timeout:
                Rect timeRectBox = new Rect(guiBox.x + 40, guiBox.y + guiBox.height, guiBox.width - 80, buttonHeight*2);
                GUI.skin.label.fontSize = SCREEN_DENSITY * 40;
                GUI.skin.label.alignment = TextAnchor.UpperCenter;
                GUI.Label(timeRectBox,"Location services TimedOut!");
                break;
        }
    }

    //Haversine Formula to Calculate distance,
    //bearing and more between points
    //based on latitude and lognitude

        float Haversine(ref float lastLatitude , ref float lastLongitude)
    {
        float newLatitude = Input.location.lastData.latitude;
        float newLongitude = Input.location.lastData.longitude;
        float deltaLatitude = (newLatitude = lastLatitude) * Mathf.Deg2Rad;
        float deltaLongitude = (newLongitude - lastLongitude) * Mathf.Deg2Rad;
        float a = Mathf.Pow((Mathf.Sin(deltaLatitude / 2)), 2) + Mathf.Cos(lastLatitude * Mathf.Deg2Rad) * Mathf.Cos(newLatitude * Mathf.Deg2Rad) * Mathf.Pow(Mathf.Sin(deltaLongitude / 2), 2);
        lastLatitude = newLatitude;
        lastLongitude = newLongitude;
        float c = Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        return EARTH_RADIUS * c;
    }

    // Update is called once per frame
    void Update () {
		if(state == LocationState.Enabled)
        {
            float deltaDist = Haversine(ref latitude, ref longitude) * 1000f;

            if(deltaDist > 0)
            {
                dist += deltaDist;
            }
        }
	}
}
