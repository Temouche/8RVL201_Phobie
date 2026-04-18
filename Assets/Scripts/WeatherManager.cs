using System.Collections;
using UnityEngine;

public enum WeatherType
{
    Clear,
    Rain,
    Snow,
    Fog,
    Storm // optionnel : pluie + vent fort / orage
}

public class WeatherManager : MonoBehaviour
{
    public static WeatherManager Instance { get; private set; }

    [Header("Particle Systems (assigner des prefabs/objets)")]
    public ParticleSystem rainPS;
    public ParticleSystem snowPS;
    public ParticleSystem stormPS; // optionnel (heavy rain + particles)
    // NOTE: on pilote l'émission via rateOverTime

    [Header("Audio")]
    public AudioSource rainAudio;
    public AudioSource snowAudio;
    public AudioSource stormAudio;
    public AudioSource ambientClearAudio; // optionnel pour ambiance clair

    [Header("Fog")]
    public bool useUnityFog = true; // si true use RenderSettings.fogDensity
    public float fogClearDensity = 0.0005f;
    public float fogRainDensity = 0.02f;
    public float fogSnowDensity = 0.015f;
    public float fogOnlyDensity = 0.025f; // pour "Fog" weather

    // Alternative : fake fog using a material (cube inside view)
    public bool useFakeFogMaterial = false;
    public Material fakeFogMaterial; // material du cube qui fait le fog
    public string fakeFogColorProperty = "_Color"; // property contenant alpha (si autre shader, adapte)
    public float fakeFogClearAlpha = 0.02f;
    public float fakeFogRainAlpha = 0.25f;
    public float fakeFogSnowAlpha = 0.18f;

    [Header("Lighting")]
    public Light directionalLight;
    public float lightClearIntensity = 1.0f;
    public float lightRainIntensity = 0.2f;
    public float lightSnowIntensity = 0.7f;
    public Color lightClearColor = new Color(0.96f, 0.79f, 0.54f, 1f);
    public Color lightOvercastColor = new Color(0.8f, 0.85f, 1f);

    [Header("Transition settings")]
    public float transitionDuration = 1.0f; // durée par défaut des fades
    public float audioFadeSpeed = 1.0f;

    // internal state
    WeatherType currentWeather = WeatherType.Clear;
    Coroutine transitionRoutine = null;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Ensure initial states
        if (rainPS != null) { rainPS.Stop(); SetEmission(rainPS, 0f); }
        if (snowPS != null) { snowPS.Stop(); SetEmission(snowPS, 0f); }
        if (stormPS != null) { stormPS.Stop(); SetEmission(stormPS, 0f); }

        if (rainAudio != null) rainAudio.volume = 0f;
        if (snowAudio != null) snowAudio.volume = 0f;
        if (stormAudio != null) stormAudio.volume = 0f;
        if (ambientClearAudio != null) ambientClearAudio.volume = 1f;

        if (useUnityFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogDensity = fogClearDensity;
        }
        else if (useFakeFogMaterial && fakeFogMaterial != null)
        {
            SetMaterialAlpha(fakeFogMaterial, fakeFogClearAlpha);
        }

        if (directionalLight != null)
        {
            directionalLight.intensity = lightClearIntensity;
            directionalLight.color = lightClearColor;
        }

