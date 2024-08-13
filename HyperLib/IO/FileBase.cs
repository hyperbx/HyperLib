namespace HyperLib.IO
{
    public class FileBase : IDisposable
    {
        /// <summary>
        /// The location of this file.
        /// </summary>
        public string Location { get; private set; }

        /// <summary>
        /// The extension used for the file name.
        /// </summary>
        public virtual string Extension { get; }

        /// <summary>
        /// The method used for writing the file.
        /// </summary>
        public virtual EWriteMode WriteMode { get; set; } = EWriteMode.Logical;

        /// <summary>
        /// Leaves the <see cref="Stream"/> open after writing.
        /// <para>If left open, the stream must manually be disposed using the <see cref="Dispose"/> method.</para>
        /// </summary>
        public virtual bool LeaveOpen { get; set; } = false;

        public Stream Stream { get; private set; }

        public FileBase(EWriteMode in_writeMode = EWriteMode.Logical, bool in_isLeaveOpen = false)
        {
            WriteMode = in_writeMode;
            LeaveOpen = in_isLeaveOpen;
        }

        public FileBase(string in_path, EWriteMode in_writeMode = EWriteMode.Logical, bool in_isLeaveOpen = false)
            : this(in_writeMode, in_isLeaveOpen)
        {
            Read(in_path);
        }

        public virtual void Read(string in_path)
        {
            Location = in_path;

            if (string.IsNullOrEmpty(in_path))
                throw new ArgumentNullException(nameof(in_path));

            if (!File.Exists(in_path))
                throw new FileNotFoundException("The specified file does not exist.", in_path);

            Stream = new FileStream(in_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            Read(Stream);
        }

        public virtual void Read(Stream in_stream)
        {
            throw new NotImplementedException();
        }

        public virtual void Write(string in_path, bool in_isOverwrite = true)
        {
            if (string.IsNullOrEmpty(in_path))
                throw new ArgumentNullException(nameof(in_path));

            if (!in_isOverwrite && File.Exists(in_path))
                throw new IOException("The specified file already exists.");

            switch (WriteMode)
            {
                case EWriteMode.Logical:
                {
                    Location = in_path;

                    using (var stream = new FileStream(in_path, FileMode.Create, FileAccess.ReadWrite))
                        Write(stream);

                    break;
                }

                case EWriteMode.Fixed:
                {
                    if (!string.IsNullOrEmpty(Location) && File.Exists(Location))
                    {
                        if (in_path == Location)
                            return;

                        // Copy the fixed file to the new writing location.
                        File.Copy(Location, in_path, true);
                    }

                    using (var stream = new FileStream(in_path, FileMode.Open, FileAccess.ReadWrite))
                        Write(stream);

                    break;
                }
            }
        }

        public virtual void Write(Stream in_stream, bool in_isOverwrite = true)
        {
            throw new NotImplementedException();
        }

        public virtual void Write(bool in_isOverwrite = true)
        {
            Write(Location, in_isOverwrite);
        }

        public virtual void Import(string in_path) { }

        public virtual void Export(string in_path = "") { }

        public void Dispose()
        {
            Stream?.Dispose();
        }

        public enum EWriteMode
        {
            /// <summary>
            /// Writes to the file directly.
            /// </summary>
            Fixed,

            /// <summary>
            /// Writes the entire file from scratch.
            /// </summary>
            Logical
        }
    }
}
