// This script handles the sound effects and music for the whole game.
// Made by Vonce Chew

using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// One audio manager for the whole game
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer routing")]
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup musicGroup;

    [Header("Combat SFX")]
    public AudioClip cardPlay;
    public AudioClip attackHit;
    public AudioClip swordHit;
    public AudioClip swordHard;
    public AudioClip blockImpact;
    public AudioClip blockGain;
    public AudioClip energized;
    public AudioClip parrySuccess;
    public AudioClip dodgeSuccess;
    public AudioClip enemyHit;
    public AudioClip enemyDeath;

    [Header("Card & Dice Feedback SFX")]
    public AudioClip cardShuffle;
    public AudioClip diceRoll;


    [Header("UI / Feedback SFX")]
    public AudioClip buttonClick;
    public AudioClip purchase;
    public AudioClip chestOpen;
    public AudioClip potionDrink;
    public AudioClip levelUp;
    public AudioClip cardAcquired;
    public AudioClip doorOpening;

    [Header("Stingers")]
    public AudioClip victory;
    public AudioClip defeat;

    private AudioSource _sfxSource; // for one-shot effects
    private AudioSource _musicSource; // for looping music

    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip dungeonMusic;
    public AudioClip battleMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.outputAudioMixerGroup = sfxGroup;
        _sfxSource.playOnAwake = false;

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.outputAudioMixerGroup = musicGroup;
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
    }

    /// <summary>
    /// Play any one-shot sound effect
    /// </summary>
    /// <param name="clip">The sound to play.</param>
    public void PlaySfx(AudioClip clip)
    {
        if (clip != null && _sfxSource != null)
            _sfxSource.PlayOneShot(clip);
    }

    /// <summary>
    /// Start, or switch to a looping music track
    /// </summary>
    /// <param name="track">The music clip to loop.</param>
    public void PlayMusic(AudioClip track)
    {
        if (track == null || _musicSource == null) return;
        if (_musicSource.clip == track && _musicSource.isPlaying) return;  // already playing it
        _musicSource.clip = track;
        _musicSource.Play();
    }

    /// <summary>
    /// Stop the music
    /// </summary>
    public void StopMusic()
    {
        if (_musicSource != null) _musicSource.Stop();
    }

    // Combat
    public void CardPlay() => PlaySfx(cardPlay);
    public void AttackHit() => PlaySfx(attackHit);
    public void SwordHit() => PlaySfx(swordHit);
    public void SwordHardHit() => PlaySfx(swordHard);
    public void BlockImpact() => PlaySfx(blockImpact);
    public void BlockGain() => PlaySfx(blockGain);
    public void Energized() => PlaySfx(energized);
    public void ParrySuccess() => PlaySfx(parrySuccess);
    public void DodgeSuccess() => PlaySfx(dodgeSuccess);
    public void EnemyHit() => PlaySfx(enemyHit);
    public void EnemyDeath() => PlaySfx(enemyDeath);
    public void Victory() => PlaySfx(victory);
    public void Defeat() => PlaySfx(defeat);

    // Card & Dice 
    public void CardShuffle() => PlaySfx(cardShuffle);
    public void DiceRoll() => PlaySfx(diceRoll);

    // Feedback
    public void ButtonClick() => PlaySfx(buttonClick);
    public void Purchase() => PlaySfx(purchase);
    public void ChestOpen() => PlaySfx(chestOpen);
    public void PotionDrink() => PlaySfx(potionDrink);
    public void LevelUp() => PlaySfx(levelUp);
    public void CardAcquired() => PlaySfx(cardAcquired);
    public void DoorOpen() => PlaySfx(doorOpening);

    // For Main menu, game and battle music

    public void PlayMenuMusic() => PlayMusic(menuMusic);
    public void PlayDungeonMusic() => PlayMusic(dungeonMusic);
    public void PlayBattleMusic() => PlayMusic(battleMusic);
}