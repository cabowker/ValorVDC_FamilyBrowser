using System;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using Newtonsoft.Json;

namespace ValorVDC_FamilyBrowser.Services;

public static class FamilyBrowserSettingsStorage
{
    // Same GUID as the BimTools version so settings are shared if both plugins are installed.
    private static readonly Guid SchemaGuid = new Guid("C4E5A1B2-D3F6-4E78-9A0B-1C2D3E4F5A6B");
    private const string SchemaName  = "ValorVDCFamilyBrowserSettings";
    private const string FieldName   = "SettingsJson";
    private const string ElementName = "ValorVDC_FamilyBrowserSettings";

    public static FamilyBrowserSettings Load(Document doc)
    {
        try
        {
            var schema = Schema.Lookup(SchemaGuid);
            if (schema == null) return new FamilyBrowserSettings();

            var storage = FindStorageElement(doc);
            if (storage == null) return new FamilyBrowserSettings();

            var entity = storage.GetEntity(schema);
            if (!entity.IsValid()) return new FamilyBrowserSettings();

            var json = entity.Get<string>(schema.GetField(FieldName));
            if (string.IsNullOrWhiteSpace(json)) return new FamilyBrowserSettings();

            return JsonConvert.DeserializeObject<FamilyBrowserSettings>(json)
                   ?? new FamilyBrowserSettings();
        }
        catch { return new FamilyBrowserSettings(); }
    }

    public static void Save(Document doc, FamilyBrowserSettings settings)
    {
        using var tx = new Transaction(doc, "Save Family Browser Settings");
        tx.Start();
        try
        {
            var schema  = GetOrCreateSchema();
            var storage = FindStorageElement(doc) ?? CreateStorageElement(doc);
            var entity  = new Entity(schema);
            entity.Set(schema.GetField(FieldName), JsonConvert.SerializeObject(settings));
            storage.SetEntity(entity);
            tx.Commit();
        }
        catch { tx.RollBack(); throw; }
    }

    private static Schema GetOrCreateSchema()
    {
        var schema = Schema.Lookup(SchemaGuid);
        if (schema != null) return schema;

        var builder = new SchemaBuilder(SchemaGuid);
        builder.SetSchemaName(SchemaName);
        builder.SetReadAccessLevel(AccessLevel.Public);
        builder.SetWriteAccessLevel(AccessLevel.Public);
        builder.AddSimpleField(FieldName, typeof(string));
        return builder.Finish();
    }

    private static DataStorage FindStorageElement(Document doc)
        => new FilteredElementCollector(doc)
            .OfClass(typeof(DataStorage))
            .Cast<DataStorage>()
            .FirstOrDefault(e => e.Name == ElementName);

    private static DataStorage CreateStorageElement(Document doc)
    {
        var storage = DataStorage.Create(doc);
        storage.Name = ElementName;
        return storage;
    }
}
