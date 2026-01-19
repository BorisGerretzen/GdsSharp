using System.Buffers;
using System.Runtime.InteropServices;
using GdsSharp.Lib.Library.BoundingBox;
using GdsSharp.Lib.Library.Builder;
using GdsSharp.Lib.Library.Builder.Payload;
using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Reading;
using GdsSharp.Lib.Reading.Enum;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Editing;

public sealed class GdsEditSession(GdsLibrary baseLibrary, IGdsVertexStore? deltaVertexStore = null) : IDisposable
{
    private readonly Dictionary<int, ElementDelta> _elementDeltas = new(); // key: base element id
    private readonly List<NewElement> _newElements = new();

    private readonly Dictionary<int, StructureDelta> _structureDeltas = new();

    private GdsLibrary Base { get; } = baseLibrary ?? throw new ArgumentNullException(nameof(baseLibrary));

    internal IGdsVertexStore DeltaVertices { get; } = deltaVertexStore ?? new ChunkedVertexStore();

    internal IReadOnlyDictionary<int, StructureDelta> StructureDeltas => _structureDeltas;
    internal IReadOnlyDictionary<int, ElementDelta> ElementDeltas => _elementDeltas;
    internal IReadOnlyList<NewElement> NewElements => _newElements;

    public void Dispose()
    {
        if (DeltaVertices is IDisposable d) d.Dispose();
    }

    /// <summary>
    ///     Build a new immutable library by replaying the base library and this edit session's deltas.
    /// </summary>
    public GdsLibrary Build(Func<IGdsVertexStore>? vertexStoreFactory = null)
    {
        return GdsReconciler.Rebuild(Base, this, vertexStoreFactory);
    }

    /// <summary>
    ///    Delete an element from a structure.
    /// </summary>
    /// <param name="structureId">Structure to delete from.</param>
    /// <param name="element">Element key to delete.</param>
    public void DeleteElement(CellId structureId, ElementKey element)
    {
        var list = EnsureStructureElementList(structureId);
        for (var i = 0; i < list.Count; i++)
            if (list[i].Equals(element))
            {
                list.RemoveAt(i);
                return;
            }
    }

    /// <summary>
    /// Moves an element from one structure to another.
    /// </summary>
    /// <param name="fromStructure">Source structure.</param>
    /// <param name="toStructure">Target structure.</param>
    /// <param name="element">Element index to move.</param>
    /// <param name="insertAt">Optional insert position in target structure (default: end).</param>
    /// <exception cref="InvalidOperationException">Thrown if element not found in source structure.</exception>
    public void MoveElement(CellId fromStructure, CellId toStructure, ElementKey element, int insertAt = -1)
    {
        var src = EnsureStructureElementList(fromStructure);
        var dst = EnsureStructureElementList(toStructure);

        var removed = false;
        for (var i = 0; i < src.Count; i++)
            if (src[i].Equals(element))
            {
                src.RemoveAt(i);
                removed = true;
                break;
            }

        if (!removed)
            throw new InvalidOperationException("Element not found in source structure.");

        if (insertAt < 0 || insertAt > dst.Count) insertAt = dst.Count;
        dst.Insert(insertAt, element);
    }

    #region Create

    public ElementKey CreateBoundary(CellId structureId, short layer, short dataType, ReadOnlySpan<GdsPoint> points,
        GdsElementCommon? common = null, int insertAt = -1)
    {
        var vr = WriteVertices(points);
        var bbox = GdsBoundingBox.FromPoints(points);

        var payload = new BoundaryPayload(layer, dataType, vr.Offset, vr.Count);
        var newId = _newElements.Count;
        _newElements.Add(new NewElement
        {
            Kind = GdsElementKind.Boundary,
            Common = common,
            Payload = payload,
            BoundingBox = bbox
        });

        InsertIntoStructure(structureId, ElementKey.New(newId), insertAt);
        return ElementKey.New(newId);
    }

