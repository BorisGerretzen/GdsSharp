namespace GdsSharp.Lib.Library;

public class GdsLibraryBuilderOptions
{
    /// <summary>
    /// How to store vertices during the building process.
    /// </summary>
    public VertexStoreType VertexStoreType { get; set; } = VertexStoreType.Memory;
    
    /// <summary>
    /// Whether to build bounding boxes during the building process.
    /// </summary>
    public bool BuildBoundingBoxes { get; set; } = false;
    
    /// <summary>
    /// The size of the read buffer in bytes when reading GDSII files.
    /// </summary>
    public int ReadBufferSize { get; set; } = GdsGlobals.DefaultReaderBufferSize;
}

public enum VertexStoreType
{
    /// <summary>
    /// Uses an in-memory vertex store. This is suitable for use cases where the entire GDSII file can fit into memory.
    /// </summary>
    Memory,
    
    /// <summary>
    /// Uses a disk-based vertex store. This is suitable for use cases where the GDSII file is too large to fit into memory.
    /// </summary>
    Disk
}