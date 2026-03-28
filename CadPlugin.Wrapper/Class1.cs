using System.Reflection;
using System.Runtime.InteropServices;
using CadPlugin.Core;

namespace CadPlugin.Wrapper;

public interface IKompas3DApi
{
    void BuildPhoneMountBracket(BracketBuildPlan plan);
}

public sealed class Kompas3DWrapper : IKompas3DApi
{
    private const int DocumentTypePart = 4;
    private const int PlaneXoyObjectType = 1;
    private const int BaseExtrusionObjectType = 24;
    private const int BossExtrusionObjectType = 25;
    private const int EndTypeBlind = 0;
    private const int SolidLineStyle = 1;

    private readonly KompasComConnector connector;

    public Kompas3DWrapper()
        : this(new KompasComConnector())
    {
    }

    public Kompas3DWrapper(KompasComConnector connector)
    {
        this.connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }

    public void BuildPhoneMountBracket(BracketBuildPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        try
        {
            var app = ExecuteStep("подключение к приложению КОМПАС-3D", () => connector.GetOrStartKompasApplication(visible: true));
            var document = ExecuteStep("создание документа-детали", () => CreatePartDocument(app));
            var topPart = ExecuteStep("получение TopPart", () => document.TopPart);
            var modelContainer = ExecuteStep("получение интерфейса контейнера модели", () => GetModelContainer(topPart));
            ExecuteStep("задание имени детали", () => topPart.Name = "PhoneMountBracket");

            var sketch = ExecuteStep("создание эскиза", () => CreateSketch(topPart, modelContainer, plan));
            ExecuteStep("создание выдавливания", () => CreateExtrusion(modelContainer, sketch, plan));

            var updated = ExecuteStep("обновление итоговой модели", () => topPart.Update());
            if (!updated)
            {
                throw new InvalidOperationException("КОМПАС-3D не смог обновить модель после построения кронштейна.");
            }
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException("Сбой на этапе построения кронштейна в КОМПАС-3D.", ex);
        }
    }

    private static IPartDocument CreatePartDocument(IApplication app)
    {
        var documents = app.Documents ?? throw new InvalidOperationException("Не удалось получить коллекцию документов КОМПАС-3D.");
        var document = documents.Add(DocumentTypePart, true);
        if (document is null)
        {
            throw new InvalidOperationException("API КОМПАС-3D не вернул документ-деталь при вызове Documents.Add.");
        }

        if (document is not IPartDocument partDocument)
        {
            throw new InvalidOperationException($"Созданный документ не удалось привести к IPartDocument. Фактический тип: {document.GetType().FullName}.");
        }

        return partDocument;
    }

    private static IModelContainer GetModelContainer(IPart7 topPart)
    {
        if (topPart is IModelContainer modelContainer)
        {
            return modelContainer;
        }

        throw new InvalidOperationException(
            "TopPart не удалось привести к IModelContainer. " +
            $"Доступные COM-члены: {DescribeComMembers(topPart)}");
    }

    private static ISketch CreateSketch(IPart7 topPart, IModelContainer modelContainer, BracketBuildPlan plan)
    {
        var plane = ExecuteStep(
                "получение базовой плоскости XOY",
                () => GetIndexedProperty(topPart, "DefaultObject", PlaneXoyObjectType)
                    ?? GetIndexedProperty(topPart, "GetDefaultEntity", PlaneXoyObjectType)
                    ?? GetIndexedProperty(topPart, "GetDefaultObject", PlaneXoyObjectType))
            ?? throw new InvalidOperationException("Не удалось получить базовую плоскость XOY.");

        var sketchs = ExecuteStep("получение коллекции эскизов", () => modelContainer.Sketchs)
            ?? throw new InvalidOperationException("Не удалось получить коллекцию эскизов.");

        var sketchObject = ExecuteStep("добавление нового эскиза", () => sketchs.Add())
            ?? throw new InvalidOperationException("КОМПАС-3D не смог создать объект эскиза.");
        if (sketchObject is not ISketch sketch)
        {
            throw new InvalidOperationException($"Созданный эскиз не удалось привести к ISketch. Фактический тип: {sketchObject.GetType().FullName}.");
        }

        ExecuteStep("задание имени эскиза", () => sketch.Name = "BracketProfile");
        ExecuteStep("назначение плоскости эскизу", () => sketch.Plane = plane);
        ExecuteStep("обновление эскиза перед редактированием", () => EnsureUpdated(sketch, "эскиз"));

        var sketchDocument = ExecuteStep("вход в режим редактирования эскиза", () => sketch.BeginEdit())
            ?? throw new InvalidOperationException("Не удалось войти в режим редактирования эскиза.");

        ExecuteStep("построение контура эскиза", () => DrawContour(sketchDocument, plan.Contour));

        var ended = ExecuteStep("выход из режима редактирования эскиза", () => sketch.EndEdit());
        if (!ended)
        {
            throw new InvalidOperationException("КОМПАС-3D не завершил редактирование эскиза.");
        }

        ExecuteStep("обновление эскиза после редактирования", () => EnsureUpdated(sketch, "эскиз"));
        return sketch;
    }

    private static void DrawContour(IFragmentDocument sketchDocument, IReadOnlyList<Point2D> contour)
    {
        if (contour.Count < 3)
        {
            throw new InvalidOperationException("Контур эскиза должен содержать минимум три точки.");
        }

        var viewsAndLayersManager = ExecuteStep("получение ViewsAndLayersManager документа эскиза", () => sketchDocument.ViewsAndLayersManager)
            ?? throw new InvalidOperationException("API КОМПАС-3D не вернул ViewsAndLayersManager для документа эскиза.");
        var views = ExecuteStep("получение коллекции Views документа эскиза", () => viewsAndLayersManager.Views)
            ?? throw new InvalidOperationException("API КОМПАС-3D не вернул коллекцию Views для документа эскиза.");
        var activeView = ExecuteStep("получение ActiveView документа эскиза", () => views.ActiveView)
            ?? throw new InvalidOperationException("API КОМПАС-3D не вернул ActiveView для документа эскиза.");
        if (activeView is not IDrawingContainer drawingContainer)
        {
            throw new InvalidOperationException($"Активный вид эскиза не удалось привести к IDrawingContainer. Фактический тип: {activeView.GetType().FullName}.");
        }

        var lineSegments = ExecuteStep("получение коллекции LineSegments активного вида", () => drawingContainer.LineSegments)
            ?? throw new InvalidOperationException("API КОМПАС-3D не вернул коллекцию LineSegments для активного вида эскиза.");

        for (var i = 0; i < contour.Count; i++)
        {
            var start = contour[i];
            var end = contour[(i + 1) % contour.Count];

            var lineSegment = ExecuteStep($"создание отрезка {i + 1}", () => lineSegments.Add())
                ?? throw new InvalidOperationException("КОМПАС-3D не смог создать объект отрезка в эскизе.");

            ExecuteStep($"задание X1 для отрезка {i + 1}", () => lineSegment.X1 = start.X);
            ExecuteStep($"задание Y1 для отрезка {i + 1}", () => lineSegment.Y1 = start.Y);
            ExecuteStep($"задание X2 для отрезка {i + 1}", () => lineSegment.X2 = end.X);
            ExecuteStep($"задание Y2 для отрезка {i + 1}", () => lineSegment.Y2 = end.Y);
            TrySetProperty(lineSegment, "Style", SolidLineStyle);

            TryInvokeMethod(lineSegment, "Update");
        }
    }

    private static void CreateExtrusion(IModelContainer modelContainer, ISketch sketch, BracketBuildPlan plan)
    {
        var extrusions = ExecuteStep("получение коллекции выдавливаний", () => modelContainer.Extrusions)
            ?? throw new InvalidOperationException("Не удалось получить коллекцию операций выдавливания.");

        IExtrusion extrusion;
        try
        {
            extrusion = CreateExtrusionObject(extrusions, BaseExtrusionObjectType, "создание базового выдавливания");
        }
        catch
        {
            extrusion = CreateExtrusionObject(extrusions, BossExtrusionObjectType, "создание альтернативного выдавливания");
        }

        ExecuteStep("задание имени выдавливания", () => extrusion.Name = "BracketExtrusion");
        ExecuteStep("передача эскиза в выдавливание", () => extrusion.Sketch = sketch);

        var setSideOk = ExecuteStep(
            "задание параметров выдавливания",
            () => extrusion.SetSideParameters(
                normal: true,
                extrusionType: EndTypeBlind,
                depth: plan.DepthMm,
                draftValue: 0.0,
                draftOutward: false,
                depthObject: null));
        if (!setSideOk)
        {
            throw new InvalidOperationException("КОМПАС-3D не принял параметры выдавливания.");
        }

        ExecuteStep("обновление операции выдавливания", () => EnsureUpdated(extrusion, "операцию выдавливания"));
    }

    private static IExtrusion CreateExtrusionObject(IExtrusions extrusions, int extrusionType, string stepName)
    {
        var extrusionObject = ExecuteStep(stepName, () => extrusions.Add(extrusionType))
            ?? throw new InvalidOperationException("КОМПАС-3D не смог создать операцию выдавливания.");
        if (extrusionObject is not IExtrusion extrusion)
        {
            throw new InvalidOperationException($"Созданную операцию не удалось привести к IExtrusion. Фактический тип: {extrusionObject.GetType().FullName}.");
        }

        return extrusion;
    }

    private static void EnsureUpdated(object target, string objectName)
    {
        var result = InvokeMethod(target, "Update");
        if (result is bool updated && !updated)
        {
            throw new InvalidOperationException($"КОМПАС-3D не смог обновить {objectName}.");
        }
    }

    private static object? GetProperty(object target, string propertyName)
    {
        try
        {
            return target.GetType().InvokeMember(
                propertyName,
                BindingFlags.GetProperty,
                binder: null,
                target: target,
                args: null);
        }
        catch (MissingMethodException)
        {
            return null;
        }
        catch (TargetInvocationException ex) when (IsUnknownName(ex))
        {
            return null;
        }
    }

    private static object? GetIndexedProperty(object target, string propertyName, params object[] args)
    {
        try
        {
            return target.GetType().InvokeMember(
                propertyName,
                BindingFlags.GetProperty,
                binder: null,
                target: target,
                args: args);
        }
        catch (MissingMethodException)
        {
            return null;
        }
        catch (TargetInvocationException ex) when (IsUnknownName(ex))
        {
            return null;
        }
    }

    private static void SetProperty(object target, string propertyName, object? value)
    {
        try
        {
            target.GetType().InvokeMember(
                propertyName,
                BindingFlags.SetProperty,
                binder: null,
                target: target,
                args:
                [
                    value,
                ]);
        }
        catch (COMException ex) when (IsUnknownName(ex))
        {
            throw new InvalidOperationException(
                $"API КОМПАС-3D не нашел свойство \"{propertyName}\" у COM-объекта типа {target.GetType().FullName}.",
                ex);
        }
        catch (TargetInvocationException ex) when (IsUnknownName(ex))
        {
            throw new InvalidOperationException(
                $"API КОМПАС-3D не нашел свойство \"{propertyName}\" у COM-объекта типа {target.GetType().FullName}.",
                ex.InnerException ?? ex);
        }
        catch (MissingMethodException ex)
        {
            throw new InvalidOperationException(
                $"API КОМПАС-3D не нашел свойство \"{propertyName}\" у COM-объекта типа {target.GetType().FullName}.",
                ex);
        }
    }

    private static bool TrySetProperty(object target, string propertyName, object? value)
    {
        try
        {
            SetProperty(target, propertyName, value);
            return true;
        }
        catch (MissingMethodException)
        {
            return false;
        }
        catch (COMException ex) when (IsUnknownName(ex))
        {
            return false;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is MissingMethodException)
        {
            return false;
        }
        catch (TargetInvocationException ex) when (IsUnknownName(ex))
        {
            return false;
        }
    }

    private static object? InvokeMethod(object target, string methodName, params object?[]? args)
    {
        try
        {
            return target.GetType().InvokeMember(
                methodName,
                BindingFlags.InvokeMethod,
                binder: null,
                target: target,
                args: args);
        }
        catch (COMException ex) when (IsUnknownName(ex))
        {
            throw new InvalidOperationException(
                $"API КОМПАС-3D не нашел метод \"{methodName}\" у COM-объекта типа {target.GetType().FullName}.",
                ex);
        }
        catch (TargetInvocationException ex) when (IsUnknownName(ex))
        {
            throw new InvalidOperationException(
                $"API КОМПАС-3D не нашел метод \"{methodName}\" у COM-объекта типа {target.GetType().FullName}.",
                ex.InnerException ?? ex);
        }
        catch (MissingMethodException ex)
        {
            throw new InvalidOperationException(
                $"API КОМПАС-3D не нашел метод \"{methodName}\" у COM-объекта типа {target.GetType().FullName}.",
                ex);
        }
    }

    private static object? TryInvokeMethod(object target, string methodName, params object?[]? args)
    {
        try
        {
            return InvokeMethod(target, methodName, args);
        }
        catch (InvalidOperationException ex) when (ex.InnerException is COMException comEx && IsUnknownName(comEx))
        {
            return null;
        }
        catch (InvalidOperationException ex) when (ex.InnerException is MissingMethodException)
        {
            return null;
        }
    }

    private static bool IsUnknownName(TargetInvocationException ex)
    {
        return ex.InnerException is COMException comEx && comEx.ErrorCode == unchecked((int)0x80020006);
    }

    private static bool IsUnknownName(COMException ex)
    {
        return ex.ErrorCode == unchecked((int)0x80020006);
    }

