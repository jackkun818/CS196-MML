using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Media3D;
using System.Windows;

namespace crs.game
{
    public class Win32WindowHost : HwndHost
    {
        //重新定义Handle为依赖属性，可以用于绑定
        new public IntPtr Handle
        {
            get { return (IntPtr)GetValue(HandleProperty); }
            private set { SetValue(HandleProperty, value); }
        }
        public static readonly DependencyProperty HandleProperty =
            DependencyProperty.Register("Handle", typeof(IntPtr), typeof(Win32WindowHost), new PropertyMetadata(IntPtr.Zero));
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            Handle = CreateWindowEx(0, "static", "", WS_CHILD | WS_VISIBLE | LBS_NOTIFY | WS_CLIPSIBLINGS, 0, 0, (int)Width, (int)Height, hwndParent.Handle, IntPtr.Zero, IntPtr.Zero, 0);
            return new HandleRef(this, Handle);
        }
        [DllImport("user32.dll", SetLastError = true)]
        static extern System.IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
        }
        const int WS_CHILD = 0x40000000;
        const int WS_VISIBLE = 0x10000000;
        const int LBS_NOTIFY = 0x001;
        const int WS_CLIPSIBLINGS = 0x04000000;
        [DllImport("user32.dll")]
        internal static extern IntPtr CreateWindowEx(int exStyle, string className, string windowName, int style, int x, int y, int width, int height, IntPtr hwndParent, IntPtr hMenu, IntPtr hInstance, [MarshalAs(UnmanagedType.AsAny)] object pvParam);
        [DllImport("user32.dll")]
        static extern bool DestroyWindow(IntPtr hwnd);
    }
}
