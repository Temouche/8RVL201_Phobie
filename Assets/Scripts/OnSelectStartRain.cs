using UnityEngine;

public class CityOrb : MonoBehaviour
{
    public WeatherType weatherForThisOrb;

    public void OnActivatedInspector()
    {
        if (WeatherManager.Instance == null)
        {
            Debug.Log("WeatherManager not found!");
            return;
        }

        WeatherType current = WeatherManager.Instance.GetCurrentWeather();

        if (current == weatherForThisOrb)
        {
            // si la météo actuelle est celle de l'orbe → on revient au beau temps
            WeatherManager.Instance.SetWeather(WeatherType.Clear);
        }
        else
        {
            // sinon on applique la météo de l'orbe
            WeatherManager.Instance.SetWeather(weatherForThisOrb);
        }
    }
}