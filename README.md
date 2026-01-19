# 🧠 Neuro-Jitter: Advanced BCI Dashboard

**Neuro-Jitter** is a C# WPF application designed for the NeuroSky MindWave Mobile 2. It serves as a comprehensive biofeedback dashboard, analyzing signal jitter, spectral power bands, and cognitive states (eSense) in real-time.

---

## 🖥️ Features

### 📡 Connectivity & Signal
*   **TCP Bridge:** Connects to the **ThinkGear Connector** (TGC) via local TCP socket (`127.0.0.1:13854`).
*   **Signal Integrity Monitor:** Visual feedback (Green/Yellow/Red) based on headset sensor contact.
*   **Jitter Analysis:** Real-time variance calculation of the raw EEG signal to detect noise vs. clean brainwave data.
*   **Artifact Rejection:** Toggleable filter to ignore data packets when signal quality is poor.

### 🧠 Real-Time Metrics
*   **eSense Gauges:** Large, vertical bar indicators for **Attention** (Focus) and **Meditation** (Relaxation).
*   **Blink Detector:** Visual indicator that flashes red when a voluntary eye blink is detected.
*   **Spectral Analysis:** Full FFT power band breakdown (Delta, Theta, Alpha, Beta, Gamma) with individual progress bars.
*   **Peak Frequency:** Automatically identifies and displays the dominant brainwave state (e.g., "Alpha (Relax)").

### 📊 Visualization & Tools
*   **Raw Waveform Oscilloscope:** High-speed plotting of the raw electrical signal (512Hz) on a canvas.
*   **Data Logging:** One-click CSV recording (`BrainSession_timestamp.csv`) for post-session analysis.
*   **Biofeedback Alert:** Set a custom threshold (0-100) to trigger an audio beep when Attention exceeds the limit.
*   **Dark/Light Mode:** Toggleable UI theme for low-light environments.

---

## 🛠 Prerequisites

### Hardware
*   **NeuroSky MindWave Mobile 2** (or original MindWave).
*   Bluetooth-enabled Windows PC.

### Software
*   **[ThinkGear Connector (TGC)](http://developer.neurosky.com/):** The official NeuroSky driver that bridges Bluetooth to TCP.
    *   *> **Note:** Ensure TGC is running in the system tray before launching the app.*
*   **Windows OS** (WPF Support).
*   **.NET Framework 4.7.2+** or **.NET Core/5/6/7**.

### Dependencies (NuGet)
*   `Newtonsoft.Json`: Required for parsing the JSON data packets from the headset.

---

## 🚀 Getting Started

1.  **Install the Driver:** Download and run the **ThinkGear Connector**. Ensure the "Brain" icon appears in your system tray.
2.  **Pair Headset:** Pair your MindWave Mobile 2 to Windows via Bluetooth.
3.  **Build Project:**
    *   Clone this repository.
    *   Open in Visual Studio.
    *   Restore NuGet Packages (`Newtonsoft.Json`).
    *   Build & Run.
4.  **Connect:** Click the **Connect TCP** button in the top-right corner.
    *   *Green Status:* Connected and receiving good data.
    *   *Yellow/Red Status:* Check headset fit (sensor must touch forehead skin).

---

## 📂 Project Architecture

*   **`Services/ThinkGearService.cs`:** Handles the raw TCP socket communication and JSON deserialization.
*   **`ViewModels/MainViewModel.cs`:** The "Brain" of the application. Processes incoming packets, calculates jitter, updates graphs, and handles commands (MVVM pattern).
*   **`Views/MainWindow.xaml`:** The Dashboard UI, utilizing Data Binding for all visual elements.
*   **`Models/MindWaveData.cs`:** Strongly-typed classes mapping to the NeuroSky JSON protocol.

---

## 📝 License

This project is open-source. NeuroSky, ThinkGear, and MindWave are trademarks of NeuroSky, Inc.
