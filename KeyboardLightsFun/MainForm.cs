﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace YonatanMankovich.KeyboardLightsFun
{
    public partial class MainForm : Form
    {
        private List<Pattern> patterns = new List<Pattern>();
        private readonly PatternShowController patternShowController = new PatternShowController();

        public MainForm()
        {
            InitializeComponent();
            PatternsFileManager.LoadPatterns(ref patterns);
            UpdatePatternsComboBox();
            patternShowController.ProgressReported = PatternShowController_ProgressReported;
            patternShowController.ShowEnded = PatternShowController_ShowEnded;

            continiousCB.Checked = Properties.Settings.Default.IsContinuous;
            if (patternsCB.Items.Count > Properties.Settings.Default.SelectedShowIndex)
                patternsCB.SelectedIndex = Properties.Settings.Default.SelectedShowIndex;
            repeatsNUD.Value = Properties.Settings.Default.ShowRepeats;
            speedNUD.Value = Properties.Settings.Default.ShowSpeed;
        }

        private void UpdatePatternsComboBox()
        {
            patternsCB.Items.Clear();
            if (patterns.Count > 0)
            {
                foreach (Pattern pattern in patterns)
                    patternsCB.Items.Add(pattern);
                patternsCB.SelectedIndex = 0;
            }
        }

        private void ContiniousCB_CheckedChanged(object sender, EventArgs e)
        {
            repeatsNUD.Enabled = !continiousCB.Checked;
            patternShowController.Repeats = continiousCB.Checked ? 0 : (int)repeatsNUD.Value;
            if (patternShowController.IsShowing())
                patternShowPB.Style = continiousCB.Checked ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
        }

        private void patternEditBTN_Click(object sender, EventArgs e)
        {
            patternShowController.EndShow();
            PatternListForm patternListForm = new PatternListForm(patterns, patternsCB.SelectedIndex);
            DialogResult patternListDialogResult = patternListForm.ShowDialog(this);
            if (patternListDialogResult == DialogResult.OK) // AKA "Save"
            {
                patterns = patternListForm.Patterns;
                UpdatePatternsComboBox();
                if (patternListForm.SelectedIndex > 0)
                    patternsCB.SelectedIndex = patternListForm.SelectedIndex;
            }
            patternListForm.Dispose();
        }

        private void startStopBTN_Click(object sender, EventArgs e)
        {
            if (patternShowController.IsShowing())
                patternShowController.EndShow();
            else if (patternsCB.SelectedItem != null)
            {
                startStopBTN.Text = "Stop";
                patternsCB.Enabled = false;
                patternShowController.PatternShow = new PatternShow((Pattern)patternsCB.SelectedItem);
                speedNUD_ValueChanged(sender, EventArgs.Empty);
                repeatsNUD_ValueChanged(sender, EventArgs.Empty);
                ContiniousCB_CheckedChanged(sender, EventArgs.Empty);
                patternShowController.StartShow();
            }
        }

        private void PatternShowController_ShowEnded()
        {
            startStopBTN.Invoke(new MethodInvoker(delegate { startStopBTN.Text = "Start"; }));
            patternsCB.Invoke(new MethodInvoker(delegate { patternsCB.Enabled = true; }));
            toggeableKeyStatesVisualizer.Invoke(new MethodInvoker(delegate { toggeableKeyStatesVisualizer.MakeInactive(); }));
            patternShowPB.Invoke(new MethodInvoker(delegate
            {
                patternShowPB.Value = 0;
                patternShowPB.Style = ProgressBarStyle.Continuous;
            }));
        }

        private void PatternShowController_ProgressReported(int currentPatternProgressPercentage, int totalShowProgressPercentage, ToggleableKeyStates currentToggleableKeyStates)
        {
            patternShowPB.Invoke(new MethodInvoker(delegate
            {
                if (patternShowController.IsShowContinuous())
                    patternShowPB.Style = ProgressBarStyle.Marquee;
                else
                    patternShowPB.Value = totalShowProgressPercentage;
            }));
            toggeableKeyStatesVisualizer.Invoke(new MethodInvoker(
                delegate { toggeableKeyStatesVisualizer.Set(currentToggleableKeyStates); }));
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            patternShowController.EndShow();
            Properties.Settings.Default.IsContinuous = continiousCB.Checked;
            Properties.Settings.Default.SelectedShowIndex = patternsCB.SelectedIndex;
            Properties.Settings.Default.ShowRepeats = (int)repeatsNUD.Value;
            Properties.Settings.Default.ShowSpeed = (int)speedNUD.Value;
            Properties.Settings.Default.Save();
        }

        private void repeatsNUD_ValueChanged(object sender, EventArgs e)
        {
            patternShowController.Repeats = (int)repeatsNUD.Value;
        }

        private void speedNUD_ValueChanged(object sender, EventArgs e)
        {
            patternShowController.Speed = (int)speedNUD.Value;
            patternShowPB.MarqueeAnimationSpeed = 100 / (int)speedNUD.Value;
        }
    }
}