using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

class P
{
	static XmlNamespaceManager nsManager;
	static XNamespace ns;
	static bool onlyRefs;

	static void Main(string[] args)
	{
		if (args.Length > 0 && args[0] == "-refsonly")
			onlyRefs = true;

		var nsStr = "http://schemas.microsoft.com/developer/msbuild/2003";
		nsManager = new XmlNamespaceManager(new NameTable());
		nsManager.AddNamespace("p", nsStr);
		ns = nsStr;

		//FeatureSamples.Core
		var coreCsproj = @"Samples/FeatureSamples/Core/Urho.Samples.csproj";
		var coreDoc = XDocument.Load(coreCsproj);
		RemovePackagesConfig(coreDoc);
		ReplaceRef(coreDoc, "Urho", "{641886db-2c6c-4d33-88da-97bec0ec5f86}", @"..\..\..\Bindings\Urho.csproj");
		coreDoc.Save(coreCsproj);

		//FeatureSamples.Droid
		var droidCsproj = @"Samples/FeatureSamples/Android/Urho.Samples.Droid.csproj";
		var droidDoc = XDocument.Load(droidCsproj);
		RemovePackagesConfig(droidDoc);
		ReplaceRef(droidDoc, "Urho", "{f0c1189b-75f7-4bd8-b394-a23d5b03eff6}", @"..\..\..\Bindings\Urho.Droid.csproj");
		droidDoc.Save(droidCsproj);

		//FeatureSamples.iOS
		var iosCsproj = @"Samples/FeatureSamples/iOS/Urho.Samples.iOS.csproj";
		var iosDoc = XDocument.Load(iosCsproj);
		RemovePackagesConfig(iosDoc);
		ReplaceRef(iosDoc, "Urho", "{9ae80bd9-e1e2-41da-bb6f-712b35028bd9}", @"..\..\..\Bindings\Urho.iOS.csproj");
		iosDoc.Save(iosCsproj);

		//FeatureSamples.Mac
		var desktopCsproj = @"Samples/FeatureSamples/Mac/Urho.Samples.Mac.csproj";
		var desktopDoc = XDocument.Load(desktopCsproj);
		RemovePackagesConfig(desktopDoc);
		ReplaceRef(desktopDoc, "Urho", "{F0359D5E-D6D4-47D3-A9F0-5A97C31DC476}", @"..\..\..\Bindings\Urho.Desktop.csproj");
		RemoveTargets(desktopDoc);
		desktopDoc.Save(desktopCsproj);
	}

	static void ReplaceRef(XDocument doc, string refName, string id, string path)
	{
		var node = doc.XPathSelectElement($"//p:Reference[@Include='{refName}' or contains(@Include, '{refName + ", "}')]", nsManager);
		if (node == null)//means it's already converted to ProjectReference
			return;
		node.Parent.Add(new XElement(ns + "ProjectReference", new XAttribute("Include", path),
			new XElement(ns + "Project", id),
			new XElement(ns + "Name", Path.GetFileNameWithoutExtension(path))));
		node.Remove();
	}

	static void RemovePackagesConfig(XDocument doc)
	{
		var node = doc.XPathSelectElement("//p:None[@Include='packages.config']", nsManager);
		node?.Remove();
	}

	static void RemoveTargets(XDocument doc)
	{
		if (onlyRefs) return;
		doc.XPathSelectElement(@"//p:Import[contains(@Project, 'packages\UrhoSharp')]", nsManager)?.Remove();
		doc.XPathSelectElement(@"//p:Error[contains(@Condition, 'packages\UrhoSharp')]", nsManager)?.Remove();
	}
}
