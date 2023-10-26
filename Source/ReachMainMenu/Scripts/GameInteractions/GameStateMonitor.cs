using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ReachMainMenu.Scripts.GameInteractions
{
    public class GameStateMonitor : MonoBehaviour
    {
        private       bool   _readyToSwitch = true;
        private       bool   _wentInGame    = false;
        public static string _assetPath;
        
        public static void Init(Mod modInstance)
        {
            _assetPath = Path.Combine(modInstance.Path, "Assets");
            GameObject gameStateMonitor = new GameObject($"{Assembly.GetExecutingAssembly().FullName} - Game State Monitor");
            DontDestroyOnLoad(gameStateMonitor);
            gameStateMonitor.AddComponent<GameStateMonitor>();
        }

        private void OnEnable()
        {
            StartCoroutine(SwitchBackground());
        }

        private void Update()
        {
            if(!GameManager.Instance.gameStateManager.IsGameStarted() && _readyToSwitch && !_wentInGame)
            {
                _readyToSwitch = false;
                StartCoroutine(SwitchBackground());
            }
            else if(!GameManager.Instance.gameStateManager.IsGameStarted() && !_readyToSwitch && _wentInGame)
            {
                _readyToSwitch = true;
            }

            if(GameManager.Instance.gameStateManager.IsGameStarted() && !_wentInGame)
            {
                _wentInGame = true;
            }

            if(!GameManager.Instance.gameStateManager.IsGameStarted() && _wentInGame)
            {
                _wentInGame = false;
            }
        }

        private IEnumerator SwitchBackground()
        {
            yield return new WaitForSeconds(1f);
            
            string[] assets      = Directory.GetFiles(_assetPath);
            string   randomAsset = assets[UnityEngine.Random.Range(0, assets.Length)];
            bool     switched    = false;

            NGUIWindowManager window = FindObjectOfType<NGUIWindowManager>();

            ForAllTransforms(window.transform, (t) =>
                                               {
                                                   if(switched) return;
                                                   UITexture uiTexture = t.GetComponent<UITexture>();

                                                   if(!(uiTexture is null) && !(uiTexture.mainTexture is null) &&
                                                      uiTexture.name == "bgTexture")
                                                   {
                                                       byte[]    bytes   = File.ReadAllBytes(randomAsset);
                                                       Texture2D texture = new Texture2D(2, 2);
                                                       texture.LoadImage(bytes);
                                                       uiTexture.mainTexture = texture;
                                                       switched              = true;
                                                   }
                                               });

            yield return new WaitForSeconds(1.1f);
            if(!switched) StartCoroutine(SwitchBackground());
        }

        private static void ForAllTransforms(Transform transform, Action<Transform> action)
        {
            action(transform);
            ForAllChildren(transform, action);
        }

        private static void ForAllChildren(Transform transform, Action<Transform> action)
        {
            foreach(Transform child in transform)
            {
                action(child);
                ForAllTransforms(child, action);
            }
        }
    }
}