        currentWeather = WeatherType.Clear;
    }

    // Public API ---------------------------------------------------------

    public WeatherType GetCurrentWeather() => currentWeather;

    public void ToggleWeather(WeatherType weather)
    {
        // toggle: si on est déjà dans ce weather -> clear, sinon set to it
        if (currentWeather == weather)
            SetWeather(WeatherType.Clear, transitionDuration);
        else
            SetWeather(weather, transitionDuration);
    }

    public void SetWeather(WeatherType target, float duration = -1f)
    {
        if (duration <= 0f) duration = transitionDuration;
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(TransitionToWeather(target, duration));
    }

    public void CycleWeather(float duration = -1f)
    {
        // simple cycle order
        WeatherType next = currentWeather == WeatherType.Clear ? WeatherType.Rain :
                           currentWeather == WeatherType.Rain ? WeatherType.Snow :
                           currentWeather == WeatherType.Snow ? WeatherType.Fog :
                           currentWeather == WeatherType.Fog ? WeatherType.Storm :
                           WeatherType.Clear;
        SetWeather(next, duration);
    }

    // Core transition coroutine -----------------------------------------
    IEnumerator TransitionToWeather(WeatherType target, float duration)
    {
        WeatherType startWeather = currentWeather;
        currentWeather = target; // set early so other calls know intention

        // Capture initial values
        float startFog = useUnityFog ? RenderSettings.fogDensity : GetMaterialAlpha(fakeFogMaterial, fakeFogClearAlpha);
        float targetFog = fogClearDensity;
        float startLightInt = directionalLight != null ? directionalLight.intensity : 0f;
        float targetLightInt = lightClearIntensity;
        Color startLightColor = directionalLight != null ? directionalLight.color : lightClearColor;
        Color targetLightColor = lightClearColor;

        // target emission rates and audio vols
        float rainStart = GetEmissionRate(rainPS), rainTarget = 0f;
        float snowStart = GetEmissionRate(snowPS), snowTarget = 0f;
        float stormStart = GetEmissionRate(stormPS), stormTarget = 0f;

        float rainVolStart = rainAudio != null ? rainAudio.volume : 0f, rainVolTarget = 0f;
        float snowVolStart = snowAudio != null ? snowAudio.volume : 0f, snowVolTarget = 0f;
        float stormVolStart = stormAudio != null ? stormAudio.volume : 0f, stormVolTarget = 0f;
        float ambientStart = ambientClearAudio != null ? ambientClearAudio.volume : 0f, ambientTarget = 1f;

        // decide targets by weather
        switch (target)
        {
            case WeatherType.Clear:
                targetFog = fogClearDensity;
                targetLightInt = lightClearIntensity;
                targetLightColor = lightClearColor;
                rainTarget = 0f; snowTarget = 0f; stormTarget = 0f;
                rainVolTarget = 0f; snowVolTarget = 0f; stormVolTarget = 0f;
                ambientTarget = 1f;
                break;
            case WeatherType.Rain:
                targetFog = fogRainDensity;
                targetLightInt = lightRainIntensity;
                targetLightColor = lightOvercastColor;
                rainTarget = 200f; // emission rate, ajuster selon ton PS
                snowTarget = 0f; stormTarget = 0f;
                rainVolTarget = 1f; snowVolTarget = 0f; stormVolTarget = 0f;
                ambientTarget = 0f;
                break;
            case WeatherType.Snow:
                targetFog = fogSnowDensity;
                targetLightInt = lightSnowIntensity;
                targetLightColor = lightOvercastColor;
                rainTarget = 0f; snowTarget = 120f; stormTarget = 0f;
                rainVolTarget = 0f; snowVolTarget = 1f; stormVolTarget = 0f;
                ambientTarget = 0f;
                break;
            case WeatherType.Fog:
                targetFog = fogOnlyDensity;
                targetLightInt = lightRainIntensity * 0.9f;
                targetLightColor = lightOvercastColor;
                rainTarget = 0f; snowTarget = 0f; stormTarget = 0f;
                rainVolTarget = 0f; snowVolTarget = 0f; stormVolTarget = 0f;
                ambientTarget = 0f;
                break;
            case WeatherType.Storm:
                targetFog = fogRainDensity * 1.2f;
                targetLightInt = lightRainIntensity * 0.6f;
                targetLightColor = new Color(0.7f, 0.75f, 0.95f);
                rainTarget = 450f; stormTarget = 200f; snowTarget = 0f;
                rainVolTarget = 1f; stormVolTarget = 1f; snowVolTarget = 0f;
                ambientTarget = 0f;
                break;
        }

        // Ensure particle systems are playing (so we can animate emission)
        EnsurePSPlaying(rainPS); EnsurePSPlaying(snowPS); EnsurePSPlaying(stormPS);

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / duration);

            // Fog
            float fogNow = Mathf.Lerp(startFog, targetFog, alpha);
            if (useUnityFog)
                RenderSettings.fogDensity = fogNow;
            else if (useFakeFogMaterial && fakeFogMaterial != null)
            {
                float clearA = fakeFogClearAlpha;
                float targetA = target == WeatherType.Rain ? fakeFogRainAlpha :
                                target == WeatherType.Snow ? fakeFogSnowAlpha : fakeFogClearAlpha;
                float aNow = Mathf.Lerp(GetMaterialAlpha(fakeFogMaterial, clearA), targetA, alpha);
                SetMaterialAlpha(fakeFogMaterial, aNow);
            }

            // Light
            if (directionalLight != null)
            {
                directionalLight.intensity = Mathf.Lerp(startLightInt, targetLightInt, alpha);
                directionalLight.color = Color.Lerp(startLightColor, targetLightColor, alpha);
            }

            // Particles emission
            SetEmission(rainPS, Mathf.Lerp(rainStart, rainTarget, alpha));
            SetEmission(snowPS, Mathf.Lerp(snowStart, snowTarget, alpha));
            SetEmission(stormPS, Mathf.Lerp(stormStart, stormTarget, alpha));

            // Audio volumes
            FadeAudio(rainAudio, Mathf.Lerp(rainVolStart, rainVolTarget, alpha));
            FadeAudio(snowAudio, Mathf.Lerp(snowVolStart, snowVolTarget, alpha));
            FadeAudio(stormAudio, Mathf.Lerp(stormVolStart, stormVolTarget, alpha));
            FadeAudio(ambientClearAudio, Mathf.Lerp(ambientStart, ambientTarget, alpha));

            yield return null;
        }

        // finalize targets
        if (useUnityFog) RenderSettings.fogDensity = targetFog;
        if (useFakeFogMaterial && fakeFogMaterial != null)
        {
            float finalA = target == WeatherType.Rain ? fakeFogRainAlpha :
                           target == WeatherType.Snow ? fakeFogSnowAlpha :
                           fakeFogClearAlpha;
            SetMaterialAlpha(fakeFogMaterial, finalA);
        }

        SetEmission(rainPS, rainTarget);
        SetEmission(snowPS, snowTarget);
        SetEmission(stormPS, stormTarget);

        FadeAudio(rainAudio, rainVolTarget);
        FadeAudio(snowAudio, snowVolTarget);
        FadeAudio(stormAudio, stormVolTarget);
        FadeAudio(ambientClearAudio, ambientTarget);

        // If particle emission is zero, stop system to save perf
        StopPSIfZero(rainPS);
        StopPSIfZero(snowPS);
        StopPSIfZero(stormPS);

        transitionRoutine = null;
        yield break;
    }

    // Helpers -----------------------------------------------------------
    void EnsurePSPlaying(ParticleSystem ps)
    {
        if (ps == null) return;
        var em = ps.emission;
        // if emission is zero but not playing, start it
        if (!ps.isPlaying) ps.Play();
    }

    void StopPSIfZero(ParticleSystem ps)
    {
        if (ps == null) return;
        float r = GetEmissionRate(ps);
        if (r <= 0.001f && ps.isPlaying) ps.Stop();
    }

    float GetEmissionRate(ParticleSystem ps)
    {
        if (ps == null) return 0f;
        var em = ps.emission;
        // read mode: MinMaxCurve -> evaluate at 0
        return em.rateOverTime.constant;
    }

    void SetEmission(ParticleSystem ps, float rate)
    {
        if (ps == null) return;
        var em = ps.emission;
        em.rateOverTime = new ParticleSystem.MinMaxCurve(Mathf.Max(0f, rate));
    }

    void FadeAudio(AudioSource src, float targetVol)
    {
        if (src == null) return;
        src.volume = Mathf.Clamp01(targetVol);
        if (!src.isPlaying && src.volume > 0.001f) src.Play();
        if (src.volume <= 0.001f && src.isPlaying) src.Stop();
    }

    float GetMaterialAlpha(Material mat, float fallback)
    {
        if (mat == null) return fallback;
        if (mat.HasProperty(fakeFogColorProperty))
        {
            Color c = mat.GetColor(fakeFogColorProperty);
            return c.a;
        }
        return fallback;
    }

    void SetMaterialAlpha(Material mat, float alpha)
    {
        if (mat == null) return;
        if (mat.HasProperty(fakeFogColorProperty))
        {
            Color c = mat.GetColor(fakeFogColorProperty);
            c.a = Mathf.Clamp01(alpha);
            mat.SetColor(fakeFogColorProperty, c);
        }
    }
}