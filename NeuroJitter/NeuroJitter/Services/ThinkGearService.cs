using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NeuroJitter.Models;
using Newtonsoft.Json;

namespace NeuroJitter.Services
{
    public class ThinkGearService
    {
        private TcpClient _client;
        private Stream _stream;
        private bool _isRunning;

        public event Action<MindWavePacket> DataReceived;
        public event Action<string> ConnectionStatusChanged;

        public void Connect()
        {
            Task.Run(() =>
            {
                try
                {
                    _client = new TcpClient("127.0.0.1", 13854);
                    _stream = _client.GetStream();

                    // Send Config to enable Raw Data format
                    var config = new { enableRawOutput = true, format = "Json" };
                    string jsonCmd = JsonConvert.SerializeObject(config);
                    byte[] cmdBytes = Encoding.ASCII.GetBytes(jsonCmd);
                    _stream.Write(cmdBytes, 0, cmdBytes.Length);

                    _isRunning = true;
                    ConnectionStatusChanged?.Invoke("Connected to TGC");
                    ReadLoop();
                }
                catch (Exception ex)
                {
                    ConnectionStatusChanged?.Invoke($"Error: {ex.Message}");
                }
            });
        }

        private void ReadLoop()
        {
            using (var reader = new StreamReader(_stream))
            {
                while (_isRunning && _client.Connected)
                {
                    try
                    {
                        string line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line)) continue;

                        // Parse JSON
                        var packet = JsonConvert.DeserializeObject<MindWavePacket>(line);
                        if (packet != null)
                        {
                            DataReceived?.Invoke(packet);
                        }
                    }
                    catch { /* Ignore parsing errors on partial packets */ }
                }
            }
        }

        public void Disconnect()
        {
            _isRunning = false;
            _client?.Close();
            ConnectionStatusChanged?.Invoke("Disconnected");
        }
    }
}