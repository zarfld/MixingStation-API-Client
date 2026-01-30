/**
 * Exception Hierarchy - MixingStation client errors
 * 
 * Design: Phase 1 Detailed Design (04-design/phase-1-detailed-design.md)
 * - Section 6: Error Handling Design
 * 
 * Exception types:
 * - TransportException: HTTP errors (4xx, 5xx, timeout)
 * - NormalizationException: Data transformation errors
 * - ExportException: Excel file I/O errors
 */

using System;
using System.Net;

namespace MixingStation.Client.Exceptions
{
    /// <summary>
    /// Base exception for all MixingStation client errors.
    /// </summary>
    public class MixingStationException : Exception
    {
        public MixingStationException(string message) : base(message) { }
        public MixingStationException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// HTTP transport errors (network, timeout, 4xx/5xx).
    /// </summary>
    public class TransportException : MixingStationException
    {
        public HttpStatusCode? StatusCode { get; }

        public TransportException(string message, HttpStatusCode? statusCode = null) 
            : base(message)
        {
            StatusCode = statusCode;
        }

        public TransportException(string message, HttpStatusCode? statusCode, Exception inner) 
            : base(message, inner)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Data normalization errors (invalid format, missing fields).
    /// </summary>
    public class NormalizationException : MixingStationException
    {
        public NormalizationException(string message) : base(message) { }
        public NormalizationException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Excel export errors (file I/O, invalid path, EPPlus errors).
    /// </summary>
    public class ExportException : MixingStationException
    {
        public ExportException(string message) : base(message) { }
        public ExportException(string message, Exception inner) : base(message, inner) { }
    }
}
