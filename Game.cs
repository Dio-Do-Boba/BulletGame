// 2025/07/04
//　最新追加：プレイヤーの弾丸の発射機能の実装が完了しました。

#nullable enable
using GameCanvas;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;


/// <summary>
/// 弾幕シューティングゲーム
/// プレイヤーは上下左右に移動でき、ボタンを押すことで弾を発射して攻撃します。
/// 敵は画面の上から接近して攻撃してくるため、プレイヤーは敵の弾を避けながら、敵を倒すことを目指します。
/// 実装した功能：プレイヤーの移動操作の実装が完了しています。
/// </summary>
public sealed class Game : GameBase
{
    // 変数の宣言
    int player_x;
    int player_y;
    int player_stage;
    int player_speed;
    int player_health; // プレイヤーの体力
    int player_shield; // プレイヤーのシールド

    List<float2> player_bullets = new();
    int player_weapon; // 0:
    int player_bullet_speed; // 弾の速度
    int player_bullet_damage; // 弾のダメージ  
    int player_bullet_cooldown; // 弾のクールダウン時間
    int player_bullet_cooldown_timer; // クールダウンタイマー
    int player_bullet_number; // 弾の数

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
        player_weapon =1; //
        player_bullet_cooldown_timer = 0; // クールダウンタイマーの初期化
    }

    /// <summary>
    /// 動きなどの更新処理
    /// </summary>
    public override void UpdateGame()
    {
        PlayerMove();
        PlayerAttack();
        if (player_bullet_cooldown_timer > 0)
        {
            player_bullet_cooldown_timer--;
        }
        PlayerStageUpdate();
        PlayerWeaponUpdate();

        for (int i = 0; i < player_bullets.Count; i++)
        {
            float2 b= player_bullets[i];
            b.y -= player_bullet_speed;
            player_bullets[i] = b;
        }

        player_bullets.RemoveAll(b => b.y < 0); // 画面外に出た弾を削除
    }

    /// <summary>
    /// 描画の処理
    /// </summary>
    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.FillRect(player_x, player_y, 20,30);
        gc.DrawImage(GcImage.Panel, 0, 880);

        gc.SetColor(0, 0, 255);
        for (int i = 0; i < player_bullets.Count; i++)
        {
            float2 b = player_bullets[i];
            gc.FillRect(b.x, b.y, 5, 10);
        }

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

    void PlayerAttack() {
        if (gc.GetPointerFrameCount(0) ==1 && player_bullet_cooldown_timer <= 0) {
            float px = gc.GetPointerX(0);
            float py = gc.GetPointerY(0);

            if (400 < px && px < 700 && 900 < py && py < 1200) {
                // 弾を発射
                for (int i = 0; i < player_bullet_number; i++) {
                    float2 bulletPosition = new float2(player_x + 10, player_y + (i * 30));
                    player_bullets.Add(bulletPosition);
                }
                player_bullet_cooldown_timer = player_bullet_cooldown; // クールダウンタイマーを設定
            }
        }
    }

    void PlayerStageUpdate() {
        if (player_stage ==1) {
            player_health = 1;
            player_shield = 1;
        }
        if (player_stage == 2) {
            player_health = 2;
            player_shield = 1;
        }
        if (player_stage == 3) {
            player_health = 3;
            player_shield = 2;
        }
    }

    void PlayerWeaponUpdate() {
        if (player_weapon == 1) {
            player_bullet_speed = 5;
            player_bullet_damage = 1;
            player_bullet_cooldown = 20;
            player_bullet_number = 1;
        }
        if (player_weapon == 2) {
            player_bullet_speed = 5;
            player_bullet_damage = 1;
            player_bullet_cooldown = 10;
            player_bullet_number = 2;
        }
        if (player_weapon == 3) {
            player_bullet_speed = 6;
            player_bullet_damage = 2;
            player_bullet_cooldown = 10;
            player_bullet_number = 3;
        }
    }
}
