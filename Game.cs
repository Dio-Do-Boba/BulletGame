// 2025/07/06
//　最新追加：敵の移動とダメージ計算の実装。

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
/// 実装した功能：
/// プレイヤーの移動操作の実装が完了しています。
/// プレイヤーの弾丸の発射機能の実装が完了しました
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

    struct Enemy{
        public float2 pos; // 敵の位置
        public int hp; // 敵の体力
        public Enemy(float2 pos, int h)
        {
            this.pos = pos;
            this.hp = h;
        }
    }
    List<Enemy> enemys = new();
    int enemy_spawn_Y; // 敵のスポーン位置
    int enemy_health; // 敵の体力
    int enemy_speed; // 敵の移動速度
    int enemy_number; // 敵の数
    int enemy_stage;
    int enemy_kill_count; // 敵を倒した数

    int time;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        player_x = 350;
        player_y = 800;
        player_speed = 3;
        player_stage = 1; //stage1,2,3,4
        player_weapon =1; //
        player_bullet_cooldown_timer = 0; // クールダウンタイマーの初期化

        enemy_stage = 1; // 敵のステージ
        enemy_speed = 1; // 敵の移動速度
        enemy_spawn_Y = -30;
        
        EnemyStageUPdate();
        for (int i = 0; i < enemy_number; i++)
        {
            enemys.Add(RandomEnemySpawn());
        }

    }
    Enemy RandomEnemySpawn()
    {
        float x = UnityEngine.Random.Range(40f, 680f); // 屏幕宽度之内
        float y = enemy_spawn_Y; // 敵のスポーン位置
        int hp = enemy_health; // 敵の初期体力
        return new Enemy(new float2(x, y), hp);
    }
    /// <summary>
    /// 動きなどの更新処理
    /// </summary>
    public override void UpdateGame()
    {
        // player
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

        // enemy
        EnemyStageUPdate();
        EnemyMove();
        EnemyGetHit();
        List<Enemy> newEnemies = new();
        
        for (int i = enemys.Count - 1; i >= 0; i--)// 画面外に出た敵やhpが0の敵を削除
        {
            if (enemys[i].pos.y > 880)
            {
                enemys.RemoveAt(i);
                newEnemies.Add(RandomEnemySpawn());
            }
            if (enemys[i].hp <= 0)
            {
                enemy_kill_count++;
                newEnemies.Add(RandomEnemySpawn());
            }
        }
        enemys.RemoveAll(e => e.hp <= 0); 
        enemys.AddRange(newEnemies);
    }

    /// <summary>
    /// 描画の処理
    /// </summary>
    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.DrawImage(GcImage.Player1, player_x, player_y);
        // bulletの位置を更新
        gc.SetColor(0, 0, 255);
        for (int i = 0; i < player_bullets.Count; i++)
        {
            float2 b = player_bullets[i];
            gc.FillRect(b.x, b.y, 5, 10);
        }

        // 敵の描画
        gc.SetColor(255, 0, 0);
        foreach (var enemy in enemys)
        {
            gc.FillRect(enemy.pos.x, enemy.pos.y, 20, 30);
            gc.DrawString($"HP: {enemy.hp}", enemy.pos.x, enemy.pos.y - 20);
        }

        gc.SetColor(0, 0, 0);
        gc.SetFontSize(48);
        gc.DrawString($"Player Stage: {player_stage}", 20, 10);

        gc.DrawImage(GcImage.Panel, 0, 880);
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

    void EnemyStageUPdate() {
        if (enemy_stage == 1) {
            enemy_health = 3;
            enemy_number = 3;
        }
        if (enemy_stage == 2) {
            enemy_health = 15;
            enemy_number = 2;
        }
        if (enemy_stage == 3) {
            enemy_health = 35;
            enemy_number = 1;
        }
    }

    void EnemyMove() {
        for (int i = 0; i < enemys.Count; i++)
        {
            Enemy e = enemys[i];
            e.pos.y += enemy_speed;
            enemys[i] = e;
        }
    }

    void EnemyGetHit(){
        for (int i = 0; i < enemys.Count; i++)
        {
            Enemy e = enemys[i];
            for (int j = 0; j < player_bullets.Count; j++)
            {
                float2 b = player_bullets[j];
                if (gc.CheckHitRect(b.x, b.y, 20, 30, e.pos.x, e.pos.y, 50, 60))
                {
                    e.hp -= player_bullet_damage;
                    player_bullets.RemoveAt(j);
                    break;
                }
            }
            enemys[i] = e;
        }
    }
}
