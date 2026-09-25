using UnityEngine;
using MiniMarketTycoon.Save;
using MiniMarketTycoon.Utilities;

namespace MiniMarketTycoon.Audio
{
    /// <summary>
    /// Audio management system providing separate channels for Music and Sound Effects.
    /// Supports volume persistence and settings synchronization.
    /// </summary>
    public class AudioManager : MonoBehaviourSingleton<AudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        [Header("Volume Controls")]
        [Range(0f, 1f)] [SerializeField] private float _musicVolume = 1f;
        [Range(0f, 1f)] [SerializeField] private float _sfxVolume = 1f;

        public float MusicVolume => _musicVolume;
        public float SfxVolume => _sfxVolume;

        protected override void OnInitialized()
        {
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                _musicSource.loop = true;
                _musicSource.playOnAwake = false;
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
                _sfxSource.loop = false;
                _sfxSource.playOnAwake = false;
            }

            if (SaveManager.HasInstance && SaveManager.Instance.CurrentSave != null)
            {
                _musicVolume = SaveManager.Instance.CurrentSave.Settings.MusicVolume;
                _sfxVolume = SaveManager.Instance.CurrentSave.Settings.SfxVolume;
            }

            ApplyVolumes();
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || _musicSource == null) return;
            if (_musicSource.clip == clip && _musicSource.isPlaying) return;

            _musicSource.clip = clip;
            _musicSource.volume = _musicVolume;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
        }

        public void PlaySfx(AudioClip clip, float pitch = 1f)
        {
            if (clip == null || _sfxSource == null) return;

            _sfxSource.pitch = pitch;
            _sfxSource.PlayOneShot(clip, _sfxVolume);
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
            {
                _musicSource.volume = _musicVolume;
            }

            SyncSettings();
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            SyncSettings();
        }

        private void ApplyVolumes()
        {
            if (_musicSource != null) _musicSource.volume = _musicVolume;
        }

        private void SyncSettings()
        {
            if (SaveManager.HasInstance && SaveManager.Instance.CurrentSave != null)
            {
                SaveManager.Instance.CurrentSave.Settings.MusicVolume = _musicVolume;
                SaveManager.Instance.CurrentSave.Settings.SfxVolume = _sfxVolume;
            }
        }
    }
}
