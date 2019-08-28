using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AudienceNetwork.Utility;
using AudienceNetwork;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{ 
    public Transform coffee_table;
    public GameObject prefab_coffee;
    public GameObject window_coffee;
    public GameObject window_ordering;

    private RewardedVideoAd rewardedVideoAd;
    private bool isLoaded;
    private bool didClose;

    // UI elements in scene
    public Text statusLabel;

    private void Start()
    {
        if (!AdUtility.IsInitialized())
        {
            AdUtility.Initialize();
        }
    }
    public void ShowAd()
    {
        LoadRewardedVideo();
    }

    private void Update()
    {
        if (window_coffee.activeSelf && Input.GetKeyDown(KeyCode.Escape)) BTN_Leave();
    }

    public void LoadRewardedVideo()
    {
        statusLabel.text = "Loading rewardedVideo ad...";
        rewardedVideoAd = new RewardedVideoAd("378297562859739_378307472858748");
        rewardedVideoAd.Register(gameObject);
        rewardedVideoAd.RewardedVideoAdDidLoad = delegate ()
        {
            Debug.Log("RewardedVideo ad loaded.");
            isLoaded = true;
            didClose = false;
            string isAdValid = rewardedVideoAd.IsValid() ? "valid" : "invalid";
            statusLabel.text = "Ad loaded and is " + isAdValid + ". Click show to present!";
            rewardedVideoAd.Show();
        };

        rewardedVideoAd.RewardedVideoAdDidFailWithError = delegate (string error)
        {
            Debug.Log("RewardedVideo ad failed to load with error: " + error);
            SetStatusOrderingWindow(false);
        };

        rewardedVideoAd.RewardedVideoAdWillLogImpression = delegate ()
        {
            AdGetReward();
        };

        rewardedVideoAd.RewardedVideoAdDidClick = delegate ()
        {
            AdGetReward();
        };

        rewardedVideoAd.RewardedVideoAdDidFail = delegate ()
        {
            Debug.Log("Rewarded video ad not validated, or no response from server");
            SetStatusOrderingWindow(false);
        };

        rewardedVideoAd.RewardedVideoAdDidClose = delegate ()
        {
            Debug.Log("Rewarded video ad did close.");
            didClose = true;
            SetStatusOrderingWindow(false);
        };

        if (Application.platform == RuntimePlatform.Android)
        {
            rewardedVideoAd.RewardedVideoAdActivityDestroyed = delegate ()
            {
                if (!didClose)
                {
                    Debug.Log("Rewarded video activity destroyed without being closed first.");
                    Debug.Log("Game should resume. User should not get a reward.");
                }
            };
        }
        rewardedVideoAd.LoadAd();
    }

    private void SetStatusOrderingWindow(bool status)
    {
        if (!status && rewardedVideoAd != null) rewardedVideoAd.Dispose(); 
        window_ordering.SetActive(status);
    }

    public void AdGetReward()
    {
        PlayerPrefs.SetInt("Coffee", PlayerPrefs.GetInt("Coffee") + 1);
        SetStatusOrderingWindow(false);
        RefreshCoffees();
    }

    public void RefreshCoffees()
    {
        if (coffee_table.childCount > 0) foreach (Transform t in coffee_table) Destroy(t.gameObject);
        StartCoroutine(PutCoffee());
    }

    private IEnumerator PutCoffee()
    {
        var coffeeN = PlayerPrefs.GetInt("Coffee");
        for (var x = 0; x < coffeeN; x++)
        {
            if (!window_coffee.activeSelf) break; //Parar de colocar tazas si cierras la ventana.
            var go_t = Instantiate(prefab_coffee).transform;
            go_t.SetParent(coffee_table);
            int fila = x / 6;
            go_t.localPosition = new Vector3(-180 + 70 * (x - fila * 6), 100 - 60 * fila, 0);
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    public void BTN_OpenCoffeeWindow()
    {
        window_coffee.SetActive(true);
        RefreshCoffees();
    }

    public void CancelMyCoffee()
    {
        SetStatusOrderingWindow(false);
    }

    public void BTN_TakeOne()
    {
        SetStatusOrderingWindow(true);
        ShowAd();
    }

    public void BTN_Leave()
    {
        if (window_ordering.activeSelf) SetStatusOrderingWindow(false);
        window_coffee.SetActive(false);
    }
}
