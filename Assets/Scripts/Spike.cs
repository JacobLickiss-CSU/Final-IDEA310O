using UnityEngine;

public class Spike : MonoBehaviour
{
    public float GrowTime = .2f;

    public float HoldTime = 2f;

    public float ShrinkTime = .2f;

    public float SizeMultiplier = 100f;

    private float timer = 0f;

    public SoundPlayer SpawnSound = null;

    public SoundPlayer VanishSound = null;

    public float SoundChance = .5f;

    private bool didPlayVanish = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.localScale = new Vector3(.01f, .01f, .01f) * SizeMultiplier;

        PlaySound(SpawnSound);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if(timer < GrowTime)
        {
            float progress = timer / GrowTime;

            transform.localScale = new Vector3(progress, progress, progress) * SizeMultiplier;
        }
        else if(timer < GrowTime + HoldTime)
        {
            float progress = (timer - GrowTime) / HoldTime;

            transform.localScale = new Vector3(1f, 1f, 1f) * SizeMultiplier;
        }
        else if(timer < GrowTime + HoldTime + ShrinkTime)
        {
            if(!didPlayVanish)
            {
                PlaySound(VanishSound);
                didPlayVanish = true;
            }

            float progress = (timer - GrowTime - HoldTime) / ShrinkTime;

            transform.localScale = new Vector3(1f - progress, 1f - progress, 1f - progress) * SizeMultiplier;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ForceShrink()
    {
        if(timer < GrowTime + HoldTime)
        {
            timer = GrowTime + HoldTime;
        }
    }

    void PlaySound(SoundPlayer player)
    {
        if (Random.Range(0f, 1f) > SoundChance) return;

        if (player != null) Instantiate(player.gameObject, transform);
    }
}
