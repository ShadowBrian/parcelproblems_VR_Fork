using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager INSTANCE;

    public Text timerText;
    private float packageTimer;
    private float lastPackageTime;
    public Text totalTimeText;
    public float totalTimeTimer;

    public float conveyorForce;
    public float maxConveyorSpeed;

    public GameObject[] globalItemArray;
    public int currentDifficulty = 0;
    public float timescaleDifficultyCoeff = 0.06f;
    public int _totalShipped = 0;
    public int _totalPoints = 0;
    public float _avgPackageTime = 0;
    public List<float> _packageTimes = new List<float>();

    //STATS
    private int _perfectPackages = 0;
    private int _garbagePackages = 0;
    private float _fastestPackage = 100;

    public int allowedErrors = 5;
    private int _errorCounter = 0;
    public bool GAMEOVER = false;
    private int diffCoeff = 0;
    public int packageAVGNum = 5;


    public float spawnVel = 3;
    public GameObject tapeSpawnPoint;
    public GameObject tapePrefab;
    public GameObject labelSpawnPoint;
    public GameObject labelPrefab;

    public BoxEffects boxEffects;
    public BoxManager boxMan;
    public PackageRequester packageReq;
    public AudioManager audio;

    public AudioSource heartbeat;
    public AudioSource phoneCall;

    public float effectInterpTime;
    public float effectIntensity;
    public PostProcessProfile cameraProfile;
    private Vignette _vin;
    private ChromaticAberration _chm;
    private DepthOfField _dof;

    public Text endPoints;
    public Text endShipped;
    public Text endPerfect;
    public Text endGarbage;
    public Text endFastest;
    public Text endOkay;
    public Canvas endCanvas;

    public Canvas startCanvas;

    public bool shiftStarted = false;

    public delegate void PackageSent();
    public event PackageSent OnPackageSend;

    public float GetDifficulty()
    {
        return currentDifficulty + timescaleDifficultyCoeff * (Mathf.Clamp(totalTimeTimer, 0, 400) + Mathf.Clamp(_totalShipped, 0, 30));
    }

    void Awake()
    {
        INSTANCE = this;
    }

    private void Start()
    {
        for (int i = 0; i < globalItemArray.Length; i++)
        {
            var script = globalItemArray[i].GetComponent<Item>();

            if (script != null)
            {
                script.itemID = i;
            }
        }

        totalTimeTimer = 0;

        _vin = cameraProfile.GetSetting<Vignette>();
        _chm = cameraProfile.GetSetting<ChromaticAberration>();
        _dof = cameraProfile.GetSetting<DepthOfField>();

        diffCoeff = Random.Range(-2, 2);

        endCanvas.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (shiftStarted)
        {
            packageTimer += Time.deltaTime;
            totalTimeTimer += Time.deltaTime;
            timerText.text = ((int)(packageTimer * 10) / 10f).ToString();
            totalTimeText.text = ((int)(totalTimeTimer * 10) / 10f).ToString();
        }


        if (packageTimer > 25 || _errorCounter > allowedErrors) GameOver();


        UpdateVisuals();

        if (_totalShipped < 10) currentDifficulty = 0;
        else if (_totalShipped < 20 + diffCoeff) currentDifficulty = 1;
        else if (_totalShipped < 30 + diffCoeff) currentDifficulty = 2;
        else if (_totalShipped < 41 + diffCoeff) currentDifficulty = 3;

        maxConveyorSpeed = currentDifficulty + 1;

        if (!heartbeat.isPlaying && packageTimer > 15) heartbeat.Play();
        if (GAMEOVER) heartbeat.Stop();

        _errorCounter = Mathf.Clamp(_errorCounter, 0, 10);
    }

    private void GameOver()
    {
        if (!GAMEOVER)
        {
            GAMEOVER = true;

            endPoints.text = "You Earned " + _totalPoints + " Points.";
            endShipped.text = _totalShipped + " Total Packages Shipped";
            endPerfect.text = _perfectPackages + " Perfect Packages";
            endGarbage.text = _garbagePackages + " Terrible Packages";
            endOkay.text = Mathf.Abs(_totalShipped - _garbagePackages - _perfectPackages) + " Okay Packages";
            if (_fastestPackage < 30) endFastest.text = ((int)(_fastestPackage * 100) / 100f) + "s Fastest Package";
            else endFastest.text = "";


            endCanvas.gameObject.SetActive(true);

            phoneCall.Play();
        }

    }

    public void StartShift()
    {
        startCanvas.gameObject.SetActive(false);
        shiftStarted = true;
    }

    public void Restart()
    {
        Scene thisScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(thisScene.name);
    }

    public void SendPackage(bool unfolded, bool taped, bool labeled, int[] contents)
    {


        //CALCULATING TOTAL POINTS
        int totalPoints = 0;
        PackageRequest req = packageReq.currentReq;

        if (currentDifficulty == 0) packageTimer--;
        if (currentDifficulty > 1) packageTimer += 1f;
        if (currentDifficulty > 2) packageTimer += 1.9f;

        if (unfolded & taped)
        {
            audio.PlayClip(4);

            for (int i = 0; i < contents.Length; i++)
            {
                for (int j = 0; j < req.requestItems.Length; j++)
                {
                    if (contents[i] == req.requestItems[j])
                    {
                        req.requestItems[j] = -1;
                        contents[i] = -1;
                    }
                }
            }

            int totalCorrect = 0;
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i] == -1) totalCorrect += 1;
            }

            if (labeled && totalCorrect == contents.Length) _perfectPackages++;
            if (totalCorrect == 0)
            {
                _garbagePackages++;
                audio.PlayClip(5);
                totalPoints = 0;
            }
            else
            {
                audio.PlayClip(4);
                totalPoints = (int)(totalCorrect / ((float)contents.Length) * 100) + 50;
            }



            if (!labeled)
            {
                totalPoints = (int)(totalPoints * .4f);
            }

            if (packageTimer < 10)
            {
                totalPoints += (int)(Mathf.Clamp(Mathf.Abs(10 - packageTimer), 0, 5)) * 10;
            }

            totalPoints += (int)(totalPoints * 0.1f * Random.value);

            _packageTimes.Add(packageTimer);

            if (_fastestPackage > packageTimer) _fastestPackage = packageTimer;

            _totalShipped++;

            float average = 0;
            for (int i = 0; i < _packageTimes.Count && i < packageAVGNum; i++)
            {
                average += _packageTimes[_packageTimes.Count - 1 - i];
            }
            average /= packageAVGNum;

            //_avgPackageTime = ( ( totalTimeTimer - _totalShipped ) / _totalShipped );
            _avgPackageTime = average;




            if (_avgPackageTime > 10) _errorCounter++;
            else if (_errorCounter > 0) _errorCounter--;

            lastPackageTime = packageTimer;
        }
        else
        {
            totalPoints = -50;
            _garbagePackages += 1;
            _errorCounter += 2;
            _totalShipped++;

            _packageTimes.Add(10);

            audio.PlayClip(5);
        }

        _totalPoints += totalPoints;

        print("Awarded " + totalPoints + " Points || TOTAL POINTS: " + _totalPoints);

        packageTimer = 0;

        OnPackageSend();

        print("Total Time Per Ship Average: " + (_avgPackageTime));

        if (_errorCounter > 2)
        {
            if (!heartbeat.isPlaying) heartbeat.Play();
        }
        else heartbeat.Pause();

    }

    private void UpdateVisuals()
    {

        if (!GAMEOVER && shiftStarted)
        {
            if (packageTimer > 14.9 || _avgPackageTime > 10)
            {

                _vin.intensity.Interp(_vin.intensity.value, .3f, Time.deltaTime * effectInterpTime);
                _chm.intensity.Interp(_chm.intensity.value, .3f, Time.deltaTime * effectInterpTime);
                _dof.focusDistance.Interp(_dof.focusDistance.value, .6f, Time.deltaTime * effectInterpTime);
            }
            //else if( _avgPackageTime > 0 && _avgPackageTime < 10)
            //{
            //    _vin.intensity.Interp( _vin.intensity.value, .2f, Time.deltaTime * effectInterpTime );
            //    _chm.intensity.Interp( _chm.intensity.value, .11f, Time.deltaTime * effectInterpTime );
            //    _dof.focusDistance.Interp( _dof.focusDistance.value, 1, Time.deltaTime * effectInterpTime );
            //}
            else
            {

                float coeff = Mathf.Clamp((effectIntensity * Mathf.Clamp(Time.time / 190, 0, 1) + _errorCounter / (float)allowedErrors), 0, 1);

                _vin.intensity.Interp(_vin.intensity.value, .2f + 0.2f * coeff, Time.deltaTime * effectInterpTime);
                _chm.intensity.Interp(_chm.intensity.value, .1f + 0.4f * coeff, Time.deltaTime * effectInterpTime);
                _dof.focusDistance.Interp(_dof.focusDistance.value, 1 - 0.6f * coeff, Time.deltaTime * effectInterpTime);


            }
        }
        else
        {
            _vin.intensity.Interp(_vin.intensity.value, .4f, Time.deltaTime * effectInterpTime);
            _chm.intensity.Interp(_chm.intensity.value, .6f, Time.deltaTime * effectInterpTime);
            _dof.focusDistance.Interp(_dof.focusDistance.value, .169f, Time.deltaTime * effectInterpTime);
        }

    }

    public void NewBox() { boxMan.NewBox(); }

    public void SpawnLabel()
    {
        var obj = Instantiate(labelPrefab, labelSpawnPoint.transform.position, Quaternion.identity);
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(labelSpawnPoint.transform.right * spawnVel * GetDifficulty());
        }
    }

    public void SpawnTape()
    {
        var obj = Instantiate(tapePrefab, tapeSpawnPoint.transform.position, Quaternion.identity);

        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(tapeSpawnPoint.transform.right * spawnVel * GetDifficulty());
        }
    }
}
