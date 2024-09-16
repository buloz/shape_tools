using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace ShapeTools.Tools;

/// <summary>
/// Provides methods for interacting with the system clipboard.
/// </summary>
public static class Clipboard
{
    
    /// <summary>
    /// Asynchronously sets the text content of the clipboard.
    /// </summary>
    /// <param name="text">The text to set on the clipboard.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public static async Task SetTextAsync(string text, CancellationToken cancellationToken)
    {
        await TryOpenClipboardAsync(cancellationToken);

        InnerSet(text);
    }

    /// <summary>
    /// Synchronously sets the text content of the clipboard.
    /// </summary>
    /// <param name="text">The text to set on the clipboard.</param>
    public static void SetText(string text)
    {
        TryOpenClipboard();

        InnerSet(text);
    }

    /// <summary>
    /// Internal method to set the text content of the clipboard.
    /// </summary>
    /// <param name="text">The text to set on the clipboard.</param>
    static void InnerSet(string text)
    {
        EmptyClipboard();
        IntPtr hGlobal = default;
        try
        {
            var bytes = (text.Length + 1) * 2;
            hGlobal = Marshal.AllocHGlobal(bytes);

            if (hGlobal == default)
            {
                ThrowWin32();
            }

            var target = GlobalLock(hGlobal);

            if (target == default)
            {
                ThrowWin32();
            }

            try
            {
                Marshal.Copy(text.ToCharArray(), 0, target, text.Length);
            }
            finally
            {
                GlobalUnlock(target);
            }

            if (SetClipboardData(cfUnicodeText, hGlobal) == default)
            {
                ThrowWin32();
            }

            hGlobal = default;
        }
        finally
        {
            if (hGlobal != default)
            {
                Marshal.FreeHGlobal(hGlobal);
            }

            CloseClipboard();
        }
    }

    /// <summary>
    /// Asynchronously attempts to open the clipboard.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    static async Task TryOpenClipboardAsync(CancellationToken cancellationToken)
    {
        var num = 10;
        while (true)
        {
            if (OpenClipboard(default))
            {
                break;
            }

            if (--num == 0)
            {
                ThrowWin32();
            }

            await Task.Delay(100, cancellationToken);
        }
    }

    /// <summary>
    /// Synchronously attempts to open the clipboard.
    /// </summary>
    static void TryOpenClipboard()
    {
        var num = 10;
        while (true)
        {
            if (OpenClipboard(default))
            {
                break;
            }

            if (--num == 0)
            {
                ThrowWin32();
            }

            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// Asynchronously retrieves the text content from the clipboard.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The text content of the clipboard, or null if not available.</returns>
    public static async Task<string?> GetTextAsync(CancellationToken cancellationToken)
    {
        if (!IsClipboardFormatAvailable(cfUnicodeText))
        {
            return null;
        }
        await TryOpenClipboardAsync(cancellationToken);

        return InnerGet();
    }

    /// <summary>
    /// Synchronously retrieves the text content from the clipboard.
    /// </summary>
    /// <returns>The text content of the clipboard, or null if not available.</returns>
    public static string? GetText()
    {
        if (!IsClipboardFormatAvailable(cfUnicodeText))
        {
            return null;
        }
        TryOpenClipboard();

        return InnerGet();
    }

    /// <summary>
    /// Internal method to retrieve the text content from the clipboard.
    /// </summary>
    /// <returns>The text content of the clipboard, or null if not available.</returns>
    static string? InnerGet()
    {
        IntPtr handle = default;

        IntPtr pointer = default;
        try
        {
            handle = GetClipboardData(cfUnicodeText);
            if (handle == default)
            {
                return null;
            }

            pointer = GlobalLock(handle);
            if (pointer == default)
            {
                return null;
            }

            var size = GlobalSize(handle);
            var buff = new byte[size];

            Marshal.Copy(pointer, buff, 0, size);

            return Encoding.Unicode.GetString(buff).TrimEnd('\0');
        }
        finally
        {
            if (pointer != default)
            {
                GlobalUnlock(handle);
            }

            CloseClipboard();
        }
    }

    const uint cfUnicodeText = 13;

    /// <summary>
    /// Throws a Win32Exception with the last Win32 error.
    /// </summary>
    static void ThrowWin32()
    {
        throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    [DllImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("User32.dll", SetLastError = true)]
    static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool CloseClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

    [DllImport("user32.dll")]
    static extern bool EmptyClipboard();

    [DllImport("Kernel32.dll", SetLastError = true)]
    static extern int GlobalSize(IntPtr hMem);
}