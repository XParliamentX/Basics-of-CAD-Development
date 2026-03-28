using System;
using System.Linq;
using System.Reflection;

var t = Type.GetTypeFromProgID("KOMPAS.Application.7", throwOnError:false) ?? Type.GetTypeFromProgID("Kompas.Application.7", throwOnError:true);
var app = Activator.CreateInstance(t!);
var appType = app!.GetType();
appType.InvokeMember("Visible", BindingFlags.SetProperty, null, app, new object[]{ false });
var docs = appType.InvokeMember("Documents", BindingFlags.GetProperty, null, app, null);
var doc = docs!.GetType().InvokeMember("Add", BindingFlags.InvokeMethod, null, docs, new object[]{ 4, true });
var topPart = doc!.GetType().InvokeMember("TopPart", BindingFlags.GetProperty, null, doc, null);
var tpType = topPart!.GetType();
Console.WriteLine($"TopPart CLR type: {tpType.FullName}");
foreach (var m in tpType.GetMembers(BindingFlags.Public|BindingFlags.Instance).OrderBy(m => m.Name).Take(400))
{
    Console.WriteLine($"{m.MemberType}: {m.Name}");
}
