using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Threading;

namespace Shared.Windows
{
    using static Shared.Windows.NativeImports;

    public class HidDeviceStream : Stream
    {
        private string m_path;
        private FileShare m_sharingMode;
        private SafeFileHandle m_handle;

        private byte[] m_readBuffer;
        private byte[] m_writeBuffer;

        public override bool CanRead => InputLength > 0;
        public override bool CanWrite => OutputLength > 0;
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public int InputLength { get; private set; }
        public int OutputLength { get; private set; }

        public bool UseHidD { get; set; }

        private HidDeviceStream() { }

        public HidDeviceStream(string path, FileShare sharingMode = FileShare.ReadWrite)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            int result = Initialize(path, sharingMode);
            if (result != (int)Win32Error.Success)
            {
                throw new Win32Exception(result);
            }
        }

        public static bool TryCreate(string path, out HidDeviceStream stream, FileShare sharingMode = FileShare.ReadWrite)
        {
            stream = new HidDeviceStream();
            int result = stream.Initialize(path, sharingMode);
            if (result != (int)Win32Error.Success)
            {
                Debug.WriteLine($"Could not create HID device stream for {path}: {new Win32Exception(result).Message} ({result})");
                stream = null;
                return false;
            }

            return true;
        }

        private int Initialize(string path, FileShare sharingMode)
        {
            int result;
            m_path = path;
            m_sharingMode = sharingMode;
            if (!OpenDevice(path, out var m_handle, sharingMode))
            {
                return Marshal.GetLastWin32Error();
            }

            if (!HidD_GetPreparsedData(m_handle, out var hidData))
            {
                result = Marshal.GetLastWin32Error();
                Debug.WriteLine($"Could not get preparsed data for {path}: {new Win32Exception(result).Message} ({result})");
                return result;
            }

            if (hidData == null || hidData.IsInvalid)
            {
                Debug.WriteLine($"Preparsed data handle is invalid for {path}");
                return (int)Win32Error.InvalidHandle;
            }

            result = HidP_GetCaps(hidData, out var caps);
            if (result < 0) // HRESULT, not Win32 error
            {
                Debug.WriteLine($"Could not get HID capabilities for {path}: {new Win32Exception(result).Message} ({result})");
                return result;
            }

            InputLength = caps.InputReportByteLength;
            OutputLength = caps.OutputReportByteLength;

            m_readBuffer = new byte[InputLength];
            m_writeBuffer = new byte[OutputLength];

            return (int)Win32Error.Success;
        }

        private static bool OpenDevice(string path, out SafeFileHandle handle, FileShare sharingMode = FileShare.ReadWrite)
        {
            handle = CreateFile(path, FileAccess.ReadWrite, sharingMode, IntPtr.Zero, FileMode.Open, EFileAttributes.Overlapped, IntPtr.Zero);
            bool result = handle != null && !handle.IsInvalid;
            Debug.WriteLineIf(!result, $"Could not create file for path {path}: {new Win32Exception(Marshal.GetLastWin32Error()).Message} ({Marshal.GetLastWin32Error()})");
            return result;
        }

        public bool Open()
        {
            if (m_handle != null && !m_handle.IsInvalid)
                return true;

            return OpenDevice(m_path, out m_handle, m_sharingMode);
        }

        private void VerifyArguments(byte[] buffer, int offset, int count)
        {
            if (m_handle == null)
                throw new ObjectDisposedException(nameof(m_handle));

            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (offset < 0)
                throw new ArgumentOutOfRangeException($"Offset must not be negative! Got {offset}", nameof(offset));

            if (count < 0)
                throw new ArgumentOutOfRangeException($"Count must not be negative! Got {count}", nameof(count));

            if ((buffer.Length - offset) < count)
                throw new ArgumentException($"Buffer is too small for the given count! Must be at least {count}, got {buffer.Length}", $"{nameof(buffer)}, {nameof(count)}");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            VerifyArguments(buffer, offset, count);
            if (buffer.Length < 1 || count < 1)
                return 0;

            Debug.WriteLine($"Reading {count} bytes");

            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            var overlapped = new NativeOverlapped
            {
                EventHandle = waitHandle.SafeWaitHandle.DangerousGetHandle()
            };

            bool success;
            uint bytesRead;
            int result;
            if (UseHidD)
            {
                success = HidD_GetInputReport(m_handle, m_readBuffer, (uint)m_readBuffer.Length);
                bytesRead = (uint)InputLength;
            }
            else
            {
                success = ReadFile(m_handle, m_readBuffer, (uint)m_readBuffer.Length, out bytesRead, ref overlapped);
            }

            result = Marshal.GetLastWin32Error();
            if (result == (int)Win32Error.IoPending)
            {
                success = GetOverlappedResult(m_handle, in overlapped, out bytesRead, true);
                result = Marshal.GetLastWin32Error();
            }

            if (!success && result != (int)Win32Error.Success)
            {
                throw new Win32Exception(result);
            }

            Debug.WriteLineIf(!success, "Result was failure but GetLastWin32Error returned success");
            Debug.WriteLineIf(result != (int)Win32Error.Success, $"Result was success but GetLastWin32Error returned {result} (\"{new Win32Exception(result).Message}\")");

            Debug.WriteLine($"Read: {BitConverter.ToString(m_readBuffer)}");

            Array.Copy(m_readBuffer, 0, buffer, offset, Math.Min(m_readBuffer.Length, buffer.Length - offset));
            return (int)bytesRead;
        }

        // TODO
        // public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        // {
        // }

        // TODO
        // public override int EndRead(IAsyncResult asyncResult)
        // {
        // }

        public override void Write(byte[] buffer, int offset, int count)
        {
            VerifyArguments(buffer, offset, count);
            if (buffer.Length < 1 || count < 1)
                return;

            m_writeBuffer.Initialize();
            Array.Copy(buffer, offset, m_writeBuffer, 0, Math.Min(m_writeBuffer.Length, buffer.Length - offset));

            Debug.WriteLine($"Writing: {BitConverter.ToString(m_writeBuffer)}");

            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            var overlapped = new NativeOverlapped
            {
                EventHandle = waitHandle.SafeWaitHandle.DangerousGetHandle()
            };

            bool success;
            int result;
            uint bytesWritten;
            if (UseHidD)
            {
                success = HidD_SetOutputReport(m_handle, m_writeBuffer, (uint)m_writeBuffer.Length);
                bytesWritten = (uint)OutputLength;
            }
            else
            {
                success = WriteFile(m_handle, m_writeBuffer, (uint)m_writeBuffer.Length, out bytesWritten, ref overlapped);
            }

            result = Marshal.GetLastWin32Error();
            if (result == (int)Win32Error.IoPending)
            {
                success = GetOverlappedResult(m_handle, in overlapped, out bytesWritten, true);
                result = Marshal.GetLastWin32Error();
            }

            if (!success && result != (int)Win32Error.Success)
            {
                throw new Win32Exception(result);
            }

            Debug.WriteLineIf(!success, "Result was failure but GetLastWin32Error returned success");
            Debug.WriteLineIf(result != (int)Win32Error.Success, $"Result was success but GetLastWin32Error returned {result} (\"{new Win32Exception(result).Message}\")");

            Debug.WriteLine($"Wrote {bytesWritten} bytes");
        }

        // TODO
        // public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        // {
        // }

        // TODO
        // public override int EndWrite(IAsyncResult asyncResult)
        // {
        // }

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                m_handle?.Close();
            }

            m_handle = null;
        }
    }
}