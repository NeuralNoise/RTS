using System;
using System.Drawing;

public interface IRenderContext : IDeviceContext, IDisposable {
    int Width { get; }
    int Height { get; }

    void Resize(int w, int h);

    float DPIX { get; }
    float DPIY { get; }

    bool IsDisposed { get; }
}