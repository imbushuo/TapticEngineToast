using HidSharp;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static TapticEngineToast.Wnf.WindowsNotification;

namespace TapticEngineToast
{
    class Program
    {
        private static HidDevice _hapticDevice;
        private static IntPtr _subscription;
        private static CallBackDelegate _callbackDelegate;
        private static CancellationTokenSource _cancellationTokenSource;

        public static int OnToastPublished(
            ulong p1,
            IntPtr p2,
            IntPtr p3,
            IntPtr p4,
            [MarshalAs(UnmanagedType.LPWStr)] string p5,
            int p6
        )
        {
            try
            {
                if (_hapticDevice.TryOpen(out DeviceStream stream))
                {
                    // 14 bytes packed struct
                    // 0x53, 0x01: Magic (uint16_t)
                    // 0x15, 0x6c, 0x02, 0x00: Strength (uint32_t)
                    // Other fields remain unknown. I think they are X axis and Y axis data, and
                    // these values should be signed int32. No significant difference is obsereved
                    // when changing these fields. It's okay to write all zero for these fields.
                    //
                    // MacBook Pro will reject feature writes if you write it too fast
                    stream.Write(new byte[] { 0x53, 0x01, 0xff, 0x6c, 0x02, 0x00, 0x21, 0x2B, 0x06, 0x01, 0x00, 0x16, 0x41, 0x13 }, 0, 14);
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    stream.Write(new byte[] { 0x53, 0x01, 0xff, 0x6c, 0x02, 0x00, 0x21, 0x2B, 0x06, 0x01, 0x00, 0x16, 0x41, 0x13 }, 0, 14);
                }
            }
            catch (Exception)
            {
                // Currently just ignore
            }

            return 0;
        }

        static void Main(string[] args)
        {
            int ret = 0;

            // Force Touch Device: USB\VID_05AC&PID_027D&MI_02
            // But I don't know if I have a better way to deal with multi collections
            var hidDeviceList = DeviceList.Local.GetHidDevices(0x05ac, 0x027d).Where(i => i.DevicePath.Contains("mi_03"));
            if (!hidDeviceList.Any()) return;

            // Handle exit event
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            _cancellationTokenSource = new CancellationTokenSource();

            _hapticDevice = hidDeviceList.FirstOrDefault();
            _callbackDelegate = new CallBackDelegate(OnToastPublished);

            // Register WNF
            ret = RtlSubscribeWnfStateChangeNotification(
                out _subscription,
                WNF_SHEL_TOAST_PUBLISHED,
                0,
                _callbackDelegate,
                0, 0, 0, 0
            );

            // Run loop
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                Thread.Sleep(500);
            }

            // RtlUnsubscribeWnfStateChangeNotification
            ret = RtlUnsubscribeWnfStateChangeNotification(_callbackDelegate);
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            // Signal exit
            _cancellationTokenSource.Cancel();
        }
    }
}