    public ElementKey CreatePath(CellId structureId, short layer, short dataType, ReadOnlySpan<GdsPoint> points,
        int? width = null, GdsPathType? pathType = null, GdsElementCommon? common = null, int insertAt = -1)
    {
        var vr = WriteVertices(points);
        var bbox = ComputePathBBox(points, width);

        var payload = new PathPayload(layer, dataType, pathType, width, vr.Offset, vr.Count);
        var newId = _newElements.Count;
        _newElements.Add(new NewElement
        {
            Kind = GdsElementKind.Path,
            Common = common,
            Payload = payload,
            BoundingBox = bbox
        });

        InsertIntoStructure(structureId, ElementKey.New(newId), insertAt);
        return ElementKey.New(newId);
    }

    public ElementKey CreateBox(CellId structureId, short layer, short boxType, ReadOnlySpan<GdsPoint> points,
        GdsElementCommon? common = null, int insertAt = -1)
    {
        var vr = WriteVertices(points);
        var bbox = GdsBoundingBox.FromPoints(points);
        var payload = new BoxPayload(layer, boxType, vr.Offset);

        var newId = _newElements.Count;
        _newElements.Add(new NewElement
        {
            Kind = GdsElementKind.Box,
            Common = common,
            Payload = payload,
            BoundingBox = bbox
        });

        InsertIntoStructure(structureId, ElementKey.New(newId), insertAt);
        return ElementKey.New(newId);
    }

    public ElementKey CreateNode(CellId structureId, short layer, short nodeType, ReadOnlySpan<GdsPoint> points,
        GdsElementCommon? common = null, int insertAt = -1)
    {
        var vr = WriteVertices(points);
        var bbox = GdsBoundingBox.FromPoints(points);
        var payload = new NodePayload(layer, nodeType, vr.Offset, vr.Count);

        var newId = _newElements.Count;
        _newElements.Add(new NewElement
        {
            Kind = GdsElementKind.Node,
            Common = common,
            Payload = payload,
            BoundingBox = bbox
        });

        InsertIntoStructure(structureId, ElementKey.New(newId), insertAt);
        return ElementKey.New(newId);
    }

    public ElementKey CreateText(CellId structureId, short layer, short textType, GdsPoint origin, string text,
        PresentationInfo? presentation = null, GdsPathType? pathType = null, int? width = null, GdsStransInfo? strans = null,
        GdsElementCommon? common = null, int insertAt = -1)
    {
        var payload = new TextPayload(layer, textType, presentation, pathType, width, strans, origin, text);

        var newId = _newElements.Count;
        _newElements.Add(new NewElement
        {
            Kind = GdsElementKind.Text,
            Common = common,
            Payload = payload,
            BoundingBox = null
        });

        InsertIntoStructure(structureId, ElementKey.New(newId), insertAt);
        return ElementKey.New(newId);
    }

    public ElementKey CreateStructureReferences(CellId structureId, string targetName, GdsPoint origin, GdsStransInfo? strans = null,
        GdsElementCommon? common = null, int insertAt = -1)
    {
        var payload = new SRefPayload(structureId, targetName, strans, origin);
        var newId = _newElements.Count;
        _newElements.Add(new NewElement
        {
            Kind = GdsElementKind.StructureReference,
            Common = common,
            Payload = payload,
            BoundingBox = null
        });

        InsertIntoStructure(structureId, ElementKey.New(newId), insertAt);
        return ElementKey.New(newId);
    }

    public ElementKey CreateArrayReferences(CellId structureId, string targetName, int rows, int columns, GdsPoint rowVector, GdsPoint colVector, GdsPoint origin,
        GdsStransInfo? strans = null, GdsElementCommon? common = null, int insertAt = -1)
    {
        var payload = new ARefPayload(structureId, targetName, strans, rows, columns, rowVector, colVector, origin);
        var newId = _newElements.Count;
        _newElements.Add(new NewElement
        {
            Kind = GdsElementKind.ArrayReference,
            Common = common,
            Payload = payload,
            BoundingBox = null
        });

        InsertIntoStructure(structureId, ElementKey.New(newId), insertAt);
        return ElementKey.New(newId);
    }

    #endregion

