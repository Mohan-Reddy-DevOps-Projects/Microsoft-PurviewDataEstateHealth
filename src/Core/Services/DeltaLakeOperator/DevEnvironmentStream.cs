// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;

/// <summary>
/// When we write from Windows Dev Environment, the _delta_log/**.json files are written with CRLF (\r\n) ending. Synapse SQL has issue reading this delta lake format manifest.
/// This class is specifically written so that, only when generating the delta lake files from locally, we generate with LF ending (\n) instead of CRLF (\r\n).
/// This class should NOT be initialized in Production setting.
/// </summary>
public class DevEnvironmentStream : Stream
{
    private readonly Stream baseStream;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseStream"></param>
    public DevEnvironmentStream(Stream baseStream)
    {
        this.baseStream = baseStream;
    }

    /// <inheritdoc />
    public override void Write(byte[] buffer, int offset, int count)
    {
        using (MemoryStream output = new MemoryStream())
        {
            for (int i = offset; i < (offset + count); i++)
            {
                if (i < buffer.Length - 1 && buffer[i] == (byte)'\r' && buffer[i + 1] == (byte)'\n')
                {
                    output.WriteByte((byte)'\n');
                    i++; // Skip the '\n' since it's already processed
                }
                else
                {
                    output.WriteByte(buffer[i]);
                }
            }

            baseStream.Write(output.ToArray(), 0, (int)output.Length);
        }
    }

    /// <inheritdoc />
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
    {
        using (MemoryStream output = new MemoryStream())
        {
            for (int i = offset; i < (offset + count); i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (i < buffer.Length - 1 && buffer[i] == (byte)'\r' && buffer[i + 1] == (byte)'\n')
                {
                    output.WriteByte((byte)'\n');
                    i++; // Skip the '\n' since it's already processed
                }
                else
                {
                    output.WriteByte(buffer[i]);
                }
            }

            await baseStream.WriteAsync(output.ToArray(), 0, (int)output.Length, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public override bool CanRead => baseStream.CanRead;

    /// <inheritdoc />
    public override bool CanSeek => baseStream.CanSeek;

    /// <inheritdoc />
    public override bool CanWrite => baseStream.CanWrite;

    /// <inheritdoc />
    public override long Length => baseStream.Length;

    /// <inheritdoc />
    public override long Position { get => baseStream.Position; set => baseStream.Position = value; }

    /// <inheritdoc />
    public override void Flush() => baseStream.Flush();

    /// <inheritdoc />
    public override int Read(byte[] buffer, int offset, int count) => baseStream.Read(buffer, offset, count);

    /// <inheritdoc />
    public override long Seek(long offset, SeekOrigin origin) => baseStream.Seek(offset, origin);

    /// <inheritdoc />
    public override void SetLength(long value) => baseStream.SetLength(value);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            baseStream.Dispose();
        }

        base.Dispose(disposing);
    }
}
