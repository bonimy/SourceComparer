using System;

namespace SourceComparer
{
    public interface IProcess : IDisposable
    {
        int Run();
    }
}