    /// <summary>
    /// Replace the vertices of an element.
    /// </summary>
    /// <param name="element">Element to modify.</param>
    /// <param name="newPoints">New vertex list.</param>
    /// <exception cref="InvalidOperationException">If element kind does not support vertices.</exception>
    public void ReplacePoints(ElementKey element, ReadOnlySpan<GdsPoint> newPoints)
    {
        var kind = GetKind(element);
        if (kind is not (GdsElementKind.Boundary or GdsElementKind.Path or GdsElementKind.Box or GdsElementKind.Node))
            throw new InvalidOperationException("Element kind does not support vertices.");

        var vr = WriteVertices(newPoints);

        if (element.IsBase)
        {
            var d = GetOrCreateElementDelta(element.Id);
            d.PayloadOverride = kind switch
            {
                GdsElementKind.Boundary => Merge((BoundaryOverride?)d.PayloadOverride, vertices: vr),
                GdsElementKind.Path => Merge((PathOverride?)d.PayloadOverride, vertices: vr),
                GdsElementKind.Box => Merge((BoxOverride?)d.PayloadOverride, vertices: vr),
                GdsElementKind.Node => Merge((NodeOverride?)d.PayloadOverride, vertices: vr),
                _ => throw new InvalidOperationException("Element kind does not support vertices.")
            };

            d.BoundingBoxOverride = kind switch
            {
                GdsElementKind.Path => ComputePathBBox(newPoints, GetEffectivePathWidth(element)),
                _ => GdsBoundingBox.FromPoints(newPoints)
            };
        }
        else
        {
            ref var ne = ref CollectionsMarshal.AsSpan(_newElements)[element.Id];
            ne.Payload = kind switch
            {
                GdsElementKind.Boundary => (BoundaryPayload)ne.Payload with { VertexOffset = vr.Offset, VertexCount = vr.Count },
                GdsElementKind.Path => (PathPayload)ne.Payload with { VertexOffset = vr.Offset, VertexCount = vr.Count },
                GdsElementKind.Box => (BoxPayload)ne.Payload with { VertexOffset = vr.Offset },
                GdsElementKind.Node => (NodePayload)ne.Payload with { VertexOffset = vr.Offset, VertexCount = vr.Count },
                _ => throw new InvalidOperationException("Element kind does not support vertices.")
            };
            ne.BoundingBox = kind switch
            {
                GdsElementKind.Path => ComputePathBBox(newPoints, ((PathPayload)ne.Payload).Width),
                _ => GdsBoundingBox.FromPoints(newPoints)
            };
        }
    }

    /// <summary>
    /// Changes the layer of an element.
    /// </summary>
    /// <param name="element">Element to modify.</param>
    /// <param name="layer">New layer.</param>
    public void SetLayer(ElementKey element, short layer)
    {
        var kind = GetKind(element);
        if (element.IsBase)
        {
            var d = GetOrCreateElementDelta(element.Id);
            d.PayloadOverride = kind switch
            {
                GdsElementKind.Boundary => Merge((BoundaryOverride?)d.PayloadOverride, layer),
                GdsElementKind.Path => Merge((PathOverride?)d.PayloadOverride, layer),
                GdsElementKind.Box => Merge((BoxOverride?)d.PayloadOverride, layer),
                GdsElementKind.Node => Merge((NodeOverride?)d.PayloadOverride, layer),
                GdsElementKind.Text => Merge((TextOverride?)d.PayloadOverride, layer),
                _ => d.PayloadOverride
            };
        }
        else
        {
            ref var ne = ref CollectionsMarshal.AsSpan(_newElements)[element.Id];
            ne.Payload = kind switch
            {
                GdsElementKind.Boundary => (BoundaryPayload)ne.Payload with { Layer = layer },
                GdsElementKind.Path => (PathPayload)ne.Payload with { Layer = layer },
                GdsElementKind.Box => (BoxPayload)ne.Payload with { Layer = layer },
                GdsElementKind.Node => (NodePayload)ne.Payload with { Layer = layer },
                GdsElementKind.Text => (TextPayload)ne.Payload with { Layer = layer },
                _ => ne.Payload
            };
        }
    }

