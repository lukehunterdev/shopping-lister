using LhDev.ShoppingLister.DbModels;
using LhDev.ShoppingLister.Exceptions;
using System.Collections.Generic;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace LhDev.ShoppingLister.Services;

public interface IWsService
{
    Task ConnectedAsync(WebSocketManager contextWebSockets, User user, object thing);
    //Task<string?> ProcessCommand(string command, string? data);
}

public abstract class WsService<TThing> : IWsService
{
    protected List<(TThing, User, WebSocket)> ThingsAndWebSockets { get; } = [];

    public Task ConnectedAsync(WebSocketManager contextWebSockets, User user, object thing) 
        => ConnectedAsync(contextWebSockets, user, (TThing)thing);

    public async Task ConnectedAsync(WebSocketManager contextWebSockets, User user, TThing thing)
    {
        var webSocket = await contextWebSockets.AcceptWebSocketAsync();
        var triple = (thing, user, webSocket);
        ThingsAndWebSockets.Add(triple);
        await WebSocketReceiveAsync(triple, this);
    }

    private static async Task WebSocketReceiveAsync((TThing, User, WebSocket) triple, WsService<TThing> wsService)
    {
        var (thing, user, webSocket) = triple;

        try
        {
            var bigString = new StringBuilder();
            var bigStringIgnore = false;

            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[4096];
                var received = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (received.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                }
                else if (received.MessageType == WebSocketMessageType.Binary)
                {
                    // Not supported!
                }
                else if (received.MessageType == WebSocketMessageType.Text)
                {
                    var str = Encoding.UTF8.GetString(buffer, 0, received.Count);

                    if (received.EndOfMessage)
                    {
                        if (bigStringIgnore)
                        {
                            bigStringIgnore = false;
                            continue;
                        }

                        if (bigString.Length > 0)
                        {
                            bigString.Append(str);
                            str = bigString.ToString();
                            bigString = new StringBuilder();
                        }

                        await wsService.ProcessString(webSocket, str, triple);
                    }
                    else
                    {
                        if (bigStringIgnore) continue;

                        bigString.Append(str);

                        if (bigString.Length <= 1 << 15) continue;

                        // Ignore strings over 32KB in size, to avoid memory issues.
                        bigStringIgnore = true;
                        bigString = new StringBuilder();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            //
        }
        finally
        {
            wsService.ThingsAndWebSockets.Remove(triple);
            webSocket.Dispose();
        }
    }

    protected async Task SendDataToOtherSocketsAsync((TThing, User, WebSocket) triple, byte[] data, Func<TThing, TThing, bool> excludeFunc)
    {
        var (thisThing, _, _) = triple;
        foreach (var thingAndWebSocket in ThingsAndWebSockets.Where(thingAndWebSocket => !thingAndWebSocket.Equals(triple)))
        {
            var (otherThing, _, otherWebSocket) = thingAndWebSocket;
            if (excludeFunc(thisThing, otherThing)) continue;

            await otherWebSocket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public async Task ProcessString(WebSocket webSocket, string str, (TThing, User, WebSocket) triple)
    {
        if (str == "ping")
        {
            await webSocket.SendAsync("pong"u8.ToArray(), WebSocketMessageType.Text, true, CancellationToken.None);
            return;
        }
        if (str == "pong") return;

        var pos = str.IndexOf('|');
        var command = pos == -1 ? str : str[..pos];
        var data = pos == -1 ? null : str[(pos+1)..];

        // Automatically capture exceptions and return an error message
        string? resp;
        try
        {
            resp = await ProcessCommand(triple, command, data);
        }
        catch (ShoppingListerWebException ex)
        {
            resp = $"error|{ex.Message}";
        }
        catch (Exception ex)
        {
            resp = $"unhandledError|{ex}";
        }

        if (resp == null) return;
        await webSocket.SendAsync(Encoding.UTF8.GetBytes(resp), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public abstract Task<string?> ProcessCommand((TThing, User, WebSocket) triple, string command, string? data);
}