using Iot.Device.CharacterLcd;
using Iot.Device.Pcx857x;
using Iot.Device.Vl53L1X;
using System.Device.Gpio;
using System.Device.I2c;
using static System.Net.Mime.MediaTypeNames;

namespace LCDController
{
    public enum LcdHardwareMode
    {
        Real,
        Emulated
    }

    public class LCD20x4Controller : IDisposable
    {
        private I2cDevice? _i2cLcd;
        private Pcf8574? _driver;
        private Lcd2004? _lcd;
        private GpioController? _gpioController;

        private readonly LcdHardwareMode _hardwareMode;

        public LCD20x4Controller(LcdHardwareMode mode)
        {
            _hardwareMode = mode;
        }
        public void Initialize()
        {
            if (_hardwareMode == LcdHardwareMode.Real)
            {
                _i2cLcd = I2cDevice.Create(new I2cConnectionSettings(1, 0x27));
                _driver = new Pcf8574(_i2cLcd);
                //_gpioController = new GpioController(PinNumberingScheme.Logical, _driver);
                _gpioController = new GpioController(_driver);

                _lcd = new Lcd2004(
                    registerSelectPin: 0,
                    enablePin: 2,
                    dataPins: new int[] { 4, 5, 6, 7 },
                    backlightPin: 3,
                    backlightBrightness: 0.1f,
                    readWritePin: 1,
                    controller: _gpioController);

                _lcd.Clear();
            }
            else
                Console.WriteLine("LCD20x4Controller : Initialize");
        }

        public void WriteDisplay(string[] lines)
        {
            if (_hardwareMode == LcdHardwareMode.Real)
                _lcd.Clear();
            else
            {
                //Console.WriteLine("WriteDisplay");
                Console.WriteLine("----X----X----X----X");
            }

            if (lines == null || lines.Length != 4)
                throw new ArgumentException(  $"Invalid number of lines. LCD 20x4 requires 4 text lines, but {lines?.Length ?? 0} were provided.", nameof(lines));

            for (int i = 0; i < lines.Length; i++)
            {
                if (_hardwareMode == LcdHardwareMode.Real)
                {
                    _lcd.SetCursorPosition(0, i);
                    _lcd.Write(lines[i] ?? string.Empty);
                }
                else
                    Console.WriteLine(lines[i] ?? string.Empty);
            }
        }

        public void WriteLine(int row, int col, string text)
        {
            if (_lcd == null)
                throw new InvalidOperationException("LCD not initialized. Call Initialize() first.");

            _lcd.SetCursorPosition(col, row);
            _lcd.Write(text);
        }

        public void Clear()
        {
            _lcd?.Clear();
        }

        public void Dispose()
        {
            _lcd?.Dispose();
            _gpioController?.Dispose();
            _driver?.Dispose();
            _i2cLcd?.Dispose();
        }
    }
}