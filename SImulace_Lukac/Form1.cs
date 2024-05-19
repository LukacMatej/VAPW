using System;
using System.Drawing;
using System.Windows.Forms;
using Knihovna_Lukac;

namespace SImulace_Lukac
{
    public partial class Form1 : Form
    {
        Lukac_mycka wash;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            wash?.Dispose();
        }
        private void Form1_Load_1(object sender, EventArgs e)
        {
            panel2.BackColor = Color.Green;
            panel4.BackColor = Color.Green;
            panel8.BackColor = Color.Blue;
            
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel2.BorderStyle = BorderStyle.FixedSingle;
            panel3.BorderStyle = BorderStyle.FixedSingle;
            panel4.BorderStyle = BorderStyle.FixedSingle;
            panel5.BorderStyle = BorderStyle.FixedSingle;
            panel6.BorderStyle = BorderStyle.FixedSingle;
            panel7.BorderStyle = BorderStyle.FixedSingle;
            panel8.BorderStyle = BorderStyle.FixedSingle;
        }
        private void Mycka_OnBackGateStateChange(object sender, GateState gateState)
        {
            panel7.BackColor = gateState == GateState.Open ? Color.Lime : Color.Red;
        }

        private void Mycka_OnFrontGateStateChange(object sender, GateState gateState)
        {
            panel6.BackColor = gateState == GateState.Open ? Color.Lime : Color.Red;
        }

        private void Mycka_OnFrontGateClosingStateChange(object sender, double closingInProcent)
        {
            this.Invoke(new Action(() => {
                panel2.Height = (int)(panel1.Height * (100 - closingInProcent) / 100);
            }));
        }

        private void Mycka_OnBackGateClosingStateChange(object sender, double closingInProcent)
        {
            this.Invoke(new Action(() => {
                panel4.Height = (int)(panel1.Height * (100 - closingInProcent) / 100);
            }));
        }

        private void Mycka_OnMyckaStateChanged(object sender, double progressInProcent)
        {
            this.Invoke(new Action(() => {
                panel8.Width = (int)(panel5.Width * (100 - progressInProcent) / 100);
            }));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            panel6.BackColor = wash.FrontGateState == GateState.Open ? Color.Lime : Color.Red;
            panel7.BackColor = wash.BackGateState == GateState.Open ? Color.Lime : Color.Red;
            panel2.Height = panel1.Height * (int)(100-(wash.FrontGateClosingState*100/wash.timeToClose)) / 100;
            panel4.Height = panel1.Height * (int)(100-(wash.BackGateClosingState*100/wash.timeToClose)) / 100;
            panel8.Width = panel5.Width * (int)(100-(wash.MyckaState*100/wash.timeToWash))/100;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var form = new SettingForm();
            var result = form.ShowDialog();
            if (result == DialogResult.OK)
            {
                wash = new Lukac_mycka(2);
                if (form.IsEvent)
                {
                    timer1.Stop();
                    panel6.BackColor = Color.Lime;
                    panel7.BackColor = Color.Red;
                    wash.OnMyckaStateChanged += Mycka_OnMyckaStateChanged;
                    wash.OnBackGateClosingStateChange += Mycka_OnBackGateClosingStateChange;
                    wash.OnFrontGateClosingStateChange += Mycka_OnFrontGateClosingStateChange;
                    wash.OnFrontGateStateChange += Mycka_OnFrontGateStateChange;
                    wash.OnBackGateStateChange += Mycka_OnBackGateStateChange;
                }
                else
                {
                    wash.OnMyckaStateChanged -= Mycka_OnMyckaStateChanged;
                    wash.OnBackGateClosingStateChange -= Mycka_OnBackGateClosingStateChange;
                    wash.OnFrontGateClosingStateChange -= Mycka_OnFrontGateClosingStateChange;
                    wash.OnFrontGateStateChange -= Mycka_OnFrontGateStateChange;
                    wash.OnBackGateStateChange -= Mycka_OnBackGateStateChange;
                    timer1.Start();
                }
            }
        }
    }
}
