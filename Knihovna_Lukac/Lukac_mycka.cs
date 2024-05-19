﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Knihovna_Lukac
{
    public class Lukac_mycka : IDisposable
    {
        private double _stateProgress;

        private double _stateBackGate;

        private double _stateFrontGate;

        private Thread _thread = new Thread(new ParameterizedThreadStart(ThreadProcedure));

        private GateState _frontState;

        private GateState _backState;

        public double timeToWash { get; private set; }

        public double timeToClose { get; private set; }

        private bool washingStart { get; set; }

        public delegate void ChangedFrontGateStateHandler(object sender, GateState gateState);

        public delegate void ChangedBackGateStateHandler(object sender, GateState gateState);

        public delegate void ChangedFrontGateClosingStateHandler(object sender, double closingInProcent);

        public delegate void ChangedBackGateClosingStateHandler(object sender, double closingInProcent);

        public delegate void ChangedMyckaStateHandler(object sender, double progressInProcent);

        public bool Running { get; set; }
        public int Stav { get; set; }

        public event ChangedFrontGateStateHandler OnFrontGateStateChange;

        public event ChangedFrontGateClosingStateHandler OnFrontGateClosingStateChange;

        public event ChangedBackGateStateHandler OnBackGateStateChange;

        public event ChangedBackGateClosingStateHandler OnBackGateClosingStateChange;

        public event ChangedMyckaStateHandler OnMyckaStateChanged;

        public GateState FrontGateState
        {
            get { return _frontState; }
            set
            {
                bool flag = value != _frontState;
                _frontState = value;
                if (flag)
                {
                    this.OnFrontGateStateChange?.Invoke(this, _frontState);
                }
            }
        }
        public GateState BackGateState
        {
            get { return _backState; }
            set
            {
                bool flag = value != _backState;
                _backState = value;
                if (flag)
                {
                    this.OnBackGateStateChange?.Invoke(this, _backState);
                }
            }
        }

        public double FrontGateClosingState
        {
            get { return _stateFrontGate; }
            set
            {
                bool flag = value != _stateFrontGate;
                _stateFrontGate = value;
                if (flag)
                {
                    this.OnFrontGateClosingStateChange?.Invoke(this, 100.0 * _stateFrontGate / timeToClose);
                }
            }
        }
        public double BackGateClosingState
        {
            get { return _stateBackGate; }
            set
            {
                bool flag = value != _stateBackGate;
                _stateBackGate = value;
                if (flag)
                {
                    this.OnBackGateClosingStateChange?.Invoke(this, 100.0 * _stateBackGate / timeToClose);
                }
            }
        }

        public int WashingSpeed { get; private set; }
        public int GateSpeed { get; private set; }
        public int WashingCycleMs { get; private set; }

        public double MyckaState
        {
            get
            {
                return _stateProgress;
            }
            private set
            {
                bool flag = value != _stateProgress;
                _stateProgress = value;
                if (flag)
                {
                    this.OnMyckaStateChanged?.Invoke(this, 100.0 * _stateProgress / timeToWash);
                }
            }
        }

        private static void ThreadProcedure(object obj)
        {
            Lukac_mycka mycka = (Lukac_mycka)obj;
            Stopwatch stopwatch = Stopwatch.StartNew();
            mycka.Stav = 0;
            while (mycka.Running)
            {
                Stopwatch stopwatch2 = Stopwatch.StartNew();
                stopwatch.Stop();
                double totalSeconds = stopwatch.Elapsed.TotalSeconds;
                if (totalSeconds > 0.0)
                {
                    double num1 = (double)((mycka.FrontGateState == GateState.Open) ? mycka.GateSpeed : 0) * totalSeconds;
                    double num12 = (double)((mycka.FrontGateState == GateState.Closed) ? mycka.GateSpeed : 0) * totalSeconds;
                    double num2 = (double)((mycka.BackGateState == GateState.Open) ? mycka.GateSpeed : 0) * totalSeconds;
                    double num22 = (double)((mycka.BackGateState == GateState.Closed) ? mycka.GateSpeed : 0) * totalSeconds;
                    double num3 = (double)((mycka.washingStart) ? -mycka.WashingSpeed : 0) * totalSeconds;
                    switch (mycka.Stav)
                    {
                        case 0:
                            mycka.FrontGateState = GateState.Open;
                            mycka.FrontGateClosingState += num1;
                            if (mycka.FrontGateClosingState >= mycka.timeToClose)
                            {
                                mycka.Stav = 1;
                                mycka.FrontGateState = GateState.Closed;
                            }
                            break;
                        case 1:
                            mycka.FrontGateClosingState -= num12;
                            if (mycka.FrontGateClosingState <= 0)
                            {
                                mycka.Stav = 2;
                                mycka.FrontGateState = GateState.Closed;
                                mycka.washingStart = true;
                            }
                            break;
                        case 2:
                            mycka.MyckaState += num3;
                            if (mycka.MyckaState <= 0)
                            {
                                mycka.Stav = 3;
                                mycka.BackGateState = GateState.Open;
                                mycka.MyckaState = mycka.timeToWash;
                            }
                            break;
                        case 3:
                            mycka.BackGateClosingState += num2;
                            if (mycka.BackGateClosingState >= mycka.timeToClose)
                            {
                                mycka.Stav = 4;
                                mycka.BackGateState = GateState.Closed;
                            }
                            break;
                        case 4:
                            mycka.BackGateClosingState -= num22;
                            if (mycka.BackGateClosingState <= 0)
                            {
                                mycka.Stav = 0;
                                mycka.BackGateState = GateState.Closed;
                                mycka.FrontGateState = GateState.Open;
                            }
                            break;
                    }
                }

                stopwatch.Restart();
                stopwatch2.Stop();
                try
                {
                    Thread.Sleep(50);
                }
                catch (ThreadInterruptedException)
                {
                    mycka.Running = false;
                }
            }
        }

        public Lukac_mycka(int program)
        {
            if (program > 4 || program < 1)
            {
                throw new InvalidOperationException("Wrong washing program!");
            }
            switch (program)
            {
                case 1:
                    timeToWash = 1000;
                    break;
                case 2:
                    timeToWash = 2000;
                    break;
                case 3:
                    timeToWash = 3000;
                    break;
                case 4:
                    timeToWash = 4000;
                    break;
            }
            Random random = new Random(Environment.TickCount);
            WashingSpeed = (int)(250.0 + random.NextDouble() * 10.0);
            GateSpeed = (int)(100.0 + random.NextDouble() * 5.0);
            FrontGateClosingState = 0;
            BackGateClosingState = 0;
            MyckaState = timeToWash;
            washingStart = false;
            timeToClose = 500;
            BackGateState = GateState.Closed;
            FrontGateState = GateState.Open;
            Running = true;
            _thread.Start(this);
        }


        public void Dispose()
        {
            try
            {
                _thread.Interrupt();
                _thread.Join();
            }
            catch (Exception)
            {
            }
        }
    }
}
#if false // Protokol o dekompilaci
Počet položek v mezipaměti: 13
------------------
Přeložit: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Našlo se jedno sestavení: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Načíst z C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\mscorlib.dll
------------------
Přeložit: System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Našlo se jedno sestavení: System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
Načíst z C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.dll
#endif
