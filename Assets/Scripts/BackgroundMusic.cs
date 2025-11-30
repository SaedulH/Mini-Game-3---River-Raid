using AudioSystem;
using Utilities;

public class BackgroundMusic : PersistentSingleton<BackgroundMusic>
{
    public AudioData BGM;
    private AudioEmitter _emitter;

    private void Start()
    {
        _emitter = AudioManager.Instance.CreateAudioBuilder()
            .WithLoop()
            .WithVolume(0.1f)
            .WithParent(transform)
            .Play(BGM);
    }
}
