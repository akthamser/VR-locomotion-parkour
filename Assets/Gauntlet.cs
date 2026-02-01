using UnityEngine;


public class Gauntlet : MonoBehaviour
{
    [Header("Setup")]
    public ParticleSystem thrusterParticles;

    public Transform thrustDirectionPoint;

    [Header("Audio")]
    public AudioSource thrusterSound;
    public float basePitch = 1.0f;
    public float maxVolume = 1.0f;


    void Start()
    {

        if (thrustDirectionPoint == null)
        {
            thrustDirectionPoint = this.transform;
        }

        if (thrusterSound != null)
        {
            thrusterSound.loop = true; 
            thrusterSound.playOnAwake = false;
            thrusterSound.volume = 0; 
        }
    }

    public void Play(float intensity = 1.0f)
    {
 
        if (thrusterParticles != null)
        {
            if (!thrusterParticles.isPlaying)
                thrusterParticles.Play();

  
        }


        if (thrusterSound != null)
        {
            if (!thrusterSound.isPlaying)
                thrusterSound.Play();


            thrusterSound.volume = Mathf.Lerp(thrusterSound.volume, intensity * maxVolume, Time.deltaTime * 10f);
            thrusterSound.pitch = basePitch + (intensity * 0.5f); 
        }
    }

    public void Stop()
    {
   
        if (thrusterParticles != null && thrusterParticles.isPlaying)
        {
            thrusterParticles.Stop();
        }


        if (thrusterSound != null && thrusterSound.isPlaying)
        {

            thrusterSound.volume = Mathf.Lerp(thrusterSound.volume, 0f, Time.deltaTime * 10f);

            if (thrusterSound.volume < 0.05f)
            {
                thrusterSound.Stop();
            }
        }
    }

    public Vector3 GetDirection()
    {
        if (thrustDirectionPoint != null)
            return -thrustDirectionPoint.forward;
        else
            return -transform.forward;
    }
}