using UnityEngine;

public class BossIntroManager : MonoBehaviour, IReset
{
    public Boss boss;

    public BossWall entranceWall;

    public SoundPlayer TextWarning;

    public GameObject FocusCamera1;

    public GameObject FocusCamera2;

    bool inIntro = false;

    float introTime = 0f;

    public float OuterZoomTime = 2f;

    public float ZoomInHold = 4f;

    public float DescentHold = 7f;

    public float ZoomOutTime = 8f;

    public float CameraMoveSpeed = 1f;

    public float CameraStickSpeed = 9f;

    public float CameraFinishSpeed = 3f;

    bool warningShown = false;

    private Vector3 CamStartPos;

    private Quaternion CamStartRot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(boss == null)
        {
            boss = GetComponent<Boss>();
        }
    }

    public void DoReset()
    {
        entranceWall.gameObject.SetActive(false);
        introTime = 0f;
        inIntro = false;
        warningShown = false;
    }

    // Update is called once per frame
    void Update()
    {
        ContinueIntro();
    }

    public void StartIntro()
    {
        entranceWall.gameObject.SetActive(true);
        PlayerManager.Instance.TriggerBossCutscene();

        CamStartPos = Camera.main.gameObject.transform.position;
        CamStartRot = Camera.main.gameObject.transform.rotation;

        inIntro = true;
    }

    public void ContinueIntro()
    {
        if(inIntro)
        {
            introTime += Time.deltaTime;

            if(introTime > 0 && introTime < OuterZoomTime)
            {
                Camera.main.gameObject.transform.position = Vector3.Lerp(Camera.main.gameObject.transform.position, FocusCamera1.transform.position, CameraMoveSpeed * Time.deltaTime);
                Camera.main.gameObject.transform.rotation = Quaternion.Lerp(Camera.main.gameObject.transform.rotation, FocusCamera1.transform.rotation, CameraMoveSpeed * Time.deltaTime);
            }
            else if (introTime > OuterZoomTime && introTime < ZoomInHold)
            {
                Camera.main.gameObject.transform.position = Vector3.Lerp(Camera.main.gameObject.transform.position, FocusCamera2.transform.position, CameraMoveSpeed * Time.deltaTime);
                Camera.main.gameObject.transform.rotation = Quaternion.Lerp(Camera.main.gameObject.transform.rotation, FocusCamera2.transform.rotation, CameraMoveSpeed * Time.deltaTime);
            }
            else if (introTime > ZoomInHold && introTime < DescentHold)
            {
                ShowWarning();
                Camera.main.gameObject.transform.position = Vector3.Lerp(Camera.main.gameObject.transform.position, FocusCamera2.transform.position, CameraStickSpeed * Time.deltaTime);
                Camera.main.gameObject.transform.rotation = Quaternion.Lerp(Camera.main.gameObject.transform.rotation, FocusCamera2.transform.rotation, CameraStickSpeed * Time.deltaTime);
            }
            else if (introTime > DescentHold && introTime < ZoomOutTime)
            {
                Camera.main.gameObject.transform.position = Vector3.Lerp(Camera.main.gameObject.transform.position, CamStartPos, CameraFinishSpeed * Time.deltaTime);
                Camera.main.gameObject.transform.rotation = Quaternion.Lerp(Camera.main.gameObject.transform.rotation, CamStartRot, CameraFinishSpeed * Time.deltaTime);
            }
            else if(introTime > ZoomOutTime)
            {
                EndIntro();
            }
        }
    }

    public void EndIntro()
    {
        PlayerManager.Instance.EndBossCutscene();

        inIntro = false;
    }

    void ShowWarning()
    {
        if(!warningShown)
        {
            PlayerInterface.Instance.ShowWarningMessage();
            PlaySound(TextWarning);
            boss.PlayIntro();
            warningShown = true;
        }
    }

    void PlaySound(SoundPlayer player)
    {
        if (player != null) Instantiate(player.gameObject);
    }
}
