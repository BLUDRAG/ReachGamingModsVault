using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ReachKillshotOverlay.Scripts.AssetManagement;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace HighwayRadio.Scripts.Features;

public static class AudioPlayer
{
    private static HighwayRadioPlayer.Scripts.HighwayRadioPlayer _player;
    private static string _modPath;
    private static string _audioPath;
    private static List<string> _audioFiles = new List<string>();
    private static float _volume = 1f;
    private static bool _showing = false;
    private static string _canvasPath => Path.Combine(_modPath, "Assets", "HighwayRadioCanvas.unity3d");
    private const  string _canvasAsset = "HighwayRadioCanvas";

    public static void Init(Mod modInstance)
    {
        _modPath   = modInstance.Path;
        _audioPath = Path.Combine(_modPath, "Music");
        LoadAudioFiles();
        _ = LoadAudioPlayer();
    }
    
    private static void LoadAudioFiles()
    {
        _audioFiles.Clear();
        _audioFiles.AddRange(Directory.GetFiles(_audioPath, "*.mp3", SearchOption.AllDirectories));
        _audioFiles.AddRange(Directory.GetFiles(_audioPath, "*.wav", SearchOption.AllDirectories));

        for(int i = 0; i < _audioFiles.Count; i++)
        {
            _audioFiles[i] = _audioFiles[i].Replace(_audioPath, "");
        }
    }

    private static async Task LoadAudioPlayer()
    {
        GameObject asset = await AssetBundleLoader.LoadContent<GameObject>(_canvasPath, _canvasAsset);
        GameObject canvas = Object.Instantiate(asset);
        _player = canvas.GetComponent<HighwayRadioPlayer.Scripts.HighwayRadioPlayer>();
    }

    public static void Show()
    {
        if(_showing)
        {
            return;
        }

        _player.Show();
        _showing = true;
    }
    
    public static void Hide()
    {
        if(!_showing)
        {
            return;
        }

        _player.Hide();
        _showing = false;
    }

    public static async Task Play()
    {
        Task<AudioClip> audioTask = Task.Run(LoadRandomAudio);
        AudioClip       clip      = await audioTask;
        if(clip is null) return;
        _player.Play(clip);
        GameManager.Instance.StartCoroutine(QueueNextMusic(clip));
    }

    private static IEnumerator QueueNextMusic(AudioClip clip)
    {
        float duration = clip.length;
        yield return new WaitUntil(() => (!_player.IsPlaying() && _player.Playtime() >= duration) || !_showing);

        if(!_showing)
        {
            yield break;
        }
        
        _ = Play();
    }

    public static void TogglePause()
    {
        _player.TogglePauseState();
    }

    private static async Task<AudioClip> LoadRandomAudio()
    {
        string randomAudio = _audioFiles[Random.Range(0, _audioFiles.Count)];
        string extension   = Path.GetExtension(randomAudio);

        AudioType type = extension == ".mp3" ? AudioType.MPEG :
                         extension == ".wav" ? AudioType.WAV : AudioType.OGGVORBIS;
        
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"file://{_audioPath}{randomAudio}", type);

        www.SendWebRequest();
        while(!www.isDone) await Task.Yield();

        if(www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Log.Error(www.error);
        }
        else
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            clip.name = Path.GetFileNameWithoutExtension(www.url);
            
            return clip;
        }

        return null;
    }

    public static void IncreaseVolume()
    {
        _volume = Mathf.Min(_volume + 0.1f, 1f);
        _player.SetVolume(_volume);
    }

    public static void DecreaseVolume()
    {
        _volume = Mathf.Max(_volume - 0.1f, 0f);
        _player.SetVolume(_volume);
    }
}