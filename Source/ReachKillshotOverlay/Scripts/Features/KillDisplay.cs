using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ReachKillshotOverlay.Scripts.AssetManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ReachKillshotOverlay.Scripts.Features
{
    public static class KillDisplay
    {
        private static CoroutineRunner _runner;
        private static Animator        _overlay;
        private static Text            _text;
        private static AudioSource     _audioSource;
        private static AudioClip       _audioClip;
        private static bool            _displaying        = false;
        private static WaitForSeconds  _textAnimationTime = new WaitForSeconds(_textFadeTime);
        private static WaitForSeconds  _fadeDelay         = new WaitForSeconds(0.33f);

        private static Queue<(Text text, string formattedText, string entity)> _killQueue =
            new Queue<(Text, string, string)>();

        private static readonly int    ShowTrigger = Animator.StringToHash("Show");
        private static readonly int    FadeTrigger = Animator.StringToHash("Fade");
        private static readonly int    Shift       = Animator.StringToHash("Shift");
        private static readonly int    Advance     = Animator.StringToHash("Advance");
        private static          int    _accumulativeHeadshotCount;
        private const           string _canvasPath            = "Assets/ReachKillshotOverlayCanvas.unity3d";
        private const           string _canvasName            = "ReachKillshotOverlayCanvas";
        private const           string _formattedText         = "<color=#dd2222{0}>{1}</color> Killed";
        private const           string _formattedTextHeadshot = "<color=#dd2222{0}>{1}</color> <b>Headshot x%m</b> Killed";
        private const           float  _textFadeTime          = 0.333f;

        public static void Init(Mod modInstance)
        {
            _ = LoadCanvas(modInstance);
            InitializeCoroutineRunner();
        }

        private static async Task LoadCanvas(Mod modInstance)
        {
            GameObject asset = await AssetBundleLoader.LoadContent(Path.Combine(modInstance.Path, _canvasPath),
                                                                   _canvasName);
            if(asset is null) return;

            GameObject canvas = Object.Instantiate(asset);
            Object.DontDestroyOnLoad(canvas);
            _overlay     = canvas.transform.Find("Overlay").GetComponent<Animator>();
            _text        = canvas.transform.Find("Text").GetComponent<Text>();
            _audioSource = canvas.GetComponent<AudioSource>();
            _audioClip   = _audioSource.clip;
        }

        private static void InitializeCoroutineRunner()
        {
            _runner = new GameObject("Coroutine Runner").AddComponent<CoroutineRunner>();
            Object.DontDestroyOnLoad(_runner.gameObject);
        }

        public static void QueueKill(string entityName, bool headshot)
        {
            _accumulativeHeadshotCount = headshot ? _accumulativeHeadshotCount + 1 : 0;
            Text   text          = Object.Instantiate(_text, _text.transform.parent);
            string formattedText = headshot ? _formattedTextHeadshot.Replace("%m", _accumulativeHeadshotCount.ToString()) : _formattedText;
            text.text = string.Format(formattedText, PercentToHex(255), entityName);
            _killQueue.Enqueue((text, formattedText, entityName));

            if(headshot)
            {
                _audioSource.PlayOneShot(_audioClip);
            }
            else
            {
            }

            if(!_displaying)
            {
                _runner.StartCoroutine(DisplayKills());
            }
        }

        private static IEnumerator DisplayKills()
        {
            _displaying = true;
            _overlay.SetTrigger(ShowTrigger);

            while(_killQueue.Count > 0)
            {
                (Text text, string formattedText, string entity) = _killQueue.Dequeue();
                GameObject gameObject = text.gameObject;
                gameObject.SetActive(true);
                
                const float time        = _textFadeTime * 1.5f;
                float       elapsedTime = 0f;
                
                while(elapsedTime < time && _killQueue.Count == 0)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                
                bool     hasMoreKills = _killQueue.Count > 0;
                Animator animator     = text.GetComponent<Animator>();
                
                if(hasMoreKills)
                {
                    animator.SetBool(Shift, true);
                    animator.SetTrigger(Advance);
                    
                    _runner.StartCoroutine(FadeOutText(255f, 200f, text, formattedText, entity, _textFadeTime));
                    yield return _textAnimationTime;
                    _runner.StartCoroutine(FadeOutText(200f, 0f, text, formattedText, entity, _textFadeTime * 2f));
                    Object.Destroy(gameObject, _textFadeTime * 2f);
                }
                else
                {
                    animator.SetTrigger(Advance);
                    yield return _textAnimationTime;
                    _runner.StartCoroutine(FadeOutText(255f, 0f, text, formattedText, entity, _textFadeTime * 2));
                    yield return _textAnimationTime;
                    yield return _textAnimationTime;
                    Object.Destroy(gameObject);
                }
            }

            _overlay.SetTrigger(FadeTrigger);
            yield return _fadeDelay;
            _displaying = false;
        }

        private static string PercentToHex(int percent)
        {
            string result = percent.ToString("X");

            return result.Length == 1 ? "00" : result;
        }

        private static IEnumerator FadeOutText(float from, float to, Text text, string formattedText, string entity, float time)
        {
            float elapsedTime = 0f;

            while(elapsedTime < time)
            {
                if(!text)
                {
                    yield break;
                }

                elapsedTime += Time.deltaTime;
                int percent = (int)Mathf.Lerp(from, to, elapsedTime / time);

                text.text = string.Format(formattedText, PercentToHex(percent), entity);

                yield return null;
            }

            text.text = string.Format(formattedText, PercentToHex(0), entity);
        }
    }
}