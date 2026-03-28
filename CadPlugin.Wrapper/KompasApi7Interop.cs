using System.Runtime.InteropServices;

namespace CadPlugin.Wrapper;

[ComImport]
[Guid("6A2EFAF7-8254-45A5-9DC8-2213F16AF5D7")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IApplication
{
    [DispId(1)]
    bool Visible { get; [param: MarshalAs(UnmanagedType.VariantBool)] set; }

    [DispId(2)]
    IDocuments Documents { [return: MarshalAs(UnmanagedType.Interface)] get; }

    [DispId(3)]
    void Quit();
}

[ComImport]
[Guid("8BF39F08-5537-4910-84CE-B338E55F7BCF")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IDocuments
{
    [DispId(1)]
    [return: MarshalAs(UnmanagedType.Interface)]
    object Add(int type, [MarshalAs(UnmanagedType.VariantBool)] bool visible);
}

[ComImport]
[Guid("0075EA2A-5498-4E28-BDF3-0288EB168054")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IPartDocument
{
    [DispId(5002)]
    IPart7 TopPart { [return: MarshalAs(UnmanagedType.Interface)] get; }
}

[ComImport]
[Guid("FA4A5FDE-A08C-4F5A-8C04-98395BA44307")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IPart7
{
    [DispId(501)]
    string Name { [return: MarshalAs(UnmanagedType.BStr)] get; [param: MarshalAs(UnmanagedType.BStr)] set; }

    [DispId(503)]
    [return: MarshalAs(UnmanagedType.VariantBool)]
    bool Update();
}

[ComImport]
[Guid("2C6E8A0F-EDC8-413C-9304-9278817B915B")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IModelContainer
{
    [DispId(10002)]
    ISketchs Sketchs { [return: MarshalAs(UnmanagedType.Interface)] get; }

    [DispId(10003)]
    IExtrusions Extrusions { [return: MarshalAs(UnmanagedType.Interface)] get; }

    [DispId(10019)]
    IBooleans Booleans { [return: MarshalAs(UnmanagedType.Interface)] get; }

    [DispId(10032)]
    IElementaryBodies ElementaryBodies { [return: MarshalAs(UnmanagedType.Interface)] get; }
}

[ComImport]
[Guid("EE562963-395C-4748-9726-FCA9C531B1CA")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface ISketchs
{
    [DispId(2)]
    [return: MarshalAs(UnmanagedType.Interface)]
    object Add();
}

[ComImport]
[Guid("E6BBF50D-8401-4FB3-A6B6-153D3F447255")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface ISketch
{
    [DispId(1)]
    object Plane { [return: MarshalAs(UnmanagedType.Interface)] get; [param: MarshalAs(UnmanagedType.Interface)] set; }

    [DispId(501)]
    string Name { [return: MarshalAs(UnmanagedType.BStr)] get; [param: MarshalAs(UnmanagedType.BStr)] set; }

    [DispId(503)]
    [return: MarshalAs(UnmanagedType.VariantBool)]
    bool Update();

    [DispId(7)]
    [return: MarshalAs(UnmanagedType.Interface)]
    IFragmentDocument BeginEdit();

    [DispId(8)]
    [return: MarshalAs(UnmanagedType.VariantBool)]
    bool EndEdit();
}

[ComImport]
[Guid("E19CE626-DF9C-48C4-A83D-3E3BC7F0DACA")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IFragmentDocument
{
    [DispId(1)]
    IViewsAndLayersManager ViewsAndLayersManager { [return: MarshalAs(UnmanagedType.Interface)] get; }
}

[ComImport]
[Guid("A4737593-578B-4187-8CAD-E1056EB5404B")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IViewsAndLayersManager
{
    [DispId(1)]
    IViews Views { [return: MarshalAs(UnmanagedType.Interface)] get; }
}

[ComImport]
[Guid("9CD1B5E6-C1A2-4910-8D0C-97080B14AA3D")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IViews
{
    [DispId(4)]
    IView ActiveView { [return: MarshalAs(UnmanagedType.Interface)] get; }
}

[ComImport]
[Guid("21A7BA87-1C8B-41B4-8247-CDD593546F37")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IView
{
}

[ComImport]
[Guid("D603FEC9-75B7-4FA5-918F-47074C45B848")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IDrawingContainer
{
    [DispId(5003)]
    ILineSegments LineSegments { [return: MarshalAs(UnmanagedType.Interface)] get; }
}

[ComImport]
[Guid("B211C782-A830-468E-9F4F-C499A77078D8")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface ILineSegments
{
    [DispId(2)]
    [return: MarshalAs(UnmanagedType.Interface)]
    ILineSegment Add();
}

[ComImport]
[Guid("64ACC86F-4B10-4897-8552-BC0A556D228B")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface ILineSegment
{
    [DispId(1)]
    double X1 { get; set; }

    [DispId(2)]
    double Y1 { get; set; }

    [DispId(3)]
    double X2 { get; set; }

    [DispId(4)]
    double Y2 { get; set; }

    [DispId(8)]
    int Style { get; set; }

    [DispId(503)]
    [return: MarshalAs(UnmanagedType.VariantBool)]
    bool Update();
}

[ComImport]
[Guid("A160C032-CF96-4467-A682-CE2243DF76BD")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IExtrusions
{
    [DispId(2)]
    [return: MarshalAs(UnmanagedType.Interface)]
    object Add(int extrusionType);
}

[ComImport]
[Guid("0D7FFE70-33EB-442C-A9B6-A205EA85A237")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IExtrusion
{
    [DispId(1)]
    object Sketch { [return: MarshalAs(UnmanagedType.Interface)] get; [param: MarshalAs(UnmanagedType.Interface)] set; }

    [DispId(501)]
    string Name { [return: MarshalAs(UnmanagedType.BStr)] get; [param: MarshalAs(UnmanagedType.BStr)] set; }

    [DispId(503)]
    [return: MarshalAs(UnmanagedType.VariantBool)]
    bool Update();

    [DispId(9)]
    [return: MarshalAs(UnmanagedType.VariantBool)]
    bool SetSideParameters(
        [MarshalAs(UnmanagedType.VariantBool)] bool normal,
        int extrusionType,
        double depth,
        double draftValue,
        [MarshalAs(UnmanagedType.VariantBool)] bool draftOutward,
        [MarshalAs(UnmanagedType.Interface)] object? depthObject);
}

[ComImport]
[Guid("1DFB9FE1-7A6C-43A3-9095-C99C8F702583")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IElementaryBodies
{
    [DispId(2)]
    [return: MarshalAs(UnmanagedType.Interface)]
    object Add(int type);
}

[ComImport]
[Guid("0075664C-0626-4ADD-A115-23270301063A")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IBlockBySizes
{
    [DispId(501)]
    string Name { [return: MarshalAs(UnmanagedType.BStr)] get; [param: MarshalAs(UnmanagedType.BStr)] set; }

    [DispId(801)]
    int OperationResult { get; set; }

    [DispId(802)]
    ILocalCoordinateSystem Position { [return: MarshalAs(UnmanagedType.Interface)] get; }

    [DispId(102)]
    double Length { get; set; }

    [DispId(103)]
    double Width { get; set; }

    [DispId(106)]
    int HeightDirection { get; set; }

    [DispId(107)]
    int HeightType { get; set; }

    [DispId(108)]
    double Height { get; set; }

    [DispId(503)]
    [return: MarshalAs(UnmanagedType.VariantBool)]
    bool Update();
}

[ComImport]
[Guid("BA6395F5-3506-4483-8864-4EEC220AF316")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface ILocalCoordinateSystem
{
    [DispId(1)]
    double X { get; set; }

    [DispId(2)]
    double Y { get; set; }

    [DispId(3)]
    double Z { get; set; }

    [DispId(503)]
    [return: MarshalAs(UnmanagedType.VariantBool)]
    bool Update();
}

[ComImport]
[Guid("1739583B-BFDA-4AD2-BFE3-14302133BB21")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IBooleans
{
    [DispId(2)]
    [return: MarshalAs(UnmanagedType.Interface)]
    object Add();
}

[ComImport]
[Guid("1FE29BDF-0B8E-4E34-A7E5-418092C6C9C3")]
[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
internal interface IBoolean
{
    [DispId(1)]
    int BooleanType { get; set; }

    [DispId(2)]
    object Bodies { get; set; }

    [DispId(3)]
    object BaseObject { get; [param: MarshalAs(UnmanagedType.Interface)] set; }

    [DispId(4)]
    bool SaveCopyBaseObject { get; set; }

    [DispId(5)]
    object ModifyObjects { get; set; }

    [DispId(6)]
    bool SaveCopyModifyObjects { get; set; }

    [DispId(503)]
    [return: MarshalAs(UnmanagedType.VariantBool)]
    bool Update();
}
