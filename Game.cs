// 劉与ビ
// 72449223
// 2025/07/02

#nullable enable
using GameCanvas;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 弾幕シューティングゲーム
/// プレイヤーは上下左右に移動でき、ボタンを押すことで弾を発射して攻撃します。
/// 敵は画面の上から接近して攻撃してくるため、プレイヤーは敵の弾を避けながら、敵を倒すことを目指します。
/// 現在の進捗：プレイヤーの移動操作の実装が完了しています。
/// </summary>
public sealed class Game : GameBase
{
    // 変数の宣言
    int player_x;
    int player_y;
    int player_speed;
    int player_stage;

    int time;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void InitGame()
    {
        gc.SetResolution(720, 1280);
        player_x = 350;
        player_y = 800;
        player_speed = 2;
        player_stage = 1; //stage1,2,3,4
    }

    /// <summary>
    /// 動きなどの更新処理
    /// </summary>
    public override void UpdateGame()
    {
        PlayerMove();
    }

    /// <summary>
    /// 描画の処理
    /// </summary>
    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.FillRect(player_x, player_y, 20,30);
        gc.DrawImage(GcImage.Panel, 0, 880);

        gc.SetColor(0, 0, 0);
        gc.SetFontSize(48);
        gc.DrawString($"Player Stage: {player_stage}", 20, 10);
    }


    void PlayerMove (){
        if (150 < gc.GetPointerX(0) && gc.GetPointerX(0) < 280) {
            if (1110 < gc.GetPointerY(0) && gc.GetPointerY(0) < 1280){
                player_y += player_speed;
            }
            if (880 < gc.GetPointerY(0) && gc.GetPointerY(0) < 1020) {
                player_y -= player_speed;
            }
        }
        if (1020 < gc.GetPointerY(0) && gc.GetPointerY(0) < 1110) {
            if (0 < gc.GetPointerX(0) && gc.GetPointerX(0) < 190){
                player_x -= player_speed;
            }
            if (190 < gc.GetPointerX(0) && gc.GetPointerX(0) < 360) {
                player_x += player_speed;
            }
        }
    }
}