    /// <summary>
    /// Changes the data type of a boundary or path element.
    /// </summary>
    /// <param name="element">Element to modify.</param>
    /// <param name="dataType">New data type.</param>
    /// <exception cref="InvalidOperationException">Element does not support data type.</exception>
    public void SetDataType(ElementKey element, short dataType)
    {
        var kind = GetKind(element);
        if (kind is not (GdsElementKind.Boundary or GdsElementKind.Path))
            throw new InvalidOperationException("Only boundary/path support DataType.");

        if (element.IsBase)
        {
            var d = GetOrCreateElementDelta(element.Id);
            d.PayloadOverride = kind switch
            {
                GdsElementKind.Boundary => Merge((BoundaryOverride?)d.PayloadOverride, dataType: dataType),
                GdsElementKind.Path => Merge((PathOverride?)d.PayloadOverride, dataType: dataType),
                _ => throw new InvalidOperationException("Only boundary/path support DataType.")
            };
        }
        else
        {
            ref var ne = ref CollectionsMarshal.AsSpan(_newElements)[element.Id];
            ne.Payload = kind switch
            {
                GdsElementKind.Boundary => (BoundaryPayload)ne.Payload with { DataType = dataType },
                GdsElementKind.Path => (PathPayload)ne.Payload with { DataType = dataType },
                _ => throw new InvalidOperationException("Only boundary/path support DataType.")
            };
        }
    }

    /// <summary>
    /// Changes the text of a text element.
    /// </summary>
    /// <param name="element">Element to modify.</param>
    /// <param name="text">New text.</param>
    /// <exception cref="InvalidOperationException">Element is not text.</exception>
    public void SetText(ElementKey element, string text)
    {
        var kind = GetKind(element);
        if (kind != GdsElementKind.Text) throw new InvalidOperationException("Element is not TEXT.");

        if (element.IsBase)
        {
            var d = GetOrCreateElementDelta(element.Id);
            d.PayloadOverride = Merge((TextOverride?)d.PayloadOverride, text: text);
        }
        else
        {
            ref var ne = ref CollectionsMarshal.AsSpan(_newElements)[element.Id];
            ne.Payload = (TextPayload)ne.Payload with { Text = text };
        }
    }

    /// <summary>
    /// Sets a property (attribute/value pair) on an element.
    /// </summary>
    /// <param name="element">Element to modify.</param>
    /// <param name="attribute">Attribute ID.</param>
    /// <param name="value">New value.</param>
    public void SetProperty(ElementKey element, short attribute, string value)
    {
        if (element.IsBase)
        {
            var d = GetOrCreateElementDelta(element.Id);
            d.PropertiesOverride ??= GetBaseProperties(element.Id)?.ToList() ?? new List<(short Attr, string Value)>();
            for (var i = 0; i < d.PropertiesOverride.Count; i++)
                if (d.PropertiesOverride[i].Attr == attribute)
                {
                    d.PropertiesOverride[i] = (attribute, value);
                    return;
                }

            d.PropertiesOverride.Add((attribute, value));
        }
        else
        {
            ref var ne = ref CollectionsMarshal.AsSpan(_newElements)[element.Id];
            ne.Properties ??= [];
            for (var i = 0; i < ne.Properties.Count; i++)
                if (ne.Properties[i].Attr == attribute)
                {
                    ne.Properties[i] = (attribute, value);
                    return;
                }

            ne.Properties.Add((attribute, value));
        }
    }

    /// <summary>
    /// Removes a property (attribute/value pair) from an element.
    /// </summary>
    /// <param name="element">Element to modify.</param>
    /// <param name="attribute">Attribute ID.</param>
    public void RemoveProperty(ElementKey element, short attribute)
    {
        if (element.IsBase)
        {
            var d = GetOrCreateElementDelta(element.Id);
            d.PropertiesOverride ??= GetBaseProperties(element.Id)?.ToList() ?? new List<(short Attr, string Value)>();
            d.PropertiesOverride.RemoveAll(p => p.Attr == attribute);
        }
        else
        {
            ref var ne = ref CollectionsMarshal.AsSpan(_newElements)[element.Id];
            ne.Properties?.RemoveAll(p => p.Attr == attribute);
        }
    }

