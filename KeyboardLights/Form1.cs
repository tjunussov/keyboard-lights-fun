﻿using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;

namespace KeyboardLights
{
    public partial class MainForm : Form
    {
        List<int>[] patterns = new List<int>[9];
        private void MainForm_Load(object sender, EventArgs e)
        {
            patternsCollection.SelectedIndex = 0;
            patterns[0] = new List<int>() { 100, 010, 001, 010 };
            patterns[1] = new List<int>() { 100, 010, 001 };
            patterns[2] = new List<int>() { 001, 010, 100 };
            patterns[3] = new List<int>() { 101, 010 };
            patterns[4] = new List<int>() { 110, 011 };
            patterns[5] = new List<int>() { 100, 010, 001, 000 };
            patterns[6] = new List<int>() { 001, 010, 100, 000 };
            patterns[7] = new List<int>() { 100, 110, 111, 011, 001, 000 };
            patterns[8] = new List<int>() { 001, 011, 111, 110, 100, 000 };
        }

        public MainForm()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        public static extern short GetKeyState(int keyCode);
        bool[] keysStartState = new bool[3];
        int lastState = 000;
        BackgroundWorker bgWorker;
        int selectedPattern;
        private void startStopBTN_Click(object sender, EventArgs e)
        {
            if (startStopBTN.Text.Equals("Start"))
            {
                startStopBTN.Text = "Working";
                patternsCollection.Enabled = false;
                configGroup.Enabled = false;
                startStopBTN.Enabled = false;
                bgWorker = new BackgroundWorker();
                bgWorker.WorkerSupportsCancellation = true;
                bgWorker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
                bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
                bgWorker.RunWorkerAsync();
            }
        }

        void Stop()
        {
            SetKeys((keysStartState[0] ? 1 : 0) * 100 + (keysStartState[1] ? 1 : 0) * 10 + (keysStartState[2] ? 1 : 0), false);
            startStopBTN.Text = "Start";
            patternsCollection.Enabled = true;
            configGroup.Enabled = true;
            startStopBTN.Enabled = true;
        }

        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            bool[] isOn = { (((ushort)GetKeyState(0x90)) & 0xffff) != 0, (((ushort)GetKeyState(0x14)) & 0xffff) != 0, (((ushort)GetKeyState(0x91)) & 0xffff) != 0 };
            keysStartState = isOn;
            lastState = (keysStartState[0] ? 1 : 0) * 100 + (keysStartState[1] ? 1 : 0) * 10 + (keysStartState[2] ? 1 : 0);

            SetKeys(000,false);
            switch (selectedPattern)
            {
                case 0:
                    {
                        Random r = new Random();
                        for (int i = 0; i < (int)repeatsUD.Value || continiousCB.Checked; i++)
                            SetKeys(r.Next(2) * 100 + r.Next(2) * 10 + r.Next(2));
                    }
                    break;
                case 1:
                    for (int i = 0; i < (int)repeatsUD.Value || continiousCB.Checked; i++)
                        for (int k = 0; k < 8; k++)
                            for (int j = 0; j < patterns[k].Count; j++)
                                SetKeys(patterns[k][j]);
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    for (int i = 0; i < (int)repeatsUD.Value || continiousCB.Checked; i++)
                        for (int j = 0; j < patterns[selectedPattern-1].Count; j++)
                            SetKeys(patterns[selectedPattern-1][j]);
                    break;
            }
            SetKeys(000,false);
        }

        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Stop();
        }

        public void SetKeys(int keysState, bool sleep = true)
        {
            Key[] keys = new Key[3];
            keys[0] = new Key(0x90);//Num
            keys[1] = new Key(0x14);//Caps
            keys[2] = new Key(0x91);//Scroll
            bool[] isOn = { (lastState / 100 == 1), (lastState % 100 / 10 == 1), (lastState % 10 == 1) };
            bool[] wantedKeyStates = { (keysState / 100 == 1), (keysState % 100 / 10 == 1), (keysState % 10 == 1) };
            for (int i = 0; i < 3; i++)
                if (isOn[i] != wantedKeyStates[i])
                    keys[i].Toggle();
            isOn = wantedKeyStates;
            if (sleep)
                System.Threading.Thread.Sleep((11 - (int)speedUD.Value) * 100);
            lastState = keysState;
        }

        private void continiousCB_CheckedChanged(object sender, EventArgs e)
        {
            repeatsUD.Enabled = !continiousCB.Checked;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            SetKeys((keysStartState[0] ? 1 : 0) * 100 + (keysStartState[1] ? 1 : 0) * 10 + (keysStartState[2] ? 1 : 0), false);
        }

        private void patternsCollection_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedPattern = patternsCollection.SelectedIndex;
        }
    }

    class Key
    {
        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        public byte keyCode;

        public Key(byte code)
        {
            keyCode = code;
        }

        public void Toggle()
        {
            keybd_event(keyCode, 0x45, 0x1, (UIntPtr)0);
            keybd_event(keyCode, 0x45, 0x1 | 0x2, (UIntPtr)0);
        }
    }
}