﻿using System;
using System.Windows.Forms;

namespace YonatanMankovich.KeyboardLightsFun
{
    public partial class PatternEditorForm : Form
    {
        public Pattern Pattern { get; }
        public bool HasNewChanges { get; set; } = false;

        private readonly PatternShowController patternShowController = new PatternShowController();

        public PatternEditorForm(Pattern pattern)
        {
            Pattern = pattern.Clone();
            InitializeComponent();
            nameTB.Text = Pattern.Name;
            foreach (ToggleableKeyStates keyStates in Pattern.StatesList)
            {
                DataGridViewRow patternGVRow = (DataGridViewRow)patternGV.Rows[0].Clone();
                patternGVRow.Cells[0].Value = keyStates.NumLockState;
                patternGVRow.Cells[1].Value = keyStates.CapsLockState;
                patternGVRow.Cells[2].Value = keyStates.ScrollLockState;
                patternGV.Rows.Add(patternGVRow);
            }
            patternShowController.ProgressReported = PatternShowController_ProgressReported;
            patternShowController.ShowEnded = PatternShowController_ShowEnded;
            patternShowController.Repeats = 0;
            previewSpeedNUD.Value = Properties.Settings.Default.ShowSpeed;
        }

        private void saveBTN_Click(object sender, EventArgs e)
        {
            UpdatePattern();
            if (nameTB.Text.Length == 0)
                MessageBox.Show("The pattern must have a name.");
            else if (Pattern.StatesList.Count == 0)
                MessageBox.Show("The pattern must have at least one key state.");
            else
                DialogResult = DialogResult.OK;
        }

        private void UpdatePattern()
        {
            Pattern.StatesList.Clear();
            foreach (DataGridViewRow row in patternGV.Rows)
                if (row.Cells[2].Value != null)
                    Pattern.StatesList.Add(new ToggleableKeyStates(
                        (bool)row.Cells[0].Value, (bool)row.Cells[1].Value, (bool)row.Cells[2].Value));
        }

        private void nameTB_TextChanged(object sender, EventArgs e)
        {
            if (nameTB.Text.Length > 0)
                Pattern.Name = nameTB.Text;
        }

        private void patternGV_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells[0].Value = false;
            e.Row.Cells[1].Value = false;
            e.Row.Cells[2].Value = false;
        }

        private void previewBTN_Click(object sender, EventArgs e)
        {
            if (patternShowController.IsShowing())
                patternShowController.EndShow();
            else
            {
                UpdatePattern();
                previewBTN.Text = "Stop";
                patternGV.ReadOnly = true;
                patternShowController.PatternShow = new PatternShow(Pattern);
                patternShowController.Speed = (int)previewSpeedNUD.Value;
                patternShowController.StartShow();
            }
        }

        private void PatternShowController_ShowEnded()
        {
            previewBTN.Invoke(new MethodInvoker(delegate { previewBTN.Text = "Start"; }));
            patternGV.Invoke(new MethodInvoker(delegate
            {
                patternGV.ReadOnly = false;
                patternGV.ClearSelection();
            }));
            toggeableKeyStatesVisualizer.Invoke(new MethodInvoker(delegate { toggeableKeyStatesVisualizer.MakeInactive(); }));
        }

        private void PatternShowController_ProgressReported(int currentPatternProgressPercentage, int totalShowProgressPercentage, ToggleableKeyStates currentToggleableKeyStates)
        {
            patternGV.ClearSelection();
            if (currentPatternProgressPercentage > 0)
            {
                patternGV.Invoke(new MethodInvoker(delegate
                {
                    patternGV.Rows[(int)Math.Round(Pattern.StatesList.Count * (double)currentPatternProgressPercentage / 100) - 1].Selected = true;
                    patternGV.FirstDisplayedScrollingRowIndex = patternGV.SelectedRows[0].Index;
                }));
                toggeableKeyStatesVisualizer.Invoke(new MethodInvoker(
                    delegate { toggeableKeyStatesVisualizer.Set(currentToggleableKeyStates); }));
            }
        }

        private void PatternEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            patternShowController.EndShow();
            if (!HasNewChanges)
                DialogResult = DialogResult.Cancel;
            if (DialogResult != DialogResult.OK && HasNewChanges)
            {
                DialogResult closeDialogResult = MessageBox.Show("Do you want to save the changes?", "Warning",
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                switch (closeDialogResult)
                {
                    case DialogResult.Cancel: e.Cancel = true; break;
                    case DialogResult.Yes: saveBTN_Click(sender,EventArgs.Empty); break;
                    case DialogResult.No: DialogResult = DialogResult.Cancel; break;
                }
            }
        }

        private void cancelBTN_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void previewSpeedNUD_ValueChanged(object sender, EventArgs e)
        {
            patternShowController.Speed = (int)previewSpeedNUD.Value;
        }

        private void patternGV_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            HasNewChanges = true;
        }

        private void nameTB_KeyUp(object sender, KeyEventArgs e)
        {
            HasNewChanges = true;
        }
    }
}