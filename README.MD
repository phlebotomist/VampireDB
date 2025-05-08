Pretty early needs testing :shrug:
Might expload and destroy everything.
If it does lmk so I can fix it.

some example usage

```csharp
// tdbset <key> <value>
[Command("dbset", shortHand: "dbs", adminOnly: true)]
public void SetValue(ChatCommandContext ctx, string key, string value)
{
Storage.Instance.Set(ctx.User.PlatformId, key, value);
ctx.Reply($"Saved: {key} = {value}");
}

    // tdbget <key>
    [Command("dbget", shortHand: "dbg", adminOnly: true)]
    public void GetValue(ChatCommandContext ctx, string key)
    {
        string result;
        if (Storage.Instance.TryGet(ctx.User.PlatformId, key, out result))
            ctx.Reply($"{key} = {result}");
        else
            ctx.Reply($"No entry for '{key}'.");
    }
    // tdbget <key>
    [Command("dbgetInt", shortHand: "dbgi", adminOnly: true)]
    public void GetIntValue(ChatCommandContext ctx, string key)
    {
        int result;
        if (Storage.Instance.TryGet(ctx.User.PlatformId, key, out result))
            ctx.Reply($"{key} = {result}");
        else
            ctx.Reply($"No entry for '{key}'.");
    }
    // tdbinc <key> [delta]   (increments an integer field; default +1)
    [Command("dbinc", shortHand: "dbi", adminOnly: true)]
    public void IncrementValue(ChatCommandContext ctx, string key, int delta = 1)
    {
        int newVal = Storage.Instance.Update<int>(
            ctx.User.PlatformId,
            key,
            current => current + delta,
            0
        );
        ctx.Reply($"{key} incremented to {newVal}");
    }

    // tdbdel <key>
    [Command("dbdel", shortHand: "dbd", adminOnly: true)]
    public void DeleteValue(ChatCommandContext ctx, string key)
    {
        bool removed = Storage.Instance.Delete(ctx.User.PlatformId, key);
        ctx.Reply(removed ? $"Deleted '{key}'." : $"Nothing to delete for '{key}'.");
    }
```
