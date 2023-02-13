/* * * * * * * * * * * * * * * * * * * * * * * * * * *
 * === Notes ===
 * 
 * - When using the Toshiba Stack,
 *   Use WriteFile with 22 byte reports
 *   
 * - When On Windows 8 & 10 with MS Stack,
 *   Use WriteFile with minimum report size
 *   
 * - When On Windows 7 or lower with MS Stack,
 *   Use SetOutputReport (does not work with TR/Pro)
 *   
 * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using static Shared.Windows.NativeImports;

namespace Shared.Windows
{
    public class WinBtStream : Stream
    {
        #region Members
        public static bool OverrideSharingMode = false;
        public static FileShare OverridenFileShare = FileShare.None;
        public static bool ForceToshibaMode = false;

        protected string _hidPath;
        protected SafeFileHandle _fileHandle;
        protected FileStream _fileStream;
        protected object _writerBlock;
        #endregion

        #region Properties
        /// <summary>
        /// Set to None to have exclusive access to the controller.
        /// Otherwise set to ReadWrite.
        /// </summary>
        public FileShare SharingMode { get; set; } = FileShare.ReadWrite;

        /// <summary>
        /// Set if the user is using the Toshiba Bluetooth Stack
        /// </summary>
        public static bool UseToshiba { get; set; }

        /// <summary>
        /// Set to use the WriteFile method (allows use with the Microsoft Bluetooth Stack)
        /// </summary>
        public bool UseWriteFile { get; set; }

        /// <summary>
        /// Set when using to use 22 byte reports when sending data (use with Toshiba Stack or Set_Output_Report)
        /// </summary>
        public bool UseFullReportSize { get; set; }
        #endregion

        static WinBtStream()
        {
            // When true, Windows Stack is enabled
            //var a = BluetoothEnableDiscovery(IntPtr.Zero, true);
        }

        public WinBtStream(string path)
        {
            UseToshiba = ForceToshibaMode;// || !BluetoothEnableDiscovery(IntPtr.Zero, true);

            // Default Windows 8/10 to ReadWrite (non exclusive)
            if (Environment.OSVersion.Version.Major > 6 
                || (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 2)) // temp
            {
                SharingMode = FileShare.ReadWrite;
                UseWriteFile = true;

                // A certian build of Windows 10 seems to have fixed the FileShare.None issue
                //if (Environment.OSVersion.Version.Major == 10 &&
                //    Environment.OSVersion.Version.Build >= 10586/* &&
                //    Environment.OSVersion.Version.Build < 14393*/)
                //{
                //    SharingMode = FileShare.None;
                //}
            }
            else
            {
                SharingMode = FileShare.None;
                UseFullReportSize = true;
            }

            // Determine if using the Toshiba Stack
            if (UseToshiba)
            {
                SharingMode = FileShare.None;
                UseFullReportSize = true;
                UseWriteFile = true;
            }
            
            _hidPath = path;
            _writerBlock = new object();
        }

        public WinBtStream(string path, DeviceInfo.BtStack btStack) : this(path)
        {
            if (btStack == DeviceInfo.BtStack.Toshiba)
            {
                UseFullReportSize = true;
                UseWriteFile = true;
            }
        }

        public WinBtStream(string path, DeviceInfo.BtStack btStack, FileShare sharingMode) : this(path, btStack)
        {
            SharingMode = sharingMode;
        }

        public bool OpenConnection()
        {
            if (string.IsNullOrWhiteSpace(_hidPath))
            {
                return false;
            }

            try
            {
                if (OverrideSharingMode)
                {
                    _fileHandle = CreateFile(_hidPath, FileAccess.ReadWrite, OverridenFileShare, IntPtr.Zero, FileMode.Open, EFileAttributes.Overlapped, IntPtr.Zero);
                }
                else
                {
                    // Open the file handle with the specified sharing mode and an overlapped file attribute flag for asynchronous operation
                    _fileHandle = CreateFile(_hidPath, FileAccess.ReadWrite, SharingMode, IntPtr.Zero, FileMode.Open, EFileAttributes.Overlapped, IntPtr.Zero);
                }
                _fileStream = new FileStream(_fileHandle, FileAccess.ReadWrite, 22, true);
            }
            catch (Exception)
            {
                _fileHandle = null;
                // If we were tring to get exclusive access try again
                if (SharingMode == FileShare.None)
                {
                    SharingMode = FileShare.ReadWrite;
                    return OpenConnection();
                }

                return false;
            }

            return true;
        }

        #region System.IO.Stream Properties
        public override bool CanRead { get { return _fileStream?.CanRead ?? false; } }

        public override bool CanWrite { get { return _fileStream?.CanWrite ?? false; } }

        public override bool CanSeek { get { return _fileStream?.CanSeek ?? false; } }

        public override long Length { get { return _fileStream?.Length ?? 0; } }

        public override long Position
        {
            get
            {
                return _fileStream?.Position ?? 0;
            }

            set
            {
                if (_fileStream != null)
                    _fileStream.Position = value;
            }
        }
        #endregion

        #region System.IO.Stream Methods
        public override void Close()
        {
            _fileStream?.Close();
            _fileHandle?.Close();
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _fileStream?.BeginRead(buffer, 0, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            // TODO: Handle device not connected
            return _fileStream?.EndRead(asyncResult) ?? -1;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Debug.WriteLine("Writing: " + BitConverter.ToString(buffer));

            if (UseFullReportSize)
            {
                var buf = new byte[22];
                buffer.CopyTo(buf, 0);
                buffer = buf;
            }

            lock (_writerBlock)
            {
                if (UseWriteFile)
                {
                    uint written = 0;
                    var nativeOverlap = new NativeOverlapped();

                    // Provide a reset event that will get set once asynchronouse writing has completed
                    var resetEvent = new ManualResetEvent(false);
                    nativeOverlap.EventHandle = resetEvent.SafeWaitHandle.DangerousGetHandle();

                    // success is most likely to be false which can mean it is being completed asynchronously, in this case we need to wait
                    bool success = WriteFile(_fileHandle, buffer, (uint)buffer.Length, out written, ref nativeOverlap);
                    int error = Marshal.GetLastWin32Error();

                    if (!success && error == (int)Win32Error.IoPending)
                    {
                        // Wait for the async operation to complete
                        success = GetOverlappedResult(_fileHandle, in nativeOverlap, out written, true);
                        error = Marshal.GetLastWin32Error();
                    }

                    if (!success && error != (int)Win32Error.Success)
                    {
                        throw new IOException($"{new Win32Exception(error).Message} ({error})");
                    }

                    // Debug.WriteLineIf(!success, "WriteFile/GetOverlappedResult returned failure but GetLastWin32Error returned success"); // Seems to consistently do this
                    Debug.WriteLineIf(error != (int)Win32Error.Success, $"WriteFile/GetOverlappedResult returned success but GetLastWin32Error returned \"{new Win32Exception(error).Message}\" ({error})");
                }
                else
                {
                    _fileStream?.Write(buffer, 0, buffer.Length);
                    // Should we even bother using SetOutputReport?
                }
            }
        }

        public override void WriteByte(byte value)
        {
            Debug.WriteLine("Writing single byte");
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            Debug.WriteLine("Flushing");
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            Debug.WriteLine("Seeking");
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            Debug.WriteLine("Setting Length");
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Debug.WriteLine("Read");
            throw new NotImplementedException();
        }
        #endregion
    }
}
