(function () {
    const chatShell = document.querySelector(".chat-shell");

    if (!chatShell) {
        return;
    }

    const messagesList = document.getElementById("messages-list");
    const messageForm = document.getElementById("message-form");
    const messageInput = document.getElementById("message-input");
    const connectionStatus = document.getElementById("connection-status");
    const currentRoomTitle = document.getElementById("current-room-title");
    const chatError = document.getElementById("chat-error");
    const roomButtons = Array.from(document.querySelectorAll(".room-button"));

    let currentRoomId = chatShell.dataset.initialRoom || "general";
    let connection = null;

    function setError(message) {
        chatError.textContent = message || "";
    }

    function setConnectionStatus(status) {
        connectionStatus.textContent = status;
        connectionStatus.dataset.status = status.toLowerCase();
    }

    function escapeHtml(value) {
        return value
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll("\"", "&quot;")
            .replaceAll("'", "&#039;");
    }

    function formatDate(value) {
        return new Date(value).toLocaleString([], {
            dateStyle: "medium",
            timeStyle: "short"
        });
    }

    function appendMessage(message) {
        const item = document.createElement("article");
        item.className = "message-item";

        item.innerHTML = `
            <div class="message-meta">
                <strong>${escapeHtml(message.userName)}</strong>
                <span>${formatDate(message.createdAtUtc)}</span>
            </div>
            <p>${escapeHtml(message.content)}</p>
        `;

        messagesList.appendChild(item);
        messagesList.scrollTop = messagesList.scrollHeight;
    }

    async function loadMessages(roomId) {
        messagesList.innerHTML = "";
        setError("");

        const response = await fetch(`/Chat/Messages?roomId=${encodeURIComponent(roomId)}`);

        if (!response.ok) {
            setError("Messages could not be loaded.");
            return;
        }

        const messages = await response.json();
        messages.forEach(appendMessage);
    }

    async function selectRoom(roomId) {
        if (roomId === currentRoomId) {
            return;
        }

        setError("");

        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            await connection.invoke("LeaveRoom", currentRoomId);
        }

        currentRoomId = roomId;

        roomButtons.forEach((button) => {
            const isActive = button.dataset.roomId === roomId;
            button.classList.toggle("active", isActive);

            if (isActive) {
                currentRoomTitle.textContent = button.textContent.trim();
            }
        });

        await loadMessages(currentRoomId);

        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            await connection.invoke("JoinRoom", currentRoomId);
        }
    }

    async function startConnection() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/chatHub")
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveMessage", appendMessage);

        connection.onreconnecting(() => setConnectionStatus("Reconnecting"));
        connection.onreconnected(async () => {
            setConnectionStatus("Connected");
            await connection.invoke("JoinRoom", currentRoomId);
        });
        connection.onclose(() => setConnectionStatus("Disconnected"));

        await connection.start();
        setConnectionStatus("Connected");
        await connection.invoke("JoinRoom", currentRoomId);
    }

    roomButtons.forEach((button) => {
        button.addEventListener("click", async () => {
            try {
                await selectRoom(button.dataset.roomId);
            } catch (error) {
                setError(error.message);
            }
        });
    });

    messageForm.addEventListener("submit", async (event) => {
        event.preventDefault();

        const content = messageInput.value.trim();
        if (!content) {
            return;
        }

        try {
            await connection.invoke("SendMessage", currentRoomId, content);
            messageInput.value = "";
            messageInput.focus();
            setError("");
        } catch (error) {
            setError(error.message);
        }
    });

    window.addEventListener("beforeunload", () => {
        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            connection.invoke("LeaveRoom", currentRoomId);
        }
    });

    loadMessages(currentRoomId)
        .then(startConnection)
        .catch((error) => {
            setConnectionStatus("Disconnected");
            setError(error.message);
        });
})();
