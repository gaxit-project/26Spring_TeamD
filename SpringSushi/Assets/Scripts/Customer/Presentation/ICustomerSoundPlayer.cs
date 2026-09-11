/// <summary>
/// サウンド再生の抽象。CustomerPresentationがSoundPlayerの実装に直接依存しないようにする。
/// </summary>
public interface ICustomerSoundPlayer
{
    void PlaySpawnVoice();
    void PlayAngryVoice();
    void PlaySatisfiedVoice(); // ★ 追加
}