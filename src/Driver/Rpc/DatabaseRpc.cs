﻿using SurrealDB.Abstractions;
using SurrealDB.Config;
using SurrealDB.Models;
using SurrealDB.Ws;

namespace SurrealDB.Driver.Rpc;

public sealed class DatabaseRpc : IDatabase<RpcResponse> {
    private readonly WsClient _client = new();
    private Config.Config _config;

    /// <inheritdoc />
    public Config.Config GetConfig() {
        return _config;
    }

    /// <inheritdoc />
    public async Task Open(
        Config.Config config,
        CancellationToken ct = default) {
        config.ThrowIfInvalid();
        _config = config;

        // Open connection
        InvalidConfigException.ThrowIfNull(config.RpcEndpoint);
        await _client.Open(config.RpcEndpoint!, ct);

        // Authenticate
        await SetAuth(config.Username, config.Password, ct);

        // Use database
        await SetUse(config.Database, config.Namespace, ct);
    }

    public async Task Close(CancellationToken ct = default) {
        await _client.Close(ct);
    }

    /// <param name="ct"> </param>
    /// <inheritdoc />
    public async Task<RpcResponse> Info(CancellationToken ct) {
        return await _client.Send(new() { Method = "info", }).ToSurreal();
    }

    /// <inheritdoc />
    public async Task<RpcResponse> Use(
        string? db,
        string? ns,
        CancellationToken ct = default) {
        WsResponse rsp = await _client.Send(new() { Method = "use", Params = new() { db, ns, }, }, ct);

        if (!rsp.Error.HasValue) {
            _config.Database = db;
            _config.Namespace = ns;
        }

        return rsp.ToSurreal();
    }

    /// <inheritdoc />
    public async Task<RpcResponse> Signup(
        Authentication auth,
        CancellationToken ct = default) {
        return await _client.Send(new() { Method = "signup", Params = new() { auth, }, }, ct).ToSurreal();
    }

    /// <inheritdoc />
    public async Task<RpcResponse> Signin(
        Authentication auth,
        CancellationToken ct = default) {
        WsResponse rsp = await _client.Send(new() { Method = "signin", Params = new() { auth, }, }, ct);

        // TODO: Update auth
        return rsp.ToSurreal();
    }

    /// <inheritdoc />
    public async Task<RpcResponse> Invalidate(CancellationToken ct = default) {
        return await _client.Send(new() { Method = "invalidate", }, ct).ToSurreal();
    }

    /// <inheritdoc />
    public async Task<RpcResponse> Authenticate(
        string token,
        CancellationToken ct = default) {
        return await _client.Send(new() { Method = "authenticate", Params = new() { token, }, }, ct).ToSurreal();
    }

    /// <inheritdoc />
    public async Task<RpcResponse> Let(
        string key,
        object? value,
        CancellationToken ct = default) {
        return await _client.Send(new() { Method = "let", Params = new() { key, value, }, }, ct).ToSurreal();
    }

    /// <inheritdoc />
    public async Task<RpcResponse> Query(
        string sql,
        IReadOnlyDictionary<string, object?>? vars,
        CancellationToken ct = default) {
        WsRequest req = new() { Method = "query", Params = new() { sql, vars, }, };
        WsResponse rsp = await _client.Send(req, ct);
        return rsp.ToSurreal();
    }

    /// <inheritdoc />
    public async Task<RpcResponse> Select(
        Thing thing,
        CancellationToken ct = default) {
        WsRequest req = new() { Method = "select", Params = new() { thing, }, };
        WsResponse rsp = await _client.Send(req, ct);
        return rsp.ToSurreal();
    }

    /// <inheritdoc />
    public async Task<RpcResponse> Create(
        Thing thing,
        object data,
        CancellationToken ct = default) {
        WsRequest req = new() { Method = "create", Async = true, Params = new() { thing, data, }, };
        WsResponse rsp = await _client.Send(req, ct);
        return rsp.ToSurreal();
    }

    /// <inheritdoc />
    public async Task<RpcResponse> Update(
        Thing thing,
        object data,
        CancellationToken ct = default) {
        return await _client.Send(new() { Method = "update", Params = new() { thing, data, }, }, ct).ToSurreal();
    }

    /// <inheritdoc />
    public async Task<RpcResponse> Change(
        Thing thing,
        object data,
        CancellationToken ct = default) {
        return await _client.Send(new() { Method = "change", Params = new() { thing, data, }, }, ct).ToSurreal();
    }

    /// <inheritdoc />
    public async Task<RpcResponse> Modify(
        Thing thing,
        object data,
        CancellationToken ct = default) {
        return await _client.Send(new() { Method = "modify", Params = new() { thing, data, }, }, ct).ToSurreal();
    }

    /// <inheritdoc />
    public async Task<RpcResponse> Delete(
        Thing thing,
        CancellationToken ct = default) {
        return await _client.Send(new() { Method = "delete", Params = new() { thing, }, }, ct).ToSurreal();
    }

    private async Task SetUse(
        string? db,
        string? ns,
        CancellationToken ct) {
        _config.Database = db;
        _config.Namespace = ns;
        await Use(db, ns, ct);
    }

    private async Task SetAuth(
        string? user,
        string? pass,
        CancellationToken ct) {
        // TODO: Support jwt auth
        _config.Username = user;
        _config.Password = pass;
        await Signin(new() { Username = user, Password = pass, }, ct);
    }
}