using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace HovyMonitor.Api.Services;

class SerialPortInputOutput
{
    private readonly SerialPort _serialPort;
    private StringBuilder _buffer = new();
    public SerialPortInputOutputConfig Config { get; }
    public bool IsOpen => _serialPort.IsOpen;
    
    public SerialPortInputOutput(SerialPortInputOutputConfig configuration)
    {
        _serialPort = new SerialPort(configuration.PortName, configuration.BaudRate);

        _serialPort.DataBits = configuration.DataBits;
        _serialPort.DataReceived += SerialPortOnDataReceived;
        _serialPort.ErrorReceived += SerialPortOnErrorReceived;
        _serialPort.PinChanged += SerialPortOnPinChanged;

        Config = configuration;
    }

    private void SerialPortOnPinChanged(object sender, SerialPinChangedEventArgs e)
    {
    }

    private void SerialPortOnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
    }

    private void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        _buffer.Append(_serialPort.ReadExisting());
    }

    public void OpenConnection()
    {
        if(!IsOpen)
            _serialPort.Open();
    }
  
    public void CloseConnection()
    {
        _serialPort.Close();
        _buffer = _buffer.Clear();
    }

    public void SendUbcCommand(string commandName, int maxCommandLength = 10)
    {
        // start chars for UBC
        var chars = new List<char>
        {
            (char)0x0a, (char)0x00
        };

        chars.AddRange(commandName.ToCharArray());

        if (chars.Count > maxCommandLength)
        {
            throw new InvalidOperationException($"Command must be less or equal {maxCommandLength} chars");
        }

        for (int i = 0; i < maxCommandLength - commandName.Length; i++)
        {
            chars.Add('\0');
        }
       
        Write(chars.Select(x => (byte)x).ToArray());
    }
    
    private void Write(byte[] packet)
    {
        if (_serialPort == null)
            throw new InvalidOperationException("Port is null. Is closed?");
        
        if (!_serialPort.IsOpen)
            throw new InvalidOperationException("Port closed.");
        
        _serialPort.Write(packet, 0, packet.Length);
    }

    public string PeekBuffer()
    {
        return _buffer.ToString();
    }

    public string PopBuffer()
    {
        var buffer = PeekBuffer();
        _buffer.Clear();
        return buffer;
    }
}

public record SerialPortInputOutputConfig (string PortName, int BaudRate, int DataBits);