/// 2025/07/16
/// 最新追加：stageup debug

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
/// プレイヤーの移動操作の実装が完了しています
/// プレイヤーの弾丸の発射機能の実装が完了しました
/// 敵の移動とダメージ計算の実装が完了しました
/// 敵の弾丸の発射機能の実装が完了しました
/// game over状態の実装が完了しました
/// stage2の実装が完了しました
/// treasureの実装が完了しました
/// stage3 completed
/// </summary>
public sealed class Game : GameBase
{
    // 変数の宣言
    int game_state; 

    int player_x;
    int player_y;
    int player_speed;
    int player_health; 

    List<float2> player_bullets = new();
    int player_weapon; 
    int player_bullet_speed; // 弾の速度
    int player_bullet_damage; // 弾のダメージ  
    int player_bullet_cooldown; // 弾のクールダウン時間
    int player_bullet_cooldown_timer; // クールダウンタイマー
    int player_bullet_number; // 弾の数

    bool hasTreasure = false;
    float2 treasure_pos;
    bool treasure_collected = false;

    bool hasTreasure2 = false;
    float2 treasure_pos2;
    bool treasure_collected2 = false;

    int game_stage;

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
    int enemy_health; // 敵の体力
    int enemy_speed; // 敵の移動速度
    int enemy_number; // 敵の数
    int enemy_stage;
    int enemy_kill_count; // 敵を倒した数

    List<float2> enemy_bullets = new(); // 敵の弾
    List<float2>  enemy_bullet_dirs = new();
    int enemy_bullet_speed; // 敵の弾の速度
    int enemy_bullet_cooldown; // 敵の弾のクールダウン時間
    int enemy_bullet_cooldown_timer; // 敵の弾のクールダウンタイマー

    bool stageTransition = false; // 全局变量添加

    int time;

