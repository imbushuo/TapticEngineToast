using System;
using System.Runtime.InteropServices;

namespace TapticEngineToast.Wnf
{
    class WindowsNotification
    {
        public const ulong WNF_SHEL_TOAST_PUBLISHED = 0xD83063EA3BD0035;

        [DllImport("ntdll.dll")]
        public static extern int RtlSubscribeWnfStateChangeNotification(
            out IntPtr Subscription,
            ulong StateId,
            int Changestamp,
            CallBackDelegate Callback,
            [Out, Optional] int CallbackContext,
            [In, Optional] int TypeId,
            [In, Optional] int SerializationGroup,
            [In, Optional] int Undetermined
        );

        [DllImport("ntdll.dll")]
        public static extern int RtlUnsubscribeWnfStateChangeNotification(
            CallBackDelegate Callback
        );

        public delegate int CallBackDelegate(
            ulong p1,
            IntPtr p2,
            IntPtr p3,
            IntPtr p4,
            [MarshalAs(UnmanagedType.LPWStr)] string p5,
            int p6
        );
    }
}
