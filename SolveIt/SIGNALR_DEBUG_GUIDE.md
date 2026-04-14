# SignalR Real-Time Messaging - Debugging Guide

## Quick Verification Checklist

### Step 1: Check if WebSocket Connection is Established
1. **Open two browser windows** (for User A and User B)
2. **User A**: Open DevTools → Network tab
3. **Search**: Type "chathub" in the Network tab
4. **Look for**: A WebSocket (WS) endpoint connecting to `/chathub`
5. **Status**: Should show "101 Switching Protocols"

**If not connected:**
- Check browser console for errors
- Verify `/chathub` endpoint is accessible
- Ensure authentication is working (check if user is logged in)

---

### Step 2: Check Server Logs
**Look for these console messages on YOUR SERVER:**

```
[ChatHub] User Connected - UserId: [user-id], ConnectionId: [guid]
SignalR connected.
[MainLayout] Initializing SignalR for user: [user-id]
```

**If missing:**
- User might not be authenticated
- SignalR service might not be starting
- Check if `InitializeInbox()` is being called in MainLayout

---

### Step 3: Send a Test Message

#### User A (Sender):
1. Open inbox → search for User B
2. Click on User B to open chat
3. Type a message: "Hello World"
4. Click Send

#### Check Browser Network Tab:
1. Look for a **POST** request (might be quick)
2. Or look for a WebSocket **FRAME** with the message data

#### Check Server Console:
```
[SendMessageAsync] Sending message to: [User B ID]
```

#### Check User B's Browser:
- Message should appear in the conversation instantly
- If not, check User B's browser console for errors

---

### Step 4: Debug Console Logging

Add this to your browser's DevTools Console to monitor SignalR:

```javascript
// Check if HubConnection exists
console.log('HubConnection State:', window.signalRConnection?.state);

// Monitor incoming messages
console.log('Listening for ReceiveMessage events...');
```

---

## Common Issues & Fixes

### ❌ Issue: "WebSocket connection failed"
**Causes:**
- User not authenticated
- `/chathub` endpoint not mapped in Program.cs
- Authentication middleware blocking the connection

**Fix Check:`
- [ ] Verify `app.MapHub<ChatHub>("/chathub");` exists in Program.cs
- [ ] Verify `app.UseAuthentication()` comes before MapHub
- [ ] Check if user can access other authenticated pages (login working?)

---

### ❌ Issue: "User Connected but no messages received"
**Causes:**
- Event handler not registered properly
- Group name mismatch (sender/receiver group mismatch)
- Event unsubscription issue

**Fix Check:**
- [ ] Verify group prefix matches: `"user-" + userId` in both ChatHub and ConversationService
- [ ] Check that SignalR.OnMessageReceived event handler is subscribed in InboxDilog
- [ ] Verify handler doesn't immediately return/skip logic

---

### ❌ Issue: "Message sent but other user doesn't see it"
**Causes:**
- `ReceiverId` is wrong (check database)
- Other user's browser not connected to WebSocket
- Message event not being broadcast to correct group

**Debug Steps:**
```csharp
// In ConversationService.SendMessageAsync, add logging:
Console.WriteLine($"Sending to UserId: {newMessage.ReceiverId}");
Console.WriteLine($"Group name: user-{newMessage.ReceiverId}");
```

---

## Testing Procedure

### Prerequisites:
- Two user accounts created and logged in (in different browsers)
- Both users have inbox open

### Test Scenario:
1. **User A** opens inbox → searches for **User B**
2. **User A** types message: "Testing 123"
3. **User A** clicks Send
4. **Within 1-2 seconds**, message should appear in **User B**'s inbox

### What Should Happen:
- Message appears in User A's chat (immediately)
- Message appears in User B's inbox list (via SignalR)
- When User B opens the chat, all messages load (from database)

---

## Advanced Debugging

### Enable Verbose SignalR Logging (Client):
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Debug) // Add this
    .build();
```

### Check Database:
```sql
-- Verify inbox exists
SELECT * FROM Inbox WHERE User1Id = 'user1' OR User2Id = 'user1';

-- Verify messages exist
SELECT * FROM Converstions WHERE InboxId = 'inbox-id' ORDER BY SendedAt DESC;
```

---

## Troubleshooting Flowchart

```
Message Not Arriving?
├─ Is WebSocket connected?
│  ├─ NO → Check /chathub endpoint
│  └─ YES → Continue
├─ Does server show [ChatHub] Connected?
│  ├─ NO → Authentication issue
│  └─ YES → Continue
├─ Does server log [SendMessageAsync]?
│  ├─ NO → Message not being sent (check UI)
│  └─ YES → Continue
├─ Does receiver's browser have WebSocket?
│  ├─ NO → Receiver not connected (refresh page)
│  └─ YES → Group name mismatch
```

---

## Key Files to Check

1. **[Program.cs](Program.cs#L72-L74)** - SignalR setup
2. **[ChatHub.cs](Hubs/ChatHub.cs)** - Connection/group management
3. **[ConversationService.cs](Services/ConversationService.cs#L29)** - Message broadcasting
4. **[SignalRService.cs](UI_State/SignalRService.cs)** - Client connection
5. **[InboxDilog.razor](Components/Modules/InboxDilog.razor#L300)** - Event handling