    /// <summary>
    /// 初期化処理
    /// </summary>
    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);

        player_bullets.Clear();
        enemy_bullets.Clear();
        enemy_bullet_dirs.Clear();
        enemys.Clear();
        game_state = 0; // ゲーム開始状態
        game_stage = 1;
        player_x = 350;
        player_y = 800;
        player_speed = 3;
        player_health = 1; // プレイヤーの初期体力
        player_weapon =1; //weapon1,2,3
        player_bullet_cooldown_timer = 0; // クールダウンタイマーの初期化

        hasTreasure = false; // 宝物を持っていない状態
        treasure_collected = false;
        hasTreasure2 = false; // 宝物2を持っていない状態
        treasure_collected2 = false;

        enemy_stage = 1; // stage1,2,3
        enemy_speed = 1; 
        enemy_health = 3; // 敵の初期体力
        enemy_number = 3; // 敵の初期数 
        enemy_kill_count = 0;

        for (int i = 0; i < enemy_number; i++)
        {
            enemys.Add(RandomEnemySpawn());
        }
        enemy_bullet_speed = 3; // 敵の弾の速度
        enemy_bullet_cooldown = 400; // 敵の弾のクールダウン時間
        enemy_bullet_cooldown_timer = 0; 

        stageTransition = false;

        time = 0;

    }
    Enemy RandomEnemySpawn()
    {
        float x = UnityEngine.Random.Range(40f, 680f); 
        float y = UnityEngine.Random.Range(-160f, -60f); // 敵のスポーン位置
        int hp = enemy_health; // 敵の初期体力
        return new Enemy(new float2(x, y), hp);
    }
    void AddEnemyBullet(float2 pos, float2 dir)
    {
        enemy_bullets.Add(pos);
        enemy_bullet_dirs.Add(math.normalize(dir)); 
    }

    /// <summary>
    /// 動きなどの更新処理
    /// </summary>
    public override void UpdateGame()
    {
        if (game_state == 0) // ゲーム初期化状態
        {
            gc.PlaySound(GcSound.Juicyloop,GcSoundTrack.BGM1,true);
            if (gc.GetPointerFrameCount(0) ==1)
            {
                InitGame(); // ゲームを再起動
                gc.StopSound(GcSoundTrack.BGM1);
                game_state = 1; // ゲームプレイ状態に戻す
            }
        }
        if (game_state == 1) // ゲームプレイ中
        {
            gc.PlaySound(GcSound.Loop,GcSoundTrack.BGM2,true);
            time++;

            if (time == 2500){
                treasure_pos = new float2(UnityEngine.Random.Range(50f, 650f), UnityEngine.Random.Range(100f, 800f));
                hasTreasure = true;
                treasure_collected = false;
            }

            if (time == 5000){
                treasure_pos2 = new float2(UnityEngine.Random.Range(50f, 650f), UnityEngine.Random.Range(100f, 800f));
                hasTreasure2 = true;
                treasure_collected2 = false;
            }
            PlayerMove();

            if (hasTreasure && !treasure_collected){
                if (gc.CheckHitRect(treasure_pos.x, treasure_pos.y, 40, 40, player_x, player_y, 50, 60))
                {
                    treasure_collected = true;
                    player_weapon = math.min(player_weapon + 1, 3); 
                }
            }

            if (hasTreasure2 && !treasure_collected2){
                if (gc.CheckHitRect(treasure_pos2.x, treasure_pos2.y, 40, 40, player_x, player_y, 50, 60))
                {
                    treasure_collected2 = true;
                    player_weapon = math.min(player_weapon + 1, 3); 
                }
            }

            PlayerAttack();
            if (player_bullet_cooldown_timer > 0)
            {
                player_bullet_cooldown_timer--;
            }
            
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
                    gc.PlaySound(GcSound.Explode);
                    enemy_kill_count++;
                    newEnemies.Add(RandomEnemySpawn());
                    if (game_stage ==1 && enemy_kill_count == 4)
                    {
                        enemys.Clear();
                        enemy_bullets.Clear();
                        player_bullets.Clear();
                        enemy_bullet_dirs.Clear();
                        stageTransition = true; 
                        game_stage = 2; 
                        player_health = 3;
                        enemy_kill_count = 0;
                        enemy_stage = 2;
                        EnemyStageUPdate();
                        
                    }
                    if (game_stage ==2 && enemy_kill_count == 3)
                    {
                        enemys.Clear();
                        enemy_bullets.Clear();
                        player_bullets.Clear();
                        enemy_bullet_dirs.Clear();
                        stageTransition = true;
                        game_stage = 3; 
                        player_health = 5;
                        enemy_kill_count = 0;
                        enemy_stage = 3;
                        EnemyStageUPdate();
                    }
                    if (game_stage ==3 && enemy_kill_count == 1)
                    {
                        gc.StopSound(GcSoundTrack.BGM2);
                        game_state = 3; // ゲームオーバー状態に移行
                    }
                }
            }
            enemys.RemoveAll(e => e.hp <= 0); 
            if (!stageTransition) // ステージ遷移中でない場合のみ新しい敵を追加
            {
                enemys.AddRange(newEnemies);
            }
            if (stageTransition){
                for (int i = 0; i < enemy_number; i++)
                {
                    enemys.Add(RandomEnemySpawn()); 
                }
            }
            stageTransition = false; // ステージ遷移フラグをリセット

            enemy_bullet_cooldown_timer--; // 敵の弾のクールダウンタイマーを減少
            if (enemy_bullet_cooldown_timer <= 0) // 敵の弾を発射
            {
                foreach (var enemy in enemys){
                    if (enemy_stage ==1)
                    {
                        AddEnemyBullet(enemy.pos + new float2(25, 20), new float2(0, 1));
                        AddEnemyBullet(enemy.pos + new float2(25, 60), new float2(0, 1));
                    }
                    if (enemy_stage == 2)
                    {
                        float2[] directions = {
                            new float2(-1, 1), // 左下
                            new float2(0, 1),  // 正下
                            new float2(1, 1),  // 右下
                        };

                        foreach (var dir in directions)
                        {
                            AddEnemyBullet(enemy.pos + new float2(25, 20) + dir * 1, dir);
                            AddEnemyBullet(enemy.pos + new float2(25, 60) + dir * 2, dir);
                        }
                    }
                    if (enemy_stage == 3)
                    {
                        int[] bulletCounts = { 12, 16, 20, 24 }; // 弾の数の候補
                        int randomCount = bulletCounts[UnityEngine.Random.Range(0, bulletCounts.Length)];
                        ShootCircularBullet(enemy, randomCount);
                    }
                }
                enemy_bullet_cooldown_timer = enemy_bullet_cooldown; // クールダウンタイマーをリセット
            }

            EnemyBulletMove(); // 敵の弾の移動処理
            EnemyHitPlayer(); // 敵の弾がプレイヤーに当たったかチェック

        }
        if (game_state == 2) // ゲームオーバー状態
        {
            gc.PlaySound(GcSound.Juicyloop,GcSoundTrack.BGM1,true);
            if (gc.GetPointerFrameCount(0) ==10)
            {
                gc.StopSound(GcSoundTrack.BGM1);
                InitGame(); // ゲームを再起動
                game_state = 1; // ゲームプレイ状態に戻す
            }
        }
        if (game_state == 3) // ゲームクリア状態
        {
            gc.PlaySound(GcSound.Juicyloop,GcSoundTrack.BGM1,true);
            if (gc.GetPointerFrameCount(0) ==10)
            {
                gc.StopSound(GcSoundTrack.BGM1);
                InitGame(); // ゲームを再起動
                game_state = 1; // ゲームプレイ状態に戻す
            }
        }
    }

    /// <summary>
    /// 描画の処理
    /// </summary>
    public override void DrawGame()
    {
        if (game_state == 0) // ゲーム初期化状態
        {
            gc.ClearScreen();
            gc.DrawImage(GcImage.BackGround, 0, 0);
            gc.DrawImage(GcImage.Title, 0, 0);
            gc.SetColor(255, 255, 255);
            gc.SetFontSize(72);
            gc.DrawString("Tap to Start", 150, 800);
            gc.SetFontSize(48);
            gc.DrawString("Press to Restart", 180, 900);
        }
        if (game_state == 1) // ゲームプレイ中
        {
            gc.ClearScreen();
            gc.DrawImage(GcImage.BackGround, 0, 0);
            gc.DrawImage(GcImage.Player1, player_x, player_y);
            // bulletの位置を更新
            gc.SetColor(240, 255, 255);
            for (int i = 0; i < player_bullets.Count; i++)
            {
                float2 b = player_bullets[i];
                gc.FillRect(b.x, b.y, 5, 10);
            }

            gc.SetColor(0, 255, 0);
            foreach (var bullet in enemy_bullets)
            {
                gc.FillRect(bullet.x, bullet.y, 5, 10);
            }

            if (hasTreasure && !treasure_collected)
            {
                gc.DrawImage(GcImage.Treasure, treasure_pos.x, treasure_pos.y);
            }
            if (hasTreasure2 && !treasure_collected2)
            {
                gc.DrawImage(GcImage.Treasure, treasure_pos2.x, treasure_pos2.y);
            }

            // 敵の描画
            if (enemy_stage == 1 || enemy_stage == 2)
            {
                gc.SetColor(255, 0, 0);
                foreach (var enemy in enemys)
                {
                    gc.DrawImage(GcImage.Enemy1, enemy.pos.x, enemy.pos.y);
                    gc.DrawString($"HP:{enemy.hp}", enemy.pos.x - 20, enemy.pos.y - 40);
                }
            }
            if (enemy_stage == 3)
            {
                gc.SetColor(255, 0, 0);
                foreach (var enemy in enemys)
                {
                    gc.DrawImage(GcImage.Boss, enemy.pos.x, enemy.pos.y);
                    gc.DrawString($"HP:{enemy.hp}", enemy.pos.x, enemy.pos.y - 40);
                }
            }

            gc.SetColor(255, 255, 255);
            gc.SetFontSize(48);
            // gc.DrawString($"Player Stage: {player_stage}", 20, 10);
            gc.DrawString($"Killed Enemy: {enemy_kill_count}", 20, 50);
            gc.DrawString($"Player Health: {player_health}", 20, 100);
            // gc.DrawString($"Time: {time}", 20, 150);
            gc.DrawString($"Game Stage: {game_stage}/3", 20, 10);
            gc.DrawImage(GcImage.Panel, 0, 880);
        }
        if (game_state == 2) // ゲームオーバー
        {
            gc.ClearScreen();
            gc.DrawImage(GcImage.BackGround, 0, 0);
            gc.SetColor(255, 255, 255);
            gc.SetFontSize(72);
            gc.DrawString("Game Over", 200, 600);
            gc.SetFontSize(48);
            gc.DrawString("Press to Restart", 180, 700);
        }
        if (game_state == 3) // ゲームクリア
        {
            gc.ClearScreen();
            gc.DrawImage(GcImage.BackGround, 0, 0);
            gc.SetColor(255, 255, 255);
            gc.SetFontSize(72);
            gc.DrawString("Game Clear", 200, 600);
            gc.SetFontSize(48);
            gc.DrawString("Press to Restart", 180, 700);
        }
    }

    void PlayerMove (){
        if (gc.GetPointerFrameCount(0) > 0){  // ポインターが押されていない場合は何もしない
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

    void PlayerAttack() {
        if (gc.GetPointerFrameCount(0) ==1 && player_bullet_cooldown_timer <= 0) {
            float px = gc.GetPointerX(0);
            float py = gc.GetPointerY(0);

            if (400 < px && px < 700 && 900 < py && py < 1200) {
                // 弾を発射
                for (int i = 0; i < player_bullet_number; i++) {
                    float2 bulletPosition = new float2(player_x + 25, player_y + (i * 30));
                    player_bullets.Add(bulletPosition);
                }
                gc.PlaySound(GcSound.Shoot); // 弾を発射する音を再生
                player_bullet_cooldown_timer = player_bullet_cooldown; // クールダウンタイマーを設定
            }
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
            player_bullet_speed = 6;
            player_bullet_damage = 1;
            player_bullet_cooldown = 15;
            player_bullet_number = 2;
        }
        if (player_weapon == 3) {
            player_bullet_speed = 7;
            player_bullet_damage = 1;
            player_bullet_cooldown = 10;
            player_bullet_number = 2;
        }
    }

    void EnemyStageUPdate() {
        if (enemy_stage == 1) {
            enemy_health = 3;
            enemy_number = 3;
            enemy_bullet_cooldown = 400;
        }
        if (enemy_stage == 2) {
            enemy_health = 12;
            enemy_number = 2;
            enemy_bullet_cooldown = 250;
        }
        if (enemy_stage == 3) {
            enemy_health = 30;
            enemy_number = 1;
            enemy_bullet_cooldown = 200;
        }
    }

    void EnemyMove() {
        if (enemy_stage == 1 || enemy_stage == 2){
            for (int i = 0; i < enemys.Count; i++)
            {
                Enemy e = enemys[i];
                e.pos.y += enemy_speed;
                enemys[i] = e;
            }
        }
        if (enemy_stage == 3){
            for (int i = 0; i < enemys.Count; i++)
            {
                Enemy e = enemys[i];
                e.pos.y += enemy_speed;
                if (e.pos.y > 580) // 敵が画面外に出たら
                {
                    e.pos.x = UnityEngine.Random.Range(40f, 680f); // 新しいx座標をランダムに設定
                    e.pos.y = UnityEngine.Random.Range(20f, 400f); // y座標を画面上部にリセット
                }
                enemys[i] = e;
            }
        }
    }

    void EnemyGetHit(){
        for (int i = 0; i < enemys.Count; i++)
        {
            Enemy e = enemys[i];
            for (int j = 0; j < player_bullets.Count; j++)
            {
                float2 b = player_bullets[j];
                if (enemy_stage != 3 && gc.CheckHitRect(b.x, b.y, 5, 10, e.pos.x, e.pos.y, 50, 60))
                {
                    e.hp -= player_bullet_damage;
                    player_bullets.RemoveAt(j);
                    break;
                }
                if (enemy_stage == 3 && gc.CheckHitRect(b.x, b.y, 5, 10, e.pos.x, e.pos.y, 100, 120))
                {
                    e.hp -= player_bullet_damage;
                    player_bullets.RemoveAt(j);
                    break;
                }
            }
            enemys[i] = e;
        }
    }

    void EnemyBulletMove() {
        
        for (int i = 0; i < enemy_bullets.Count; i++)
        {
            enemy_bullets[i] += enemy_bullet_dirs[i] * enemy_bullet_speed;
        }
        for (int i = enemy_bullets.Count - 1; i >= 0; i--)
        {
            if (enemy_bullets[i].y > 1280 || enemy_bullets[i].x < 0 || enemy_bullets[i].x > 720)
            {
                enemy_bullets.RemoveAt(i);
                enemy_bullet_dirs.RemoveAt(i);
            }
        }
    }

    void EnemyHitPlayer(){
        for (int i = enemy_bullets.Count - 1; i >= 0; i--)
        {
            float2 b = enemy_bullets[i];
            if (gc.CheckHitRect(b.x, b.y, 5, 10, player_x, player_y, 50, 60))
            {
                player_health--; // プレイヤーの体力を減少
                enemy_bullets.RemoveAt(i);
            }
            if (player_health <= 0)
            {
                // プレイヤーが倒された場合の処理
                gc.PlaySound(GcSound.Explode); // ゲームオーバーの音を再生
                gc.StopSound(GcSoundTrack.BGM2);
                game_state = 2; // ゲームオーバー状
            }
        }
    }

    void ShootCircularBullet(Enemy enemy, int count){
        for (int i = 0; i < count; i++)
        {
            float angle = math.radians(360f / count * i); // 每个弹的角度
            float2 dir = new float2(math.cos(angle), math.sin(angle));
            AddEnemyBullet(enemy.pos +  new float2(50, 100) + dir, dir);
        }
    }
}
