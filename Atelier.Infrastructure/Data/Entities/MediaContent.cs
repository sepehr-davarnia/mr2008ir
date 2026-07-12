using System;

namespace Atelier.Infrastructure.Data.Entities;

public class MediaContent
{
    public int Id { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
}