    /// <summary>
    /// Transforms an element by applying a translation/rotation/mirror operation.
    /// </summary>
    /// <param name="element">Element to modify.</param>
    /// <param name="op">Transformation operation.</param>
    /// <param name="dx">Translation in X direction.</param>
    /// <param name="dy">Translation in Y direction.</param>
    /// <param name="pivot">Pivot point for rotation/mirroring (default: origin).</param>
    /// <exception cref="NotSupportedException">If element kind does not support transformation.</exception>
    public void Transform(ElementKey element, TransformOp op, int dx = 0, int dy = 0, GdsPoint? pivot = null)
    {
        var kind = GetKind(element);
        var pv = pivot ?? new GdsPoint(0, 0);

        switch (kind)
        {
            case GdsElementKind.Boundary:
            case GdsElementKind.Path:
            case GdsElementKind.Box:
            case GdsElementKind.Node:
                TransformVertices(element, kind, op, dx, dy, pv);
                break;

            case GdsElementKind.Text:
                TransformText(element, op, dx, dy, pv);
                break;

            case GdsElementKind.StructureReference:
                TransformSRef(element, op, dx, dy, pv);
                break;

            case GdsElementKind.ArrayReference:
                TransformARef(element, op, dx, dy, pv);
                break;

            default:
                throw new NotSupportedException($"Transform not supported for element kind {kind}.");
        }
    }

    private void TransformVertices(ElementKey element, GdsElementKind kind, TransformOp op, int dx, int dy, GdsPoint pivot)
    {
        var (vref, count) = GetEffectiveVertices(element, kind);
        if (count <= 0) return;

        GdsPoint[]? rented = null;
        var pts = count <= 256
            ? stackalloc GdsPoint[count]
            : (rented = ArrayPool<GdsPoint>.Shared.Rent(count)).AsSpan(0, count);

        ReadVertices(vref, pts);
        for (var i = 0; i < pts.Length; i++) pts[i] = TransformUtil.Apply(pts[i], op, pivot, dx, dy);

        ReplacePoints(element, pts);

        if (rented != null) ArrayPool<GdsPoint>.Shared.Return(rented);
    }

    private void TransformText(ElementKey element, TransformOp op, int dx, int dy, GdsPoint pivot)
    {
        var payload = GetEffectiveText(element);
        var newOrigin = TransformUtil.Apply(payload.Origin, op, pivot, dx, dy);
        var newStrans = StransUtil.Apply(payload.Strans, op);

        if (element.IsBase)
        {
            var d = GetOrCreateElementDelta(element.Id);
            d.PayloadOverride = Merge((TextOverride?)d.PayloadOverride, origin: newOrigin, strans: newStrans);
        }
        else
        {
            ref var ne = ref CollectionsMarshal.AsSpan(_newElements)[element.Id];
            var t = (TextPayload)ne.Payload;
            ne.Payload = t with { Origin = newOrigin, Strans = newStrans };
        }
    }

    private void TransformSRef(ElementKey element, TransformOp op, int dx, int dy, GdsPoint pivot)
    {
        var payload = GetEffectiveSRef(element);
        var newOrigin = TransformUtil.Apply(payload.Origin, op, pivot, dx, dy);
        var newStrans = StransUtil.Apply(payload.Strans, op);

        if (element.IsBase)
        {
            var d = GetOrCreateElementDelta(element.Id);
            d.PayloadOverride = Merge((SRefOverride?)d.PayloadOverride, origin: newOrigin, strans: newStrans);
        }
        else
        {
            ref var ne = ref CollectionsMarshal.AsSpan(_newElements)[element.Id];
            var s = (SRefPayload)ne.Payload;
            ne.Payload = s with { Origin = newOrigin, Strans = newStrans };
        }
    }

    private void TransformARef(ElementKey element, TransformOp op, int dx, int dy, GdsPoint pivot)
    {
        var payload = GetEffectiveARef(element);
        var newOrigin = TransformUtil.Apply(payload.Origin, op, pivot, dx, dy);
        var newRow = TransformUtil.Apply(payload.RowVector, op, pivot, dx, dy);
        var newCol = TransformUtil.Apply(payload.ColumnVector, op, pivot, dx, dy);
        var newStrans = StransUtil.Apply(payload.Strans, op);

        if (element.IsBase)
        {
            var d = GetOrCreateElementDelta(element.Id);
            d.PayloadOverride = Merge((ARefOverride?)d.PayloadOverride, origin: newOrigin, rowVector: newRow, colVector: newCol, strans: newStrans);
        }
        else
        {
            ref var ne = ref CollectionsMarshal.AsSpan(_newElements)[element.Id];
            var a = (ARefPayload)ne.Payload;
            ne.Payload = a with { Origin = newOrigin, RowVector = newRow, ColumnVector = newCol, Strans = newStrans };
        }
    }

    private List<ElementKey> EnsureStructureElementList(CellId structureId)
    {
        if (!_structureDeltas.TryGetValue(structureId.Id, out var sd))
        {
            sd = new StructureDelta();
            _structureDeltas.Add(structureId.Id, sd);
        }

        sd.Elements ??= MaterializeBaseElementList(structureId);
        return sd.Elements;
    }

    private List<ElementKey> MaterializeBaseElementList(CellId structureId)
    {
        var s = Base.Structures[structureId.Id];
        var list = new List<ElementKey>(s.ElementCount);
        var start = s.ElementStartIndex;
        var end = start + s.ElementCount;
        for (var i = start; i < end; i++) list.Add(ElementKey.Base(i));
        return list;
    }

    private void InsertIntoStructure(CellId structureId, ElementKey element, int insertAt)
    {
        var list = EnsureStructureElementList(structureId);
        if (insertAt < 0 || insertAt > list.Count) insertAt = list.Count;
        list.Insert(insertAt, element);
    }

    private ElementDelta GetOrCreateElementDelta(int baseElementId)
    {
        if (!_elementDeltas.TryGetValue(baseElementId, out var d))
        {
            d = new ElementDelta();
            _elementDeltas.Add(baseElementId, d);
        }

        return d;
    }

    private GdsElementKind GetKind(ElementKey element)
    {
        return element.IsBase ? Base.Elements[element.Id].Kind : _newElements[element.Id].Kind;
    }

    private VertexRef WriteVertices(ReadOnlySpan<GdsPoint> points)
    {
        var off = DeltaVertices.Write(points);
        return new VertexRef(VertexStoreKind.Delta, off, points.Length);
    }

    private void ReadVertices(VertexRef v, Span<GdsPoint> destination)
    {
        var store = v.Store == VertexStoreKind.Base ? Base.VertexStore : DeltaVertices;
        store.Read(v.Offset, destination);
    }

    private (VertexRef Ref, int Count) GetEffectiveVertices(ElementKey element, GdsElementKind kind)
    {
        if (element.IsBase)
        {
            if (_elementDeltas.TryGetValue(element.Id, out var d) && d.PayloadOverride is not null)
                switch (kind)
                {
                    case GdsElementKind.Boundary:
                        if (d.PayloadOverride is BoundaryOverride { Vertices: not null } bo) return (bo.Vertices.Value, bo.Vertices.Value.Count);
                        break;
                    case GdsElementKind.Path:
                        if (d.PayloadOverride is PathOverride { Vertices: not null } po) return (po.Vertices.Value, po.Vertices.Value.Count);
                        break;
                    case GdsElementKind.Box:
                        if (d.PayloadOverride is BoxOverride { Vertices: not null } xo) return (xo.Vertices.Value, xo.Vertices.Value.Count);
                        break;
                    case GdsElementKind.Node:
                        if (d.PayloadOverride is NodeOverride { Vertices: not null } no) return (no.Vertices.Value, no.Vertices.Value.Count);
                        break;
                }

            // fallback to base payload
            return kind switch
            {
                GdsElementKind.Boundary => (
                    new VertexRef(VertexStoreKind.Base, Base.Boundaries[Base.Elements[element.Id].Index].VertexOffset, Base.Boundaries[Base.Elements[element.Id].Index].VertexCount),
                    Base.Boundaries[Base.Elements[element.Id].Index].VertexCount),
                GdsElementKind.Path => (new VertexRef(VertexStoreKind.Base, Base.Paths[Base.Elements[element.Id].Index].VertexOffset, Base.Paths[Base.Elements[element.Id].Index].VertexCount),
                    Base.Paths[Base.Elements[element.Id].Index].VertexCount),
                GdsElementKind.Box => (new VertexRef(VertexStoreKind.Base, Base.Boxes[Base.Elements[element.Id].Index].VertexOffset, GdsGlobals.BoxPointCount),
                    GdsGlobals.BoxPointCount),
                GdsElementKind.Node => (new VertexRef(VertexStoreKind.Base, Base.Nodes[Base.Elements[element.Id].Index].VertexOffset, Base.Nodes[Base.Elements[element.Id].Index].VertexCount),
                    Base.Nodes[Base.Elements[element.Id].Index].VertexCount),
                _ => throw new InvalidOperationException("No vertices")
            };
        }

        var ne = _newElements[element.Id];
        return kind switch
        {
            GdsElementKind.Boundary => (new VertexRef(VertexStoreKind.Delta, ((BoundaryPayload)ne.Payload).VertexOffset, ((BoundaryPayload)ne.Payload).VertexCount),
                ((BoundaryPayload)ne.Payload).VertexCount),
            GdsElementKind.Path => (new VertexRef(VertexStoreKind.Delta, ((PathPayload)ne.Payload).VertexOffset, ((PathPayload)ne.Payload).VertexCount),
                ((PathPayload)ne.Payload).VertexCount),
            GdsElementKind.Box => (new VertexRef(VertexStoreKind.Delta, ((BoxPayload)ne.Payload).VertexOffset, GdsGlobals.BoxPointCount),
                GdsGlobals.BoxPointCount),
            GdsElementKind.Node => (new VertexRef(VertexStoreKind.Delta, ((NodePayload)ne.Payload).VertexOffset, ((NodePayload)ne.Payload).VertexCount),
                ((NodePayload)ne.Payload).VertexCount),
            _ => throw new InvalidOperationException("No vertices")
        };
    }

    private int? GetEffectivePathWidth(ElementKey element)
    {
        if (!element.IsBase) return ((PathPayload)_newElements[element.Id].Payload).Width;

        if (_elementDeltas.TryGetValue(element.Id, out var d) && d.PayloadOverride is PathOverride { Width: not null } po) return po.Width;
        return Base.Paths[Base.Elements[element.Id].Index].Width;
    }

    private TextPayload GetEffectiveText(ElementKey element)
    {
        if (!element.IsBase) return (TextPayload)_newElements[element.Id].Payload;

        var basePayload = Base.Texts[Base.Elements[element.Id].Index];
        if (_elementDeltas.TryGetValue(element.Id, out var d) && d.PayloadOverride is TextOverride to)
            return new TextPayload(
                to.Layer ?? basePayload.Layer,
                to.TextType ?? basePayload.TextType,
                to.Presentation ?? basePayload.Presentation,
                to.PathType ?? basePayload.PathType,
                to.Width ?? basePayload.Width,
                to.Strans ?? basePayload.Strans,
                to.Origin ?? basePayload.Origin,
                to.Text ?? basePayload.Text
            );

        return basePayload;
    }

    private SRefPayload GetEffectiveSRef(ElementKey element)
    {
        if (!element.IsBase) return (SRefPayload)_newElements[element.Id].Payload;
        var basePayload = Base.StructureReferences[Base.Elements[element.Id].Index];
        if (_elementDeltas.TryGetValue(element.Id, out var d) && d.PayloadOverride is SRefOverride so)
            return basePayload with
            {
                TargetName = so.TargetName ?? basePayload.TargetName,
                Strans = so.Strans ?? basePayload.Strans,
                Origin = so.Origin ?? basePayload.Origin
            };

        return basePayload;
    }

    private ARefPayload GetEffectiveARef(ElementKey element)
    {
        if (!element.IsBase) return (ARefPayload)_newElements[element.Id].Payload;
        var basePayload = Base.ArrayReferences[Base.Elements[element.Id].Index];
        if (_elementDeltas.TryGetValue(element.Id, out var d) && d.PayloadOverride is ARefOverride ao)
            return basePayload with
            {
                TargetName = ao.TargetName ?? basePayload.TargetName,
                Strans = ao.Strans ?? basePayload.Strans,
                Rows = ao.Rows ?? basePayload.Rows,
                Columns = ao.Columns ?? basePayload.Columns,
                RowVector = ao.RowVector ?? basePayload.RowVector,
                ColumnVector = ao.ColumnVector ?? basePayload.ColumnVector,
                Origin = ao.Origin ?? basePayload.Origin
            };

        return basePayload;
    }

    private List<(short Attr, string Value)>? GetBaseProperties(int baseElementId)
    {
        if (Base.TryGetElementProperties(baseElementId, out var props)) return null;
        return props.Select(p => (p.Attribute, p.Value)).ToList();
    }

    #region Merge helpers

    private static BoundaryOverride Merge(BoundaryOverride? cur, short? layer = null, short? dataType = null, VertexRef? vertices = null)
    {
        var c = cur ?? new BoundaryOverride(null, null, null);
        return new BoundaryOverride(layer ?? c.Layer, dataType ?? c.DataType, vertices ?? c.Vertices);
    }

    private static PathOverride Merge(PathOverride? cur, short? layer = null, short? dataType = null, GdsPathType? pathType = null, int? width = null, VertexRef? vertices = null)
    {
        var c = cur ?? new PathOverride(null, null, null, null, null);
        return new PathOverride(layer ?? c.Layer, dataType ?? c.DataType, pathType ?? c.PathType, width ?? c.Width, vertices ?? c.Vertices);
    }

    private static BoxOverride Merge(BoxOverride? cur, short? layer = null, short? boxType = null, VertexRef? vertices = null)
    {
        var c = cur ?? new BoxOverride(null, null, null);
        return new BoxOverride(layer ?? c.Layer, boxType ?? c.BoxType, vertices ?? c.Vertices);
    }

    private static NodeOverride Merge(NodeOverride? cur, short? layer = null, short? nodeType = null, VertexRef? vertices = null)
    {
        var c = cur ?? new NodeOverride(null, null, null);
        return new NodeOverride(layer ?? c.Layer, nodeType ?? c.NodeType, vertices ?? c.Vertices);
    }

    private static TextOverride Merge(TextOverride? cur, short? layer = null, short? textType = null, PresentationInfo? presentation = null,
        GdsPathType? pathType = null, int? width = null, GdsStransInfo? strans = null, GdsPoint? origin = null, string? text = null)
    {
        var c = cur ?? new TextOverride(null, null, null, null, null, null, null, null);
        return new TextOverride(
            layer ?? c.Layer,
            textType ?? c.TextType,
            presentation ?? c.Presentation,
            pathType ?? c.PathType,
            width ?? c.Width,
            strans ?? c.Strans,
            origin ?? c.Origin,
            text ?? c.Text);
    }

    private static SRefOverride Merge(SRefOverride? cur, string? targetName = null, GdsStransInfo? strans = null, GdsPoint? origin = null)
    {
        var c = cur ?? new SRefOverride(null, null, null);
        return new SRefOverride(targetName ?? c.TargetName, strans ?? c.Strans, origin ?? c.Origin);
    }

    private static ARefOverride Merge(ARefOverride? cur, string? targetName = null, GdsStransInfo? strans = null, int? rows = null, int? columns = null,
        GdsPoint? rowVector = null, GdsPoint? colVector = null, GdsPoint? origin = null)
    {
        var c = cur ?? new ARefOverride(null, null, null, null, null, null, null);
        return new ARefOverride(
            targetName ?? c.TargetName,
            strans ?? c.Strans,
            rows ?? c.Rows,
            columns ?? c.Columns,
            rowVector ?? c.RowVector,
            colVector ?? c.ColumnVector,
            origin ?? c.Origin);
    }

    private static GdsBoundingBox ComputePathBBox(ReadOnlySpan<GdsPoint> points, int? width)
    {
        var halfWidth = (width ?? 0) / 2;
        var bb = GdsBoundingBox.FromPoints(points);
        return new GdsBoundingBox(
            new GdsPoint(bb.Min.X - halfWidth, bb.Min.Y - halfWidth),
            new GdsPoint(bb.Max.X + halfWidth, bb.Max.Y + halfWidth));
    }

    #endregion
}