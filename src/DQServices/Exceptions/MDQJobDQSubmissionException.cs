namespace Microsoft.Purview.DataEstateHealth.DHModels.Exceptions;

using System;

public class MDQJobDQSubmissionException : Exception
{
    public MDQJobDQSubmissionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public override string ToString()
    {
        return $"MDQJobDQSubmissionException, inner exception: {this.InnerException}";
    }
}
