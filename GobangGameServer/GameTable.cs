﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Timers;

namespace GobangGameServer
{
    class GameTable
    {
        private const int None = -1;       //无棋子
        private const int Black = 0;       //黑色棋子
        private const int White = 1;       //白色棋子
        public Player[] gamePlayer;        //保存同一桌的玩家信息
        private int[,] grid = new int[15, 15];       //15*15的方格
        private System.Timers.Timer timer;       //用于定时产生棋子
        private int NextdotColor = 0;            //应该产生黑棋子还是白棋子
        private ListBox listbox;
        public int turn;                   //轮流玩,0为黑，1为白 edit
        public int Turn{
            get{return turn;}
            set { turn = value; }
        }
        Random rnd = new Random();
        Service service;
        public GameTable(ListBox listbox)
        {
            gamePlayer = new Player[2];
            gamePlayer[0] = new Player();
            gamePlayer[1] = new Player();
            timer = new System.Timers.Timer();
            //timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = false;
            this.listbox = listbox;
            this.turn = -1;  //edit
            service = new Service(listbox);
            ResetGrid();
        }
        /// <summary>重置棋盘</summary>
        public void ResetGrid()
        {
            for (int i = 0; i <= grid.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= grid.GetUpperBound(1); j++)
                {
                    grid[i, j] = None;
                }
            }
            gamePlayer[0].grade = 0;
            gamePlayer[1].grade = 0;
        }
        /// <summary>启动Timer</summary>
        public void StartTimer()
        {
            timer.Start();
        }
        /// <summary>停止Timer</summary>
        public void StopTimer()
        {
            timer.Stop();
        }
        /// <summary>设置时间间隔</summary>
        /// <param name="interval">时间间隔</param>
        public void SetTimerLevel(int interval)
        {
            timer.Interval = interval;
        }
        /// <summary>达到时间间隔时处理事件</summary>
        private void timer_Elapsed(object sender, EventArgs e)
        {
            int x, y;
            //随机产生一个格内没有棋子的单元格位置
            do
            {
                x = rnd.Next(15);  //产生一个小于15的非负整数
                y = rnd.Next(15);
            } while (grid[x, y] != None);
            //放置棋子:x坐标,y坐标,颜色
            SetDot(x, y, NextdotColor);
            //设定下次分发的旗子颜色
            NextdotColor = (NextdotColor + 1) % 2;
        }
        /// <summary>发送产生的棋子信息</summary>
        /// <param name="i">指定棋盘的第几行</param>
        /// <param name="j">指定棋盘的第几列</param>
        /// <param name="dotColor">棋子颜色</param>
        public void SetDot(int i, int j, int dotColor)//edit private to public
        {
            if (dotColor == turn)//edit
            {
                //向两个用户发送产生的棋子信息，并判断是否有相邻棋子
                //发送格式：SetDot,行,列,颜色
                grid[i, j] = dotColor;
                service.SendToBoth(this, string.Format("SetDot,{0},{1},{2}", i, j, dotColor));
                if (win(dotColor))//edit
                {
                    ShowWin(dotColor);
                }
                turn = (turn + 1)%2;
            }
        }
        //是否胜利。
        private bool win(int dotColor)//edit
        {
            int num = 15;
            int checkPoint = 0;
            int i = 0;
            int j = 0;
            //横着检查。
            for (int x = 0; x < num * num; x++)
            {
                int consecutive = 0;
                checkPoint = x;
                for (int y = 0; y < 5; y++)
                {
                    i = (checkPoint + y) % num;
                    j = (int)checkPoint / num;
                    if (checkPoint > (num * num - 1) || i > num - 1)
                        break;
                    if (grid[i, j] == dotColor)
                    {
                        consecutive++;
                    }
                    //checkPoint++;
                }
                if (consecutive == 5)
                    return true;
            }
            //竖着检查
            for (int x = 0; x < num * num; x++)
            {
                int consecutive = 0;
                checkPoint = x;
                for (int y = 0; y < 5; y++)
                {
                    i = (checkPoint + y) % num;
                    j = (int)checkPoint / num;
                    if (checkPoint > (num * num - 1) || i > num - 1)
                        break;
                    if (grid[j, i] == dotColor)
                    {
                        consecutive++;
                    }
                    //checkPoint++;
                }
                if (consecutive == 5)
                    return true;
            }
            //正斜
            for (int x = 0; x < num * num; x++)
            {
                int consecutive = 0;
                checkPoint = x;
                for (int y = 0; y < 5; y++)
                {
                    i = checkPoint % num + y;
                    j = ((int)checkPoint / num) + y;
                    if (i > num - 1 || j > num - 1)
                        break;
                    if (grid[i, j] == dotColor)
                    {
                        consecutive++;
                    }
                }
                if (consecutive == 5)
                    return true;
            }
            //反斜
            for (int x = 0; x < num * num; x++)
            {
                int consecutive = 0;
                checkPoint = x;
                for (int y = 0; y < 5; y++)
                {
                    i = checkPoint % num - y;
                    j = (int)checkPoint / num + y;
                    if (i > num - 1 || i < 0 || j > num - 1 || j < 0)
                        break;
                    if (grid[i, j] == dotColor)
                    {
                        consecutive++;
                    }
                }
                if (consecutive == 5)
                    return true;
            }
            return false;
        }

        /// <summary>出现相邻点的颜色为dotColor</summary>
        /// <param name="dotColor">相邻点的颜色</param>
        private void ShowWin(int dotColor)
        {
            timer.Enabled = false;
            gamePlayer[0].started = false;
            gamePlayer[1].started = false;
            this.ResetGrid();
            //发送格式：Win,相邻点的颜色,黑方成绩,白方成绩
            service.SendToBoth(this, string.Format("Win,{0},{1},{2}",
                dotColor, gamePlayer[0].grade, gamePlayer[1].grade));
        }
        /// <summary>消去棋子的信息</summary>
        /// <param name="i">指定棋盘的第几行</param>
        /// <param name="j">指定棋盘的第几列</param>
        /// <param name="color">指定棋子颜色</param>
        public void UnsetDot(int i, int j, int color)
        {
            //向两个用户发送消去棋子的信息
            //格式：UnsetDot,行,列,黑方成绩,白方成绩
            grid[i, j] = None;
            gamePlayer[color].grade++;
            string str = string.Format("UnsetDot,{0},{1},{2},{3}",
                i, j, gamePlayer[0].grade, gamePlayer[1].grade);
            service.SendToBoth(this, str);
        }
    }
}
