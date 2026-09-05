/// <summary>
/// SoundDatabaseのキー定数。
/// 音を鳴らす側はこのクラスの定数を使う。
/// Inspectorのkeyフィールドもこの文字列と一致させる。
/// </summary>
public static class SoundKeys
{
    // --- UI ---
    public const string ButtonPress = "ui_button_press";    // ボタンを押したときのポンッ
    public const string CursorMove = "ui_cursor_move";     // カーソル移動音

    // --- レーン選択(選択方式) ---
    public const string LaneCursorMove = "lane_cursor_move"; // スティックでレーン色を切り替えたときの音
    public const string LaneDecide = "lane_decide";           // 決定ボタンでレーンを反転させたときの音

    // --- ゲーム開始演出 ---
    public const string OpenStore = "open_store";         // 開店音
    public const string CloseStore = "close_store";      //閉店音
    public const string DoorOpen = "door_open";          // ドアがガラガラ開く音

    // --- 寿司Spawner ---
    public const string SelectSushi = "select_sushi";       // スティックで寿司を選択したとき
    // 寿司ごとのボイスは "voice_sushi_{SushiData.sushiName}" で動的生成
    // 例: voice_sushi_まぐろ / voice_sushi_たまご

    // --- 寿司・レーン ---
    public const string PlateBreak = "plate_break";        // 皿同士がぶつかって割れる音
    public const string SushiSpawn = "sushi_spawn";

    // --- スコア ---
    public const string ScoreUp = "score_up";           // チャリンチャリン（加算）
    public const string ScoreDown = "score_down";         // 減算音

    // --- ボイス系 ---
    public const string CustomerSpawn = "customer_spawn";     // テレレンテレレン！
    public const string CustomerOrder = "customer_order";     // 注文だよー・すみませーん・これお願いしまーす
    public const string CustomerEat = "customer_eat";       // むしゃむしゃ
    public const string CustomerServed = "customer_served";    // キラン（寿司が届いた）
    public const string CustomerAngry = "customer_angry";     //ふざけんなよ
    public const string CustomerAngry2 = "customer_angry_2";  //むかつくわ
    public const string CustomerAngryOrder = "customer_angry_order";  //はよもってこい
    public const string EntryVoice = "entry_voice";  //へいらっしゃい


    // --- BGM ---
    public const string BgmTitle = "bgm_title";          // タイトル画面BGM
    public const string BgmGame = "bgm_game";           // ゲーム中BGM
    public const string BgmResult = "bgm_result";       //リザルト画面BGM
}