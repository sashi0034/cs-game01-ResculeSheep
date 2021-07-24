using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DxLibDLL;
using System.Drawing;
using System.IO;


namespace Test_test03
{

    static class ProgramProperty
    {
        public const string version = "1.0.0";
        public const string published = "2021/06/24";
        public const string maker = "sashi";
    }

    static class Program
    {
        const int SHOT_STRAIGHT = 0;
        const int SHOT_LEFT = 1;

        const int DRAW_WIDTH = 256;
        const int DRAW_HEIGHT = 224;

        static public string[] MapMatrix = new string[0];

        static Random rand = new Random();



        class Sheep
        {
            public static int Max = 0;
            public static int Number = 0;
            public bool used = false;
            public float x, y;
            public int sp;
            public float vx, vy;

            public Sheep(int X, int Y)
            {
                x = X;
                y = Y;
                used = true;
                sp = Sprite.Set(-1,0,0,32,32);

                vx = 1;

                
                if (rand.Next(1,3)==1) vx *= -1;

                Sheep.Max++;
                Sheep.Number++;
            }
        }



        class Block
        {
            public int x, y;
            public bool used;
            public Block(int X, int Y)
            {
                x = X;
                y = Y;
                used = true;
            }
        }






        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            int draw_scale = 3;

            // ウインドウモードで起動
            DX.ChangeWindowMode(DX.TRUE);
            DX.SetGraphMode(DRAW_WIDTH*draw_scale, DRAW_HEIGHT*draw_scale, 16);

            DX.SetAlwaysRunFlag(1); //非アクティブ状態でも動かす

            // ＤＸライブラリの初期化
            if (DX.DxLib_Init() < 0)
            {
                return;
            }
            Sprite.Init(); // スプライトの初期化

            DX.SetTransColor(0, 0, 0);
            DX.SetDrawMode(DX.DX_DRAWMODE_NEAREST);
            DX.SetMainWindowText("羊救出大作戦");

            int[] Stage_High = new int[10];
            for (int i = 0; i < 10; i++) Stage_High[i] = 0;


            // 画像の読み込み
            int Hndl_ground = DX.LoadGraph(@"Assets\Images\横スク地面.png");
            int Hndl_block = DX.LoadGraph(@"Assets\Images\顔ブロック2f16.png");
            int Hndl_ball = DX.LoadGraph(@"Assets\Images\茶玉.png");
            int Hndl_balltrace = DX.LoadGraph(@"Assets\Images\球状飛行機雲.png");
            int Hndl_player = DX.LoadGraph(@"Assets\Images\地面スライダー32_12.png");
            int Hndl_sheep = DX.LoadGraph(@"Assets\Images\ひつじ2f32.png");
            int Hndl_star = DX.LoadGraph(@"Assets\Images\星4f24.png");

            int Hndl_MenuBack = DX.LoadGraph(@"Assets\Images\森風ウィンドウ256_224.png");
            int Hndl_numple = DX.LoadGraph(@"Assets\Images\ナンバープレート9f48_32.png");
            int Hndl_peke = DX.LoadGraph(@"Assets\Images\ペケ.png");

            int Hndl_Title = DX.LoadGraph(@"Assets\Images\羊救出大作戦タイトル.png");

            int se_scene = DX.LoadSoundMem(@"Assets\Sounds\シーン切り替え.mp3");
            //DX.PlaySoundMem(se_scene, DX.DX_PLAYTYPE_BACK);
            int se_ok = DX.LoadSoundMem(@"Assets\Sounds\決定、ボタン押下22.mp3");
            int se_sheep = DX.LoadSoundMem(@"Assets\Sounds\ヒツジの鳴き声.mp3");
            int se_star = DX.LoadSoundMem(@"Assets\Sounds\きらーん.mp3");
            int se_jamp = DX.LoadSoundMem(@"Assets\Sounds\ジャンプ.mp3");
            int se_bound = DX.LoadSoundMem(@"Assets\Sounds\ぷよん.mp3");
            int se_kong = DX.LoadSoundMem(@"Assets\Sounds\試合終了のゴング.mp3");


            //タイルマップレイヤーの作成
            int map_width = 16;
            int map_height = 14;

            int Hndl_BackScreen = DX.MakeScreen(map_width * 16, map_height * 16, 0);

            int Hndl_DrawScreen = DX.MakeScreen(map_width * 16, map_height * 16, 0);

            int Hndl_font12 = DX.CreateFontToHandle(null, 12, 1, DX.DX_FONTTYPE_NORMAL);



            Title_Loop();
            sav_load();
            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^ ループ地点
            Label_Main:
            //----------------------------


            

            List<Sheep> sheep = new List<Sheep>(0);
            Sheep.Max = 0;
            Sheep.Number = 0;

            int loop_cnt = 0;

            int player_sp;
            int player_x = 8 * 16, player_y = 12 * 16 + 8;

            int ball_sp;
            float ball_x = 8 * 16, ball_y = 8 * 16;
            float ball_vx = 0, ball_vy = 1;
            float ball_speedNormal = 2f;
            float ball_speed = ball_speedNormal;
            int ball_Mode = 0;

            int Game_Score = 0;

            int stage_select = 0;

            int Game_State = 0;
            const int GAME_PLAYING = 0;
            const int GAME_CLEAR = 1;
            const int GAME_OVER = 2;
            int finish_cnt = 0;
            bool Game_retry = false;


            List<Block> block = new List<Block>(0);


            Menu_Loop();

            if (stage_select>0) Main_Loop();

            if (Game_retry) goto Label_Main; //Goto文

            //int test_sp = Sprite.Set(Hndl_ball,0,0,16,16);
            //Sprite.Offset(test_sp,50,player_y,100);
            /*
            Sprite.Anim(test_sp, -Sprite.AnimType_XY,
                60,100,100,
                60,200,100,
                60,100,200,
                0);
            */
            //Useful.Sprite_ParabolaAnim(test_sp, 50, 50, 200, 200, 5, 2);
            //Useful.Clash_effect(Hndl_block, 100,100);

           

            // ＤＸライブラリの後始末
            DX.DxLib_End();
            Sprite.End(); //スプライトの後始末
            return;
            // ===================================================================== program終了


            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ メインループ
            void Main_Loop()
            {
                player_sp = Sprite.Set(Hndl_player, 0, 0, 32, 12);
                ball_sp = Sprite.Set(Hndl_ball, 0, 0, 16, 16);

                Map_Load();

                DX.SetDrawScreen(Hndl_BackScreen);
                DX.DrawBox(0, 0, DRAW_WIDTH, DRAW_HEIGHT, DX.GetColor(100, 240, 240), DX.TRUE);

                while (DX.ProcessMessage() != -1)
                {

                    if (DX.CheckHitKey(DX.KEY_INPUT_ESCAPE) == 1) //escape押されたら終了
                    {
                        break;
                    }

                    if (Game_State != GAME_PLAYING) finish_cnt++;


                    if (finish_cnt>180)  //ゲーム終了
                    {
                        if (Game_State == GAME_CLEAR)
                        {
                            Stage_High[stage_select] = Math.Max(Stage_High[stage_select],Game_Score);
                            sav_save();
                            DX.PlaySoundMem(se_scene, DX.DX_PLAYTYPE_BACK);
                        }
                        Sprite.Clear();
                        break;
                    }

                    Player_Update();
                    Sheep_Update();
                    Ball_Update();

                    Sprite.AllUpdate();

                    Screen_Update();
                    loop_cnt++;
                }

            }// --------------------------------------------------------- メインループ





            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^メニューループ
            void Menu_Loop()
            {
                int px0 = 48, py0 = 56;
                int pw0 = 56, ph0 = 48;
                int cx = 0, cy=0, csp;
                bool pushf = false;
                bool push0f = false;
                //Console.WriteLine("menu_loop");
                Menu_Start();

                while (DX.ProcessMessage() != -1)
                {

                    if (push0f && DX.CheckHitKey(DX.KEY_INPUT_ESCAPE) == 1) //escape押されたら終了
                    {
                        break;
                        stage_select = 0;
                    }

                    if (DX.CheckHitKey(DX.KEY_INPUT_RETURN) == 1)
                    {
                        //Console.WriteLine("enter");
                        stage_select = cx + cy * 3 + 1;
                        Game_retry = true;
                        DX.PlaySoundMem(se_ok, DX.DX_PLAYTYPE_BACK);
                        break;
                    }
                    else
                    {
                        push0f = true;
                    }

                    Menu_Update();

                    Screen_Project();
                }
                Sprite.Clear();
                return;
                //================== 終了


                //開始処理
                void Menu_Start() 
                { 
                    DX.SetDrawScreen(Hndl_BackScreen);
                    DX.DrawGraph(0, 0, Hndl_MenuBack, DX.TRUE);
                    //DX.DrawGraph(0, 0, Hndl_numple, DX.TRUE);

                    for (int i=0; i<9; i++)
                    {
                        int x1 = i % 3, y1 = i / 3;
                        int sp = Sprite.Set(Hndl_numple, x1*48, y1*32, 48, 32);

                        Sprite.Offset(sp, px0 + x1*pw0, py0 + y1*ph0, 0);



                        if (Stage_High[i + 1] > 0)
                        {
                            int sp1 = Sprite.Set(Hndl_peke, 0, 0, 32, 32);
                            Sprite.Offset(sp1, px0 + x1 * pw0, py0 + y1 * ph0 - 4, 0);
                        }

                    }

                    Useful.DrawString_bordered(80,28,"STAGE SELECT");

                    csp = Sprite.Set(Hndl_sheep, 0,0,32,32);
                    Sprite.Anim(csp,Sprite.AnimType_UV,
                        30,0,0,
                        30,32,0,
                        0);
                }


                //更新処理
                void Menu_Update()
                {
                    if (!pushf)
                    {
                        if (Button.Left()) { cx--; pushf = true; }
                        if (Button.Right()) { cx++; pushf = true; }
                        if (Button.Up()) { cy--; pushf = true; }
                        if (Button.Down()) { cy++; pushf = true; }
                    }
                    if (pushf && !(Button.Left() || Button.Right() || Button.Up() || Button.Down())) pushf = false;

                    cx = cx % 3;if (cx < 0) cx = 2;
                    cy = cy % 3; if (cy < 0) cy = 2;

                    Sprite.Offset(csp, px0 + cx * pw0 + 16, py0 + cy * ph0, -50);


                    DX.SetDrawScreen(Hndl_DrawScreen);
                    DX.DrawGraph(0, 0, Hndl_BackScreen, DX.TRUE);

                    //ハイスコア表示
                    for (int i = 0; i < 9; i++)
                    {
                        int x1 = i % 3, y1 = i / 3;
                        if (Stage_High[i + 1] == 0) continue;
                        //Useful.DrawString_bordered(px0 + x1 * pw0, py0 + y1 * ph0+32, .ToString());
                        DX.DrawStringToHandle(px0 + x1 * pw0, py0 + y1 * ph0 + 32, 
                            Stage_High[i + 1].ToString(), DX.GetColor(200,200, 255), Hndl_font12);

                    }

                        Sprite.Drawing();

                }
            }
            //-----------------------------------------------------------



            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^タイトルループ
            void Title_Loop()
            {
                Title_Start();

                bool f = false;
                while (DX.ProcessMessage() != -1)
                {

                    if (DX.CheckHitKey(DX.KEY_INPUT_ESCAPE) == 1) //escape押されたら終了
                    {
                        break;

                    }

                    if (DX.CheckHitKey(DX.KEY_INPUT_RETURN) == 1)
                    {
                        f = true;
                        DX.PlaySoundMem(se_ok, DX.DX_PLAYTYPE_BACK);
                        break;
                    }

                    Title_Update();

                    Screen_Project();
                }
                //Sprite.Clear();
                
                if (f) Thread.Sleep(100);


                return;
                //================== 終了


                //開始処理
                void Title_Start()
                {
                    DX.SetDrawScreen(Hndl_BackScreen);
                    DX.DrawGraph(0, 0, Hndl_Title, DX.TRUE);
                    //DX.DrawGraph(0, 0, Hndl_numple, DX.TRUE);

                    Useful.DrawString_shadow(0, 0, "ver"+ProgramProperty.version + "("+ ProgramProperty.published+ ")");
                    Useful.DrawString_shadow(0, 224-16, "a game by " + ProgramProperty.maker);

                    Useful.DrawString_bordered(64, 224-32, "plese push Enter");
                }

                //更新処理
                void Title_Update()
                {
 

                    DX.SetDrawScreen(Hndl_DrawScreen);
                    DX.DrawGraph(0, 0, Hndl_BackScreen, DX.TRUE);


                }

            }
            //-----------------------------------------------------------




            //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ バイナリファイル読み書き
            void sav_save()
            {
                string path = "data.sav";
                byte[] binary = new byte[2 * 10];

                for (int i=0; i<=9; i++)
                {
                    binary[i*2 + 0] = (byte)(Stage_High[i]/0xFF);
                    binary[i*2 + 1] = (byte)(Stage_High[i] % 0xFF);
                }
                File.WriteAllBytes(path, binary);
            }
            //--------------------------------
            void sav_load()
            {
                string path = "data.sav";

                if (!File.Exists(path)) return;

                byte[] binary = File.ReadAllBytes(path);

                for (int i = 0; i <= 9; i++)
                {
                    Stage_High[i] = binary[i * 2 + 0]*0xFF + binary[i * 2 + 1];

                }
                File.WriteAllBytes(path, binary);
            }








            //^^^^^^^^^ マップ読み込み
            void Map_Load()
            {

                switch (stage_select)
                {
                    case 0:
                        MapMatrix = new string[14] {
                        "                ",
                        " b c            ",
                        "                ",
                        " g              ",
                        "                ",
                        " s              ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        };
                        break;
                    case 1:
                        MapMatrix = new string[14] {
                        "                ",
                        "   bbbbbbbbbb   ",
                        " bbbb      bbbb ",
                        " bb g      g bb ",
                        " bb g  s   g bb ",
                        " bb gggggggg bb ",
                        " bbb        bbb ",
                        "   bbbbbbbbbb   ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        };
                        break;
                    case 2:
                        MapMatrix = new string[14] {
                        "  s             ",
                        " bbbbbbbbbbbbbb ",
                        " bb         s b ",
                        " bb    gggggggg ",
                        " bb    bb       ",
                        " bb    bb       ",
                        " bbbbbbbb       ",
                        " bbbbbbbb       ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        };
                        break;
                    case 3:
                        MapMatrix = new string[14] {
                        "                ",
                        "   bbbbbbbbbb   ",
                        " bbbb  gg   bbb ",
                        " bb  s gg  s bb ",
                        " bbggggggggggbb ",
                        " bb    gg    bb ",
                        " bb  s gg s bbb ",
                        "   bbbbbbbbbb   ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        };
                        break;
                    case 4:
                        MapMatrix = new string[14] {
                        "                ",
                        " bbbbbbbbbbbbbb ",
                        " b s          b ",
                        "bbggggggggggggbb",
                        "bbb          bbb",
                        "                ",
                        "bbb  s   s   bbb",
                        "bbbggggggggggbbb",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        };
                        break;
                    case 5:
                        MapMatrix = new string[14] {
                        "     c     c    ",
                        "gggggc     c    ",
                        "     c     c    ",
                        "     c     cbbbb",
                        "  s  c     cbbbb",
                        "gggggc     c    ",
                        "  s  c     c s  ",
                        "bbbbbc     cbbbb",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        };
                        break;
                    case 6:
                        MapMatrix = new string[14] {
                        "   s            ",
                        " bbbbbb       b ",
                        " b    b   s   b ",
                        " b s  bcccccccb ",
                        " b            b ",
                        " bggggggggggggb ",
                        "                ",
                        "cccccccccccccccc",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        };
                        break;
                    case 7:
                        MapMatrix = new string[14] {
                        "   s s s s s s s",
                        "  bbbbbbbbbbbbbb",
                        "                ",
                        "cccccccccccccc  ",
                        "                ",
                        "  gggggggggggggg",
                        "                ",
                        "bbbbbbbbbbbbbbbb",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        };
                        break;
                    case 8:
                        MapMatrix = new string[14] {
                        "      bbbb      ",
                        "     bb  bb     ",
                        "    bb s  bb    ",
                        "   bbccccccbb   ",
                        "  bbc      cbb  ",
                        " bb  c s  c  bb ",
                        "bb s  gggg s  bb",
                        "bbbbbbbbbbbbbbbb",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        "                ",
                        };
                        break;
                    case 9:
                        MapMatrix = new string[14] {
                        "bbbbbbbbbbbbbbbb",
                        "bcs s s       cb",
                        "bcbbbbggggggg cb",
                        "bcgggg  s s s cb",
                        "bc s  ggggggggcb",
                        "bc gggbbbbbbb cb",
                        "bc s   s      cb",
                        "bcbbbbbbbbbbbbcb",
                        "                ",
                        " s              ",
                        " cc             ",
                        "                ",
                        "                ",
                        "                ",
                        };
                        break;

                }






                for (int x = 0; x < map_width; x++)
                {
                    for (int y = 0; y < map_height; y++)
                    {
                        if (MapMatrix[y][x] == 'b' || MapMatrix[y][x] == 'g' || MapMatrix[y][x] == 'c')
                        {
                            block.Add(new Block(x * 16, y * 16));
                        }

                        if (MapMatrix[y][x] == 's')
                        {
                            sheep.Add(new Sheep(x * 16, y * 16));
                        }
                    }
                }
            }





            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ ひつじ処理
            void Sheep_Update()
            {
                for (int i=0; i<Sheep.Max; i++)
                {
                    if (sheep[i].used) {
                        int u = ((int)(loop_cnt % 60) / 30) *32;

                        float x0 = sheep[i].x, y0 = sheep[i].y;
                        float vx = sheep[i].vx, vy = sheep[i].vy;

                        int w0 = 14, h0 = 12;
                        int dx0 = (16 - w0) / 2, dy0 = 16 - h0;

                        //^^^^^ 位置計算

                        if (Block_Hit((int)(x0+dx0 + vx), (int)(y0+dy0), w0, h0)!=-1) vx *= -1;
                        x0 += vx;

                        if (x0 < 0)//画面外に行かないように
                        {
                            x0 = 0;
                            vx *= -1;
                        }
                        if (x0>256-32)//画面外に行かないように
                        {
                            x0 = 256 - 32;
                            vx *= -1;
                        }


                        vy = vy + 0.1f;
                        if (Block_Hit((int)(x0+dx0), (int)(y0+dy0 + vy), w0, h0)==-1) y0 += vy;
                        if (vy > 0 && Block_Hit((int)(x0 + dx0), (int)(y0 + dy0 + vy), w0, h0) != -1) //ジャンプ
                        {
                            vy = (float)(-1.5 - rand.Next(0, 100) / 100f);
                            //Console.WriteLine($"vy = {vy}");
                        }
                        if (vy > 2) vy = 2; //終端落下速度


                        if (vx > 0) Sprite.Attribution(sheep[i].sp, Sprite.Attribution_reverse);
                            else Sprite.Attribution(sheep[i].sp, 0);



                        //-----

                        sheep[i].x = x0; sheep[i].y = y0;
                        sheep[i].vx = vx; sheep[i].vy = vy;


                        Sprite.Image(sheep[i].sp, Hndl_sheep, u,0,32,32);
                        Sprite.Offset(sheep[i].sp, sheep[i].x-8, sheep[i].y-16);

                        
                        if (rand.Next(0, 256) == 0)//羊の鳴き声
                        {
                            DX.PlaySoundMem(se_sheep, DX.DX_PLAYTYPE_BACK);
                        }


                        if (sheep[i].y > 512)//ひつじ落下死
                        {
                            sheep[i].used = false;
                            Game_State = GAME_OVER;
                            DX.PlaySoundMem(se_kong, DX.DX_PLAYTYPE_BACK);
                        }

                        if (Sheep_HitPlayer((int)sheep[i].x, (int)sheep[i].y)) //ひつじキャッチ
                        {
                            Sheep_GetEffect((int)sheep[i].x, (int)sheep[i].y);
                            sheep[i].used = false;
                            Sprite.Clear(sheep[i].sp);
                            Game_Score += 1000;
                            Sheep.Number--;
                            DX.PlaySoundMem(se_star, DX.DX_PLAYTYPE_BACK);
                            if (Sheep.Number <= 0)//ゲームクリア
                            {
                                Game_State = GAME_CLEAR;
                                DX.PlaySoundMem(se_kong, DX.DX_PLAYTYPE_BACK);
                            }
                        }


                    }
                }
            }

            
            //ひつじゲットエフェクト
            void Sheep_GetEffect(int x0, int y0)
            {
                for (int a=0; a<12; a++)
                {
                    int sp = Sprite.Set(Hndl_star,0,0,24,24);

                    int x1 = x0 + 4, y1 = y0 + 4;
                    int x2 = x1 + (int)(Math.Cos(Math.PI * (a * 30f / 180f)) * 128);
                    int y2 = y1 + (int)(Math.Sin(Math.PI * (a * 30f / 180f)) * 128);

                    Sprite.Offset(sp, x1, y1);

                    Sprite.Anim(sp, Sprite.AnimType_XY
                        ,-10, x2, y2
                        ,1,x1,y1
                        ,-10,x2,y2
                        ,3);
                    Sprite.Anim(sp, Sprite.AnimType_UV
                        ,5, 24 * 0, 0
                        ,5, 24 * 1, 0
                        ,5, 24 * 2, 0
                        ,5, 24 * 3, 0
                        ,0
                        );
                    
                    Sprite.sprite[sp].Update += new SpriteCompornent.UpdateDelegate(Useful.Sprite_EffeectfadeXY);
                }
            }


            bool Sheep_HitPlayer(int x, int y) //当たり判定
            {
                
                Rectangle r1 = new Rectangle(x,y+16,32,16);
                Rectangle r2 = new Rectangle(player_x,player_y,32,12);
                return r1.IntersectsWith(r2);
            }

            int Block_Hit(int x, int y, int w, int h) //ブロック当たり判定
            {
                Rectangle r1 = new Rectangle(x,y,w,h);

                for (int i=0; i<block.Count(); i++)
                {
                    if (block[i].used)
                    {
                        Rectangle r2 = new Rectangle(block[i].x, block[i].y, 16, 16);
                        if (r1.IntersectsWith(r2))
                        {
                            return i;
                        }
                    }
                }



                return -1;
            }

            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ プレイヤー処理
            void Player_Update()
            {
                int speed = 4;
                if (Button.Left()) { player_x-=speed;}
                if (Button.Right()) { player_x+=speed;}
                Sprite.Offset(player_sp, player_x, player_y);
            }


            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ ボール処理
            void Ball_Update()
            {
                int x, y;
                int bh;
                bool F1;

                if (ball_Mode == 1)//変化球モード
                {
                    if (loop_cnt % 6 == 0)
                    {
                        ball_speed += 0.4f;
                        ball_VelocityCalculate();                    }
                }


                //++++

                x = (int)(ball_x + ball_vx);


                bh = Block_Hit(x, (int)ball_y, 16, 16);

                if (bh!=-1 || x < 0 || x>=(DRAW_WIDTH-16))
                {
                    ball_Mode = 0;
                    ball_speed = (ball_speed < ball_speedNormal) ? ball_speedNormal * 2 : ball_speedNormal;

                    if (bh != -1) block_break(bh); 
                    ball_vx *= -1;
                    ball_VelocityCalculate();
                    DX.PlaySoundMem(se_bound, DX.DX_PLAYTYPE_BACK);
                }
                else
                {
                    ball_x += ball_vx;
                }

                // ++++

                y = (int)(ball_y + ball_vy);

                bh = Block_Hit((int)ball_x, y, 16, 16);
                F1 = Ball_HitPlayer((int)ball_x, y);
                if (bh!=-1 || y < 0 || F1)
                {
                    ball_Mode = 0;
                    ball_speed = (ball_speed < ball_speedNormal) ? ball_speedNormal * 2 : ball_speedNormal;

                    if (bh != -1) block_break(bh);
                    if (F1)//プレイヤーとヒット
                    {
                        ball_y = player_y - 16;
                        ball_vx += (player_x - 128 + 16)/200; //プレイヤーの位置補正で乱雑さを与える

                        if (ball_vx == 0) ball_vx = rand.Next(50, 200)/200f;
                            if (rand.Next(0, 2) == 0) ball_vx *= -1;
                        if (ball_vy == 0) ball_vy = -1;
                        DX.PlaySoundMem(se_jamp, DX.DX_PLAYTYPE_BACK);
                    }
                    else
                    {
                        DX.PlaySoundMem(se_bound, DX.DX_PLAYTYPE_BACK);
                    }
                    ball_vy *= -1;
                    ball_VelocityCalculate();

                }
                else
                {
                    ball_y += ball_vy;
                }

                if (ball_y > 256)
                {
                    if (Game_State == GAME_PLAYING)
                    {
                        Game_State = GAME_OVER;
                        DX.PlaySoundMem(se_kong, DX.DX_PLAYTYPE_BACK);

                    }
                }

                Sprite.Offset(ball_sp, ball_x, ball_y);


                if (loop_cnt % 6 == 0)
                {
                    DX.SetDrawScreen(Hndl_BackScreen);
                    DX.SetDrawBlendMode(DX.DX_BLENDGRAPHTYPE_ALPHA, 40);
                        DX.DrawGraph((int)ball_x, (int)ball_y, Hndl_balltrace, DX.TRUE);
                    DX.SetDrawBlendMode(DX.DX_BLENDGRAPHTYPE_NORMAL, 0);
                }
            }

            void ball_VelocityCalculate()
            {
                float s = (float)Math.Sqrt(ball_vx * ball_vx + ball_vy * ball_vy);
                ball_vx = ball_vx / s * ball_speed;
                ball_vy = ball_vy / s * ball_speed;
            }


            bool Ball_HitPlayer(int x, int y) //当たり判定
            {
                return (Useful.between(x - player_x, -16, -16 + 16 + 16*2) && Useful.between(y - player_y, -16, -16+4));
            }

            
            void block_break(int num) //ブロック破壊
            {
                string s = "";
                int y0 = block[num].y / 16, x0 = block[num].x / 16;

                char r = 'x';

                if (MapMatrix[y0][x0]=='g') //エフェクト発生ブロックだったらエフェクト
                {
                    ball_Mode = 1;
                    ball_speed = 0.01f;
                    Game_Score += 20;
                }
                else
                {
                    if (MapMatrix[y0][x0] == 'c') //お休みブロックなら置き換えするだけ
                    {
                        r = 'b';
                    }
                    else
                    {
                        Game_Score += 10;
                    }
                }


                for (int x=0; x<16; x++)
                {
                    if (x == x0)
                    {
                        s = s + r;
                        continue;
                    }
                    s = s + MapMatrix[y0][x];
                }
                MapMatrix[y0] = s;

                if (r == 'x')
                {
                    Useful.Clash_effect(Hndl_block, x0 * 16 + 4, y0 * 16 + 4);
                    block[num].used = false;
                }

                    //MapMatrix[block[num].y / 16].ChangeCharAt(block[num].x / 16, 'x');
                    //MapMatrix[0].ChangeCharAt(0, 'b');
                    //MapMatrix[block[num].y / 16][block[num].x / 16] = ' ';




                }


            //UI描画
            void UI_Update()
            {
                Useful.DrawString_bordered(0,0, " score: " + Game_Score.ToString());
                
            }






            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ 画面描画更新
            void Screen_Update()
            {
                // 描画先を裏画面に変更
                //DX.SetDrawScreen(DX.DX_SCREEN_BACK);


                // 背景塗りつぶし

                DX.SetDrawScreen(Hndl_DrawScreen);
                    DX.DrawGraph(0, 0, Hndl_BackScreen, DX.TRUE);


                // マップを描画
                //DX.SetDrawScreen(Hndl_TilemapScreen);
                //DX.ClearDrawScreen();
                //DX.DrawBox(0, 0, DRAW_WIDTH, DRAW_HEIGHT, DX.GetColor(0,0,0), DX.TRUE);

                //DX.DrawGraph(0, 0, Hndl_ground, DX.TRUE);
                for (int x=0; x<16; x++)
                    {
                        for (int y=0; y<14; y++)
                    {
                            FormatMaptip_put(x, y, 'g', 'g', Hndl_ground) ;
                            
                            if (MapMatrix[y][x]=='b')
                            {
                            DX.DrawRectGraph(x * 16, y * 16,0,0,16,16, Hndl_block, DX.TRUE);
                            }
                        
                            if (MapMatrix[y][x] == 'c')
                            {
                                 DX.DrawRectGraph(x * 16, y * 16, 16, 0, 16, 16, Hndl_block, DX.TRUE);
                            }

                    }
    
                    }


                //スプライト描画
                Sprite.Drawing();

                //DX.DrawString(0, 0, "frame = "+block.Count().ToString(), DX.GetColor(255,255,255));
                //DX.DrawString(0, 0, DX.GetColor(255,255,255).ToString(), DX.GetColor(255, 255, 255));

                //DX.SetDrawScreen(Hndl_DrawScreen);
                //    DX.DrawGraph(0, 0, Hndl_TilemapScreen, DX.TRUE);

                //UI描画
                UI_Update();

                //特殊処理
                switch (Game_State)
                {
                    case GAME_OVER:
                        Useful.DrawString_bordered(88,224/2-8, "GAME OVER",DX.GetColor(255,100,100));
                        break;
                    case GAME_CLEAR:
                        Useful.DrawString_bordered(80, 224 / 2 - 8, "GAME CLEAR", DX.GetColor(255, 255,255));
                        break;

                }


                Screen_Project();


                }


            //画面を拡大して表示
            void Screen_Project()
            {
                DX.SetDrawScreen(DX.DX_SCREEN_BACK);
                DX.DrawExtendGraph(0, 0, DRAW_WIDTH * draw_scale, DRAW_HEIGHT * draw_scale, Hndl_DrawScreen, DX.FALSE);

                // 裏画面の内容を表画面に反映する
                DX.ScreenFlip();
            }




            //MapMatrix[y][x]に指定文字があるか
            bool char_check(int x, int y, char c)
            {
                if (x < 0 || y < 0 || x >= 16 || y >= 14) return false;
                if (MapMatrix[y][x] == c) return true;
                return false;
            }

            //フォーマットマップチップを描画
            void FormatMaptip_put(int x, int y, char SubjectiveChar, char ObjectiveString, int image48)
            {
                if (MapMatrix[y][x] == SubjectiveChar)
                {

                    if (char_check(x - 1, y, ObjectiveString) == false && char_check(x + 1, y, ObjectiveString) == false &&
                        char_check(x, y - 1, ObjectiveString) == false && char_check(x, y + 1, ObjectiveString) == false)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 0, 0, 8, 8, image48, DX.TRUE, DX.FALSE);
                        DX.DrawRectGraph(x * 16 + 8, y * 16, 40, 0, 8, 8, image48, DX.TRUE, DX.FALSE);
                        DX.DrawRectGraph(x * 16, y * 16 + 8, 0, 40, 8, 8, image48, DX.TRUE, DX.FALSE);
                        DX.DrawRectGraph(x * 16 + 8, y * 16 + 8, 40, 40, 8, 8, image48, DX.TRUE, DX.FALSE);
                        return;
                    }


                    if (char_check(x - 1, y, ObjectiveString) == false && char_check(x + 1, y, ObjectiveString) == false &&
                        char_check(x, y - 1, ObjectiveString) == false && char_check(x, y + 1, ObjectiveString) == true)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 0, 0, 8, 16, image48, DX.TRUE, DX.FALSE);
                        DX.DrawRectGraph(x * 16 + 8, y * 16, 40, 0, 8, 16, image48, DX.TRUE, DX.FALSE);
                        return;
                    }


                    if (char_check(x - 1, y, ObjectiveString) == false && char_check(x + 1, y, ObjectiveString) == false &&
                        char_check(x, y - 1, ObjectiveString) == true && char_check(x, y + 1, ObjectiveString) == false)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 0, 32, 8, 16, image48, DX.TRUE, DX.FALSE);
                        DX.DrawRectGraph(x * 16 + 8, y * 16, 40, 32, 8, 16, image48, DX.TRUE, DX.FALSE);
                        return;
                    }

                    if (char_check(x - 1, y, ObjectiveString) == false && char_check(x + 1, y, ObjectiveString) == false &&
                        char_check(x, y - 1, ObjectiveString) == true && char_check(x, y + 1, ObjectiveString) == true)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 0, 16, 8, 16, image48, DX.TRUE, DX.FALSE);
                        DX.DrawRectGraph(x * 16 + 8, y * 16, 40, 16, 8, 16, image48, DX.TRUE, DX.FALSE);
                        return;
                    }

                    // ++

                    if (char_check(x - 1, y, ObjectiveString) == false && char_check(x + 1, y, ObjectiveString) == true &&
                        char_check(x, y - 1, ObjectiveString) == false && char_check(x, y + 1, ObjectiveString) == false)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 0, 0, 16, 8, image48, DX.TRUE, DX.FALSE);
                        DX.DrawRectGraph(x * 16, y * 16 + 8, 0, 40, 16, 8, image48, DX.TRUE, DX.FALSE);
                        return;
                    }


                    if (char_check(x - 1, y, ObjectiveString) == false && char_check(x + 1, y, ObjectiveString) == true &&
                        char_check(x, y - 1, ObjectiveString) == false && char_check(x, y + 1, ObjectiveString) == true)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 0, 0, 16, 16, image48, DX.TRUE, DX.FALSE);
                        return;
                    }


                    if (char_check(x - 1, y, ObjectiveString) == false && char_check(x + 1, y, ObjectiveString) == true &&
                        char_check(x, y - 1, ObjectiveString) == true && char_check(x, y + 1, ObjectiveString) == false)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 0, 32, 16, 16, image48, DX.TRUE, DX.FALSE);
                        return;
                    }

                    if (char_check(x - 1, y, ObjectiveString) == false && char_check(x + 1, y, ObjectiveString) == true &&
                        char_check(x, y - 1, ObjectiveString) == true && char_check(x, y + 1, ObjectiveString) == true)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 0, 16, 16, 16, image48, DX.TRUE, DX.FALSE);
                        return;
                    }

                    //++++

                    if (char_check(x - 1, y, ObjectiveString) == true && char_check(x + 1, y, ObjectiveString) == false &&
                        char_check(x, y - 1, ObjectiveString) == false && char_check(x, y + 1, ObjectiveString) == false)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 32, 0, 16, 8, image48, DX.TRUE, DX.FALSE);
                        DX.DrawRectGraph(x * 16, y * 16 + 8, 32, 40, 16, 8, image48, DX.TRUE, DX.FALSE);
                        return;
                    }

                    if (char_check(x - 1, y, ObjectiveString) == true && char_check(x + 1, y, ObjectiveString) == false &&
                        char_check(x, y - 1, ObjectiveString) == false && char_check(x, y + 1, ObjectiveString) == true)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 32, 0, 16, 16, image48, DX.TRUE, DX.FALSE);
                        return;
                    }


                    if (char_check(x - 1, y, ObjectiveString) == true && char_check(x + 1, y, ObjectiveString) == false &&
                        char_check(x, y - 1, ObjectiveString) == true && char_check(x, y + 1, ObjectiveString) == false)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 32, 32, 16, 16, image48, DX.TRUE, DX.FALSE);
                        return;
                    }

                    if (char_check(x - 1, y, ObjectiveString) == true && char_check(x + 1, y, ObjectiveString) == false &&
                        char_check(x, y - 1, ObjectiveString) == true && char_check(x, y + 1, ObjectiveString) == true)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 32, 16, 16, 16, image48, DX.TRUE, DX.FALSE);
                        return;
                    }

                    // ++

                    if (char_check(x - 1, y, ObjectiveString) == true && char_check(x + 1, y, ObjectiveString) == true &&
                        char_check(x, y - 1, ObjectiveString) == false && char_check(x, y + 1, ObjectiveString) == false)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 16, 0, 16, 8, image48, DX.TRUE, DX.FALSE);
                        DX.DrawRectGraph(x * 16, y * 16 + 8, 16, 40, 16, 8, image48, DX.TRUE, DX.FALSE);
                        return;
                    }


                    if (char_check(x - 1, y, ObjectiveString) == true && char_check(x + 1, y, ObjectiveString) == true &&
                        char_check(x, y - 1, ObjectiveString) == false && char_check(x, y + 1, ObjectiveString) == true)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 16, 0, 16, 16, image48, DX.TRUE, DX.FALSE);
                        //DX.DrawRectGraph(x * 16+8, y * 16, 32, 32, 8, 16, image48, DX.TRUE, DX.FALSE);
                        return;
                    }


                    if (char_check(x - 1, y, ObjectiveString) == true && char_check(x + 1, y, ObjectiveString) == true &&
                        char_check(x, y - 1, ObjectiveString) == true && char_check(x, y + 1, ObjectiveString) == false)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 16, 32, 16, 16, image48, DX.TRUE, DX.FALSE);
                        return;
                    }

                    if (char_check(x - 1, y, ObjectiveString) == true && char_check(x + 1, y, ObjectiveString) == true &&
                        char_check(x, y - 1, ObjectiveString) == true && char_check(x, y + 1, ObjectiveString) == true)
                    {
                        DX.DrawRectGraph(x * 16, y * 16, 16, 16, 16, 16, image48, DX.TRUE, DX.FALSE);
                        return;
                    }
                }


            }



        }// --------------------------------------- Main














    }// ------------------------------ class Program



    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ class Sprote
    class SpriteCompornent
    {
        public bool used;
        public float X, Y;
        //public float Z;
        public int image;
        public int U, V;
        public int Width, Height;
        public int Attribution;

        public delegate void UpdateDelegate(int sp);
        public UpdateDelegate Update;


        public static readonly int ANIM_MAX = 8;
        public List<long>[] AnimData;
        public bool[] AnimFrag;
        public bool[] AnimRelative;
        public int[] AnimStep;
        public int[] AnimCount;
        public bool[] AnimDelete;


        public SpriteCompornent()
        {
            used = false;
            X = 0;
            Y = 0;
            //Z = 0;
            image = -1;
            U = 0;
            V = 0;
            Width = 0;
            Height = 0;
            Attribution = 0;

            AnimData = new List<long>[ANIM_MAX+1]; //現状, アニメデータは最大8個まで追加予定
            AnimFrag = new bool[ANIM_MAX + 1];
            AnimRelative = new bool[ANIM_MAX+1];
            AnimStep = new int[ANIM_MAX + 1];
            AnimCount = new int[ANIM_MAX + 1];
            AnimDelete = new bool[ANIM_MAX + 1];

            Update = delegate (int sp){};
        }
        
    }




    static class Sprite
    {
        public static readonly int SPRITE_MAX = 512;

        public static SpriteCompornent[] sprite = new SpriteCompornent[SPRITE_MAX];
        public static Dictionary<int, short> sprite_Z = new Dictionary<int, short>();

        public static int NextNum = 0;
        
        public static int Attribution_reverse = 0b1;

        public static bool WasDisposed;

        public static int ThreadFPS = 60;


        //Spriteクラス初期化
        public static void Init()
        {
            for (int i=0; i<SPRITE_MAX; i++)
            {
                sprite[i] = new SpriteCompornent();
                sprite_Z.Add(i,0);
            }

            WasDisposed = false;

            Thread thread1 = new Thread(new ThreadStart(Animation_Thread));
            thread1.Start();


            //アニメ1ステップ要素数
            Anim1step_Load();
        }


        //Spriteクラス終了処理
        public static void End()
        {
            WasDisposed = true;
        }

        

        public static int Set(int imageHndl, int u, int v, int width, int height)
        {
            for (int i=0; i<SPRITE_MAX; i++)
            {
                int n = (NextNum+i) % SPRITE_MAX;

                if (sprite[n].used == false)
                {
                    sprite[n] = new SpriteCompornent();
                    sprite_Z[n] = 0;

                    sprite[n].used = true;
                    sprite[n].image = imageHndl;
                        // imageHndlが-1で空スプライトの作成
                    sprite[n].U = u;
                    sprite[n].V = v;
                    sprite[n].Width = width;
                    sprite[n].Height = height;
                    NextNum = n + 1;
                    return n;
                }
            }

            return -1;
        }

        public static void Attribution(int n, int attribution)
        {
            sprite[n].Attribution = attribution;
        }

        public static void Image(int n, int U, int V)
        {
            sprite[n].U = U;
            sprite[n].V = V;
        }

        public static void Image(int n, int U, int V, int Width, int Heigth)
        {
            sprite[n].U = U;
            sprite[n].V = V;
            sprite[n].Width = Width;
            sprite[n].Height = Heigth;
        }

        public static void Image(int n, int imageHndl, int U, int V, int Width, int Heigth)
        {
            sprite[n].image = imageHndl;
            sprite[n].U = U;
            sprite[n].V = V;
            sprite[n].Width = Width;
            sprite[n].Height = Heigth;
            sprite[n].AnimDelete[AnimType_UV] = true;
        }


        public static void Offset(int n, float x, float y)
        {
            sprite[n].X = x;
            sprite[n].Y = y;
            sprite[n].AnimDelete[AnimType_XY] = true;
        }

        public static void Offset(int n, float x, float y, short z)
        {
            sprite[n].X = x;
            sprite[n].Y = y;
            sprite_Z[n] = z;
            sprite[n].AnimDelete[AnimType_XY] = true;
        }

        //使用済処理
        public static void Clear(int n)
        {
            sprite[n].used = false;
            NextNum = n + 1;
        }
        public static void Clear()
        {
            for (int n=0; n<SPRITE_MAX; n++)
            {
                sprite[n].used = false;
                NextNum = n + 1;
            }
        }



        //^^^^^^^ アニメ情報
        //アニメ情報を登録する際はここに加えてAnimUpdate系統の追加も必要
        //そして、この下のAnimメソッドのswitch構文にも処理を追加
        //さらに, アニメでない通常処理を行ってパラメーターを変更した際, 自動でアニメのdeleteフラグをtrueにする必要がある
        public const int AnimType_XY = 1;
        public const int AnimType_UV = 2;

        private static void Anim1step_Load()
        {
            Anim1step[AnimType_XY] = 3;
            Anim1step[AnimType_UV] = 3;
        }
        //-------



        public static readonly int[] Anim1step = new int[SpriteCompornent.ANIM_MAX + 1]; //アニメの1ステップ要素数


        public static void Anim(int n, int Type, params int[] data)
        {
            if (Type<0)
            {
                Type *= -1;
                sprite[n].AnimRelative[Type] = true;
            }
            else
            {
                sprite[n].AnimRelative[Type] = false;
            }
            sprite[n].AnimFrag[Type] = true;
            sprite[n].AnimDelete[Type] = false;


            //初期情報登録

            sprite[n].AnimData[Type] = new List<long>(0);

            switch (Type)
            {
                case AnimType_XY:
                    sprite[n].AnimData[Type].Add(0);
                    sprite[n].AnimData[Type].Add(sprite[n].AnimRelative[Type] ? 0 : (long)sprite[n].X);
                    sprite[n].AnimData[Type].Add(sprite[n].AnimRelative[Type] ? 0 : (long)sprite[n].Y);
                    break;
                case AnimType_UV:
                    sprite[n].AnimData[Type].Add(0);
                    sprite[n].AnimData[Type].Add(sprite[n].AnimRelative[Type] ? 0 : (long)sprite[n].U);
                    sprite[n].AnimData[Type].Add(sprite[n].AnimRelative[Type] ? 0 : (long)sprite[n].V);
                    break;
            }



            //アニメデータ引数を登録
            for (int i=0; i<data.Length; i++)
            {
                sprite[n].AnimData[Type].Add(data[i]);
            }



            //回数が省略されていれば自動で1にする
            if (data.Length % Anim1step[Type] == 0) sprite[n].AnimData[Type].Add(1);


        }

        //アニメ動作中か調べる
        public static bool AnimCheck(int n)
        {
            for (int i=1; i<SpriteCompornent.ANIM_MAX; i++)
            {
                if (sprite[n].AnimFrag[i]) return true;
            }
            return false;
        }




        //スプライト一括更新処理
        public static void AllUpdate()
        {
            for (int i=0; i<SPRITE_MAX; i++)
            {
                sprite[i].Update(i);
            }
        }






        //スプライト一括描画処理
        public static void Drawing()
        {
            var draws = sprite_Z.OrderByDescending((x) => x.Value);
            
            //for (int i=0; i<SPRITE_MAX; i++)
            foreach (var v in draws)
            {
                int i = v.Key;

                if (sprite[i].used)
                {
                    if (sprite[i].image == -1) continue;

                    DX.DrawRectGraph((int)sprite[i].X, (int)sprite[i].Y, sprite[i].U, sprite[i].V,
                        sprite[i].Width, sprite[i].Height, sprite[i].image, 
                        DX.TRUE, sprite[i].Attribution & Attribution_reverse);
                }
            }
        }



        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ アニメーションスレッド (完全に指定FPS通りに処理はしていない)
        private static void Animation_Thread()
        {
            int frame = 0;
            int before = Environment.TickCount;
            

            while (!WasDisposed)
            {
                int now = Environment.TickCount;
                int progress = now - before;

                int ideal = (int)(frame * 1000.0F / ThreadFPS);

                //^^^^^^^^^^ 処理
                for (int i=0; i<SPRITE_MAX; i++)
                {
                    if (sprite[i].used)
                    {
                        for (int j=0 + 1; j<SpriteCompornent.ANIM_MAX+1; j++)
                        {
                            if (sprite[i].AnimFrag[j])
                            {
                                if (sprite[i].AnimDelete[j]) //アニメデータ強制削除
                                {
                                    sprite[i].AnimData[j] = new List<long>(0);
                                    sprite[i].AnimFrag[j] = false;
                                    continue;
                                }

                                AnimUpdateBase(i, j); //iが管理番号, jがアニメタイプ
                            }
                        }
                    }
                }
                //---------

                if (ideal > progress) Thread.Sleep(ideal - progress);

                frame++;
                if (progress >= 1000) //1sごとに更新
                {
                    before = now;
                    frame = 0;
                }
            }

            //^^^^^^^^^^ アニメ更新処理
            void AnimUpdateBase(int n, int type)
            {
                bool smooth = false;
                int count = sprite[n].AnimCount[type];
                int step = sprite[n].AnimStep[type];

                if ( count >= Math.Abs((int)sprite[n].AnimData[type][step * Anim1step[type]]) ) //次のコマへ移行
                {
                    step++;
                    count = 0;
                    int datalen = sprite[n].AnimData[type].Count;
                    int stepmax = (datalen-1)/ Anim1step[type] - 1;

                    if (step>stepmax) //1ループ終了

                    {
                        if (sprite[n].AnimData[type][datalen-1]>0) //ループ数が有限なら
                        {
                            sprite[n].AnimData[type][datalen - 1]--;
                            if (sprite[n].AnimData[type][datalen - 1] == 0) //ループ終了したらアニメデータを削除
                            {
                                sprite[n].AnimData[type] = new List<long>(0);
                                sprite[n].AnimFrag[type] = false;
                                return;
                            }
                        }
                        step = 0;

                    }
                }
                else
                {
                    count++;
                }

                int nextc = (int)sprite[n].AnimData[type][step * Anim1step[type]];
                if (nextc < 0) //スムーズアニメ
                {
                    nextc *= -1;
                    smooth = true;
                }

                //Console.WriteLine(step);
                switch (type)
                {
                    case AnimType_XY:
                        AnimUpdate_XY();break;
                    case AnimType_UV:
                        AnimUpdate_UV(); break;

                }



                sprite[n].AnimCount[type] = count;
                sprite[n].AnimStep[type] = step;
                //ここでAnimUpdateBaseの処理は終了



                //^^^^^^^^^ 各々アニメ型に応じて場合分け
                //メソッドを追加したら上のswitch構文でちゃんと飛んでいけるようにすること
                void AnimUpdate_XY()
                {
                    int rx = 0, ry = 0;
                    if (sprite[n].AnimRelative[type])
                    {
                        rx = (int)sprite[n].AnimData[type][1];
                        ry = (int)sprite[n].AnimData[type][2];

                    }


                    int x2 = (int)sprite[n].AnimData[type][step * 3 + 1] + rx;
                    int y2 = (int)sprite[n].AnimData[type][step * 3 + 2] + ry;

                    if (smooth) //スムーズアニメなら
                    {
                        int backstep = step - 1;
                        if (step == 0) backstep = 0;

                        int x1 = (int)sprite[n].AnimData[type][backstep * 3 + 1] + rx;
                        int y1 = (int)sprite[n].AnimData[type][backstep * 3 + 2] + ry;

                        sprite[n].X = x1 + (float)((x2 - x1) * ((double)count / nextc));
                        //Console.WriteLine(nextc);
                        sprite[n].Y = y1 + (float)((y2 - y1) * ((double)count / nextc));
                    }
                    else //ラフアニメなら
                    {
                        if (count == 0)
                        {
                            sprite[n].X = x2;
                            sprite[n].Y = y2;
                        }
                    }
                }

                void AnimUpdate_UV()
                {
                    int ru = 0, rv = 0;
                    if (sprite[n].AnimRelative[type])
                    {
                        ru = (int)sprite[n].AnimData[type][1];
                        rv = (int)sprite[n].AnimData[type][2];

                    }

                    //Console.WriteLine(step);
                    int u2 = (int)sprite[n].AnimData[type][step * 3 + 1] + ru;
                    int v2 = (int)sprite[n].AnimData[type][step * 3 + 2] + rv;

                    if (smooth) //スムーズアニメなら
                    {
                        int backstep = step - 1;
                        if (step == 0) backstep = 0;

                        int u1 = (int)sprite[n].AnimData[type][backstep * 3 + 1] + ru;
                        int v1 = (int)sprite[n].AnimData[type][backstep * 3 + 2] + rv;

                        sprite[n].U = u1 + (int)((u2 - u1) * ((double)count / nextc));
                        sprite[n].V = v1 + (int)((v2 - v1) * ((double)count / nextc));
                    }
                    else //ラフアニメなら
                    {
                        if (count == 0)
                        {
                            sprite[n].U = u2;
                            sprite[n].V = v2;
                        }
                    }
                }

                //-----------

            }

        }




        // ------------------------------------------ アニメーションスレッド

    }

    //-------------------------------------------- class Sprite

    //お役立ちクラス
    static class Useful
    {
        //2値の間にあるかどうか
        public static bool between(double a, double min, double max)
        {
            if (min<=a && a<=max)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //スプライトを放物運動アニメさせる
        public static void Sprite_ParabolaAnim(int sp, int x1, int y1, int x2, int y2,int interval, double curv)
        {
            int w = x2-x1, h = y2-y1;

            int[] AnimData = new int[16*3];

            for (int i=0; i<16; i++)
            {
                AnimData[i * 3 + 0] = -interval;
                AnimData[i * 3 + 1] = x1 + (int)(w * (double)(i + 1) / 16);
                AnimData[i * 3 + 2] = y1 + (int)(h * (double)(i + 1) / 16 + curv * ( (i+1-8) * (i+1-8) - 64) );
            }

            Sprite.Anim(sp, Sprite.AnimType_XY,AnimData);
        }

        //簡易破壊エフェクト
        public static void Clash_effect(int imageHndl, int x0, int y0)
        {
            int sp;
            int w = 16; int h = 16;

            for (int i=0; i<4; i++)
            {
                int u = (i % 2) * (w / 2);
                int v = (int)(i / 2) * (w / 2);

                sp = Sprite.Set(imageHndl, u, v, w/2, h/2);
                Sprite.Offset(sp, x0 + u, y0 + v, -512);

                int x1 = x0 + (-w / 4 + u) * 3;
                int y1 = y0 + (-h / 4 + v) * 3 + h;
                Sprite_ParabolaAnim(sp, x0, y0, x1, y1, 2, 0.4);

                Sprite.sprite[sp].Update += new SpriteCompornent.UpdateDelegate(Sprite_Effeectfade); //削除処理追加
            }

        }


        public static void Sprite_Effeectfade(int sp)
        {
            if (!Sprite.AnimCheck(sp)) Sprite.Clear(sp);
        }

        public static void Sprite_EffeectfadeXY(int sp)
        {
            if (!Sprite.sprite[sp].AnimFrag[Sprite.AnimType_XY]) Sprite.Clear(sp);
        }



        public static void DrawString_shadow(int x, int y, string s)
        {
            DX.DrawString(x, y+1, s, DX.GetColor(32,32,32));
            DX.DrawString(x, y, s, DX.GetColor(255, 255, 255));
        }

        public static void DrawString_bordered(int x, int y, string s)
        {
            DX.DrawString(x, y - 1, s, DX.GetColor(32, 32, 32));
            DX.DrawString(x, y + 1, s, DX.GetColor(32, 32, 32));
            DX.DrawString(x - 1, y, s, DX.GetColor(32, 32, 32));
            DX.DrawString(x + 1, y, s, DX.GetColor(32, 32, 32));
            DX.DrawString(x, y, s, DX.GetColor(255, 255, 255));
        }
        public static void DrawString_bordered(int x, int y, string s, uint c)
        {
            DX.DrawString(x, y - 1, s, DX.GetColor(32, 32, 32));
            DX.DrawString(x, y + 1, s, DX.GetColor(32, 32, 32));
            DX.DrawString(x - 1, y, s, DX.GetColor(32, 32, 32));
            DX.DrawString(x + 1, y, s, DX.GetColor(32, 32, 32));
            DX.DrawString(x, y, s, c);
        }

        /*
        public static void Wait(int frame)
        {
            int i=0;
            while (DX.ProcessMessage() != -1)
            {
                i++;
                Console.WriteLine(i);
                if (i > frame) return;
            }
        }
        */




    }


    //入力クラス
    static class Button
    {
        public static bool Left()
        {
            if (DX.CheckHitKey(DX.KEY_INPUT_A) == DX.TRUE || DX.CheckHitKey(DX.KEY_INPUT_LEFT) == DX.TRUE) return true;
            return false;
        }
        public static bool Right()
        {
            if (DX.CheckHitKey(DX.KEY_INPUT_D) == DX.TRUE || DX.CheckHitKey(DX.KEY_INPUT_RIGHT) == DX.TRUE) return true;
            return false;
        }
        public static bool Up()
        {
            if (DX.CheckHitKey(DX.KEY_INPUT_W) == DX.TRUE || DX.CheckHitKey(DX.KEY_INPUT_UP) == DX.TRUE) return true;
            return false;
        }
        public static bool Down()
        {
            if (DX.CheckHitKey(DX.KEY_INPUT_S) == DX.TRUE || DX.CheckHitKey(DX.KEY_INPUT_DOWN) == DX.TRUE) return true;
            return false;
        }

    }



}











