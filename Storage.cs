
using System;
using System.IO;
using LiteDB;

namespace VampireDB;

/// <summary>
/// Wrapper around a LiteDB file.  
/// Key = (platformId, string key) –> arbitrary typed value (serialized via LiteDB/BsonMapper).
/// </summary>
public sealed class Storage : IDisposable
{
    private static readonly object _sync = new();
    private readonly LiteDatabase _db;
    private readonly ILiteCollection<Record> _col;

    private const string DATA_FILE_NAME = "VampireDB.db";

    private const string DB_PATH = "BepInEx/config/VampireDB/";
    public static Storage Instance { get; } =
        new Storage(Path.Combine(DB_PATH, DATA_FILE_NAME));

    private Storage(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(dir))
        {
            Plugin.Logger.LogInfo($"No storage found creating storage at: {filePath}");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }

        _db = new LiteDatabase(new ConnectionString
        {
            Filename = filePath,
            Connection = ConnectionType.Shared
        });

        _col = _db.GetCollection<Record>("records");
        // Unique compound index so each (platformId,key) pair exists only once.
        _col.EnsureIndex(r => new { r.PlatformId, r.Key }, true);
    }


    /// <summary>
    /// Update a value based on a lambda function that expects the current val as a param.
    /// </summary>
    public T Update<T>(ulong platformId, string key, Func<T, T> mutator, T defaultValue = default)
    {
        lock (_sync)
        {
            var oldVal = TryGet(platformId, key, out T v) ? v : defaultValue;
            var newVal = mutator(oldVal);
            Set(platformId, key, newVal);
            return newVal;
        }
    }


    /// <summary>
    /// Insert or update a value.
    /// </summary>
    public void Set<T>(ulong platformId, string key, T value)
    {
        lock (_sync)
        {
            var rec = new Record
            {
                Id = $"{platformId}:{key}",
                PlatformId = platformId,
                Key = key,
                Value = _db.Mapper.Serialize(typeof(T), value)
            };
            _col.Upsert(rec);
        }
    }

    /// <summary>
    /// Try to read a value. Returns false if no entry exists.
    /// </summary>
    public bool TryGet<T>(ulong platformId, string key, out T value)
    {
        lock (_sync)
        {
            var rec = _col.FindOne(r => r.PlatformId == platformId && r.Key == key);
            if (rec != null)
            {
                value = (T)_db.Mapper.Deserialize(typeof(T), rec.Value);
                return true;
            }
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Delete an entry. Returns true if something was removed.
    /// </summary>
    public bool Delete(ulong platformId, string key)
    {
        lock (_sync)
        {
            return _col.DeleteMany(r => r.PlatformId == platformId && r.Key == key) > 0;
        }
    }

    /// <summary>
    /// Called at server bepeinex shutdown.
    /// </summary>
    public void Dispose()
    {
        _db?.Dispose();
    }
    private class Record
    {
        public string Id { get; set; }
        public ulong PlatformId { get; set; }
        public string Key { get; set; }
        public BsonValue Value { get; set; }
    }
}