    private static string DescribeComMembers(object target)
    {
        try
        {
            var dispatchPtr = Marshal.GetIDispatchForObject(target);
            try
            {
                var dispatch = (IDispatchInfo)Marshal.GetTypedObjectForIUnknown(dispatchPtr, typeof(IDispatchInfo));
                var hr = dispatch.GetTypeInfoCount(out var typeInfoCount);
                if (hr != 0 || typeInfoCount == 0)
                {
                    return "type info недоступна";
                }

                hr = dispatch.GetTypeInfo(0, 0, out var typeInfo);
                if (hr != 0 || typeInfo is null)
                {
                    return "type info недоступна";
                }

                typeInfo.GetTypeAttr(out var typeAttrPtr);
                try
                {
                    var typeAttr = Marshal.PtrToStructure<System.Runtime.InteropServices.ComTypes.TYPEATTR>(typeAttrPtr);
                    var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    for (var i = 0; i < typeAttr.cFuncs; i++)
                    {
                        typeInfo.GetFuncDesc(i, out var funcDescPtr);
                        try
                        {
                            var funcDesc = Marshal.PtrToStructure<System.Runtime.InteropServices.ComTypes.FUNCDESC>(funcDescPtr);
                            var memberNames = new string[Math.Max(1, funcDesc.cParams + 1)];
                            typeInfo.GetNames(funcDesc.memid, memberNames, memberNames.Length, out var count);
                            if (count > 0 && !string.IsNullOrWhiteSpace(memberNames[0]))
                            {
                                names.Add(memberNames[0]);
                            }
                        }
                        finally
                        {
                            typeInfo.ReleaseFuncDesc(funcDescPtr);
                        }
                    }

                    if (names.Count == 0)
                    {
                        return "члены не обнаружены";
                    }

                    return string.Join(", ", names.OrderBy(x => x).Take(80));
                }
                finally
                {
                    typeInfo.ReleaseTypeAttr(typeAttrPtr);
                }
            }
            finally
            {
                Marshal.Release(dispatchPtr);
            }
        }
        catch
        {
            return "не удалось прочитать COM type info";
        }
    }

    private static T ExecuteStep<T>(string stepName, Func<T> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Ошибка на шаге: {stepName}.", ex);
        }
    }

    private static void ExecuteStep(string stepName, Action action)
    {
        ExecuteStep(
            stepName,
            () =>
            {
                action();
                return true;
            });
    }
}

[ComImport]
[Guid("00020400-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IDispatchInfo
{
    [PreserveSig]
    int GetTypeInfoCount(out uint pctinfo);

    [PreserveSig]
    int GetTypeInfo(uint iTInfo, int lcid, [MarshalAs(UnmanagedType.Interface)] out System.Runtime.InteropServices.ComTypes.ITypeInfo typeInfo);

    [PreserveSig]
    int GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] names, uint cNames, int lcid, [Out] int[] dispIds);

    [PreserveSig]
    int Invoke(int dispIdMember, ref Guid riid, int lcid, short wFlags, ref System.Runtime.InteropServices.ComTypes.DISPPARAMS pDispParams, out object pVarResult, ref System.Runtime.InteropServices.ComTypes.EXCEPINFO pExcepInfo, out uint puArgErr);
}

public sealed class KompasComConnector
{
    private static readonly string[] ProgIdsToTry =
    [
        "KOMPAS.Application.7",
        "Kompas.Application.7",
    ];

    internal IApplication GetOrStartKompasApplication(bool visible)
    {
        Exception? lastError = null;

        foreach (var progId in ProgIdsToTry)
        {
            try
            {
                var type = Type.GetTypeFromProgID(progId, throwOnError: false);
                if (type is null)
                {
                    continue;
                }

                var app = Activator.CreateInstance(type);
                if (app is not IApplication typedApp)
                {
                    continue;
                }

                typedApp.Visible = visible;
                return typedApp;
            }
            catch (Exception ex)
            {
                lastError = ex;
            }
        }

        throw new InvalidOperationException(
            "Не удалось подключиться к API 7 КОМПАС-3D через COM. Проверь, что КОМПАС-3D v24 установлен, запускается и что ProgID KOMPAS.Application.7 зарегистрирован в системе.",
            lastError);
    }
}

public sealed class BracketBuilder
{
    private readonly IKompas3DApi api;

    public BracketBuilder(IKompas3DApi api)
    {
        this.api = api ?? throw new ArgumentNullException(nameof(api));
    }

    public IReadOnlyList<ValidationIssue> Build(BracketParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var issues = parameters.Validate();
        if (issues.Count > 0)
        {
            return issues;
        }

        var plan = BracketGeometryPlanner.CreatePlan(parameters);
        api.BuildPhoneMountBracket(plan);
        return Array.Empty<ValidationIssue>();
    }
}
