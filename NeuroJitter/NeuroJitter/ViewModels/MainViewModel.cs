using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using NeuroJitter.Models;
using NeuroJitter.Services;

namespace NeuroJitter.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ThinkGearService _service;
        private StreamWriter _logWriter;

        // --- 20+ Interactive Properties ---

        // 1. Connection
        public string ConnectionStatus { get; set; } = "Ready";

        // 2. Signal Quality
        public int SignalQuality { get; set; } = 200; // 0 is good
        public Brush SignalColor => SignalQuality == 0 ? Brushes.LimeGreen : (SignalQuality < 50 ? Brushes.Yellow : Brushes.Red);

        // 3-4. eSense
        public int Attention { get; set; }
        public int Meditation { get; set; }

        // 5. Raw Jitter Calculation
        public double JitterMetric { get; set; }

        // 6. Blink Detection
        public int BlinkStrength { get; set; }
        public bool IsBlinkDetected { get; set; }

        // 7-14. Power Bands
        public int Delta { get; set; }
        public int Theta { get; set; }
        public int LowAlpha { get; set; }
        public int HighAlpha { get; set; }
        public int LowBeta { get; set; }
        public int HighBeta { get; set; }
        public int LowGamma { get; set; }
        public int HighGamma { get; set; }

        // 15. Graphing
        public PointCollection RawWavePoints { get; set; }

        // 16. Threshold Alert
        public int AlertThreshold { get; set; } = 80;
        public bool IsAlertActive { get; set; }

        // 17. Logging
        public bool IsLogging { get; set; }

        // 18. Theme
        public bool DarkMode { get; set; } = true;
        public Brush BackgroundBrush => DarkMode ? new SolidColorBrush(Color.FromRgb(30, 30, 30)) : Brushes.White;
        public Brush TextBrush => DarkMode ? Brushes.White : Brushes.Black;

        // 19. Artifact Rejection
        public bool IgnoreArtifacts { get; set; } = true;

        // 20. Peak Detector
        public string PeakFreqBand { get; set; } = "None";

        public RelayCommand ConnectCommand { get; set; }
        public RelayCommand ToggleLogCommand { get; set; }
        public RelayCommand ToggleThemeCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            _service = new ThinkGearService();
            _service.DataReceived += OnDataReceived;
            _service.ConnectionStatusChanged += (s) => { ConnectionStatus = s; OnPropertyChanged(nameof(ConnectionStatus)); };

            RawWavePoints = new PointCollection(Enumerable.Repeat(new Point(0, 50), 200));

            ConnectCommand = new RelayCommand(o => _service.Connect());
            ToggleLogCommand = new RelayCommand(o => ToggleLogging());
            ToggleThemeCommand = new RelayCommand(o => {
                DarkMode = !DarkMode;
                OnPropertyChanged(nameof(BackgroundBrush));
                OnPropertyChanged(nameof(TextBrush));
            });
        }

        private void OnDataReceived(MindWavePacket data)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Feature: Artifact Rejection
                if (IgnoreArtifacts && data.PoorSignalLevel > 50 && data.PoorSignalLevel != 200) return;

                // Feature: Raw Wave & Jitter
                if (data.RawEeg != 0)
                {
                    UpdateRawGraph(data.RawEeg);
                    // Simple Jitter: variance of raw signal
                    JitterMetric = Math.Abs(data.RawEeg) / 10.0;
                    OnPropertyChanged(nameof(JitterMetric));
                }

                // Feature: Blink
                if (data.BlinkStrength > 0)
                {
                    BlinkStrength = data.BlinkStrength;
                    IsBlinkDetected = true;
                    OnPropertyChanged(nameof(BlinkStrength));
                    OnPropertyChanged(nameof(IsBlinkDetected));
                    // Reset blink visual after small delay
                    DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                    timer.Tick += (s, e) => { IsBlinkDetected = false; OnPropertyChanged(nameof(IsBlinkDetected)); timer.Stop(); };
                    timer.Start();
                }

                // Standard 1Hz Packet
                if (data.ESense != null)
                {
                    SignalQuality = data.PoorSignalLevel;
                    Attention = data.ESense.Attention;
                    Meditation = data.ESense.Meditation;

                    // Feature: Audio Alert
                    if (IsAlertActive && Attention > AlertThreshold)
                    {
                        System.Media.SystemSounds.Beep.Play();
                    }

                    // Power Bands
                    Delta = data.EegPower.delta;
                    Theta = data.EegPower.theta;
                    LowAlpha = data.EegPower.lowAlpha;
                    HighAlpha = data.EegPower.highAlpha;
                    LowBeta = data.EegPower.lowBeta;
                    HighBeta = data.EegPower.highBeta;
                    LowGamma = data.EegPower.lowGamma;
                    HighGamma = data.EegPower.highGamma;

                    // Feature: Peak Detection
                    CalculatePeak();

                    // Feature: Logging
                    if (IsLogging && _logWriter != null)
                    {
                        _logWriter.WriteLine($"{DateTime.Now},{Attention},{Meditation},{data.RawEeg}");
                    }

                    NotifyAll();
                }
            });
        }

        private void UpdateRawGraph(int rawVal)
        {
            var points = new PointCollection(RawWavePoints);
            points.RemoveAt(0);

            // Map Raw (-32000 to 32000) to Canvas (0 to 100)
            double scaledY = 50 - (rawVal / 20.0);
            if (scaledY < 0) scaledY = 0; if (scaledY > 100) scaledY = 100;

            points.Add(new Point(200, scaledY)); // Add to end

            // Shift X
            for (int i = 0; i < points.Count; i++) points[i] = new Point(i, points[i].Y);

            RawWavePoints = points;
            OnPropertyChanged(nameof(RawWavePoints));
        }

        private void ToggleLogging()
        {
            if (IsLogging)
            {
                _logWriter?.Close();
                IsLogging = false;
            }
            else
            {
                _logWriter = new StreamWriter($"BrainSession_{DateTime.Now.Ticks}.csv", true);
                _logWriter.WriteLine("Time,Attention,Meditation,Raw");
                IsLogging = true;
            }
            OnPropertyChanged(nameof(IsLogging));
        }

        private void CalculatePeak()
        {
            int max = new[] { Delta, Theta, LowAlpha, HighAlpha, LowBeta, HighBeta }.Max();
            if (max == Delta) PeakFreqBand = "Delta (Sleep)";
            else if (max == Theta) PeakFreqBand = "Theta (Dream)";
            else if (max == LowAlpha || max == HighAlpha) PeakFreqBand = "Alpha (Relax)";
            else PeakFreqBand = "Beta (Active)";
        }

        private void NotifyAll()
        {
            // Trigger UI updates for all bound properties
            OnPropertyChanged(nameof(SignalQuality));
            OnPropertyChanged(nameof(SignalColor));
            OnPropertyChanged(nameof(Attention));
            OnPropertyChanged(nameof(Meditation));
            OnPropertyChanged(nameof(Delta));
            OnPropertyChanged(nameof(Theta));
            OnPropertyChanged(nameof(LowAlpha));
            OnPropertyChanged(nameof(HighAlpha));
            OnPropertyChanged(nameof(LowBeta));
            OnPropertyChanged(nameof(HighBeta));
            OnPropertyChanged(nameof(LowGamma));
            OnPropertyChanged(nameof(HighGamma));
            OnPropertyChanged(nameof(PeakFreqBand));
        }

        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class RelayCommand : System.Windows.Input.ICommand
    {
        private Action<object> _action;
        public RelayCommand(Action<object> action) => _action = action;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _action(parameter);
        public event EventHandler CanExecuteChanged;
    }
}