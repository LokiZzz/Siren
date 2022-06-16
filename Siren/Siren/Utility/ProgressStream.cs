using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Siren.Utility
{
    public class ProgressStream : Stream
    {
        private Stream m_input = null;
        private long m_length = 0L;
        private long m_position = 0L;
        public event EventHandler<ProgressEventArgs> UpdateProgress;

        public ProgressStream(Stream input)
        {
            m_input = input;
            m_length = input.Length;
        }
        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int n = m_input.Read(buffer, offset, count);
            m_position += n;
            UpdateProgress?.Invoke(this, new ProgressEventArgs((1.0f * m_position) / m_length));
            return n;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => m_length;
        public override long Position
        {
            get { return m_position; }
            set { throw new System.NotImplementedException(); }
        }
    }

    public class ProgressEventArgs : EventArgs
    {
        private float m_progress;

        public ProgressEventArgs(float progress)
        {
            m_progress = progress;
        }

        public float Progress => m_progress;

        public string Message { get; set; }
    }
}
