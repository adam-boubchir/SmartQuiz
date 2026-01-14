document.addEventListener("DOMContentLoaded", () => {

    const chatItems = document.querySelectorAll(".chat-item");
    const chatMessages = document.getElementById("chat-messages");
    const chatInput = document.getElementById("chat-input");
    const sendBtn = document.getElementById("send-btn");
    const currentConversationInput = document.getElementById("current-conversation-id");

    let isLoading = false;

    // ----------------------------------------
    // 📂 Sélection d'une conversation
    // ----------------------------------------
    chatItems.forEach(item => {
        item.addEventListener("click", async () => {

            // UI active
            chatItems.forEach(i => i.classList.remove("active"));
            item.classList.add("active");

            const conversationId = item.dataset.id;
            currentConversationInput.value = conversationId;

            chatMessages.innerHTML = `
                <div class="text-muted text-center mt-4">
                    Chargement...
                </div>`;

            try {
                const response = await fetch(`/Chatbot/GetConversation?id=${conversationId}`);

                if (!response.ok) {
                    chatMessages.innerHTML = `
                        <div class="text-danger text-center mt-4">
                            ❌ Erreur serveur (${response.status})
                        </div>`;
                    return;
                }

                const data = await response.json();

                chatMessages.innerHTML = "";

                if (!data.messages || data.messages.length === 0) {
                    chatMessages.innerHTML = `
                        <div class="text-muted text-center mt-4">
                            Aucun message dans cette discussion
                        </div>`;
                    return;
                }

                data.messages.forEach(m => {
                    appendMessage(m.sender, m.content);
                });
            }
            catch (err) {
                chatMessages.innerHTML = `
                    <div class="text-danger text-center mt-4">
                        ❌ Erreur réseau
                    </div>`;
                console.error(err);
            }
        });
    });

    // ----------------------------------------
    // 📤 Envoi d'un message
    // ----------------------------------------
    sendBtn.addEventListener("click", sendMessage);
    chatInput.addEventListener("keypress", e => {
        if (e.key === "Enter") sendMessage();
    });

    async function sendMessage() {
        const message = chatInput.value.trim();
        const conversationId = currentConversationInput.value;

        if (!message || !conversationId || isLoading) return;

        appendMessage("user", message);
        chatInput.value = "";
        isLoading = true;

        appendMessage("ai", "🤖 L’IA réfléchit...");

        try {
            const response = await fetch("/Chatbot/Ask", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    conversationId,
                    message
                })
            });

            if (!response.ok) {
                removeLastMessage();
                appendMessage("ai", "❌ Erreur serveur");
                return;
            }

            const data = await response.json();

            removeLastMessage();
            appendMessage("ai", data.reply);
        }
        catch (err) {
            removeLastMessage();
            appendMessage("ai", "❌ Erreur réseau");
            console.error(err);
        }
        finally {
            isLoading = false;
        }
    }

    // ----------------------------------------
    // 🧱 Helpers UI
    // ----------------------------------------
    function appendMessage(sender, content) {
        const div = document.createElement("div");
        div.className = sender === "user"
            ? "alert alert-primary text-end"
            : "alert alert-secondary text-start";

        div.innerText = content;
        chatMessages.appendChild(div);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    function removeLastMessage() {
        if (chatMessages.lastChild) {
            chatMessages.removeChild(chatMessages.lastChild);
        }
    }
});
