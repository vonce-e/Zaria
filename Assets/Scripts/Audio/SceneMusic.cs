// This script plays the correct background music for each scene.
// Made by Vonce Chew

using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    public enum Track { Menu, Dungeon, Battle }
    public Track track;

    void Start()
    {
        if (AudioManager.Instance == null) return;

        switch (track)
        {
            case Track.Menu: AudioManager.Instance.PlayMenuMusic(); break;
            case Track.Dungeon: AudioManager.Instance.PlayDungeonMusic(); break;
            case Track.Battle: AudioManager.Instance.PlayBattleMusic(); break;
        }
    }
}