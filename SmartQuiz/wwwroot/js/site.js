// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {

    const chatbotButton = document.getElementById("chatbot-button");
    const chatbotWindow = document.getElementById("chatbot-window");
    const closeChat = document.getElementById("close-chat");

    const input = document.getElementById("chatbot-input");
    const sendBtn = document.getElementById("chatbot-send");
    const chatBody = document.querySelector(".chat-body");

    if (!chatbotButton || !chatbotWindow) return;

    chatbotButton.onclick = () => chatbotWindow.classList.toggle("hidden");
    closeChat.onclick = () => chatbotWindow.classList.add("hidden");

    function addMessage(text, className) {
        const div = document.createElement("div");
        div.className = className;
        div.innerText = text;
        chatBody.appendChild(div);
        chatBody.scrollTop = chatBody.scrollHeight;
    }

    sendBtn.onclick = async () => {
        const message = input.value.trim();
        if (!message) return;

        addMessage("🧑 " + message, "bot-message");
        input.value = "";

        addMessage("🤖 Le bot réfléchit...", "bot-message");

        try {
            const response = await fetch("/api/chatbot/ask", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ message })
            });

            const data = await response.json();

            // Supprimer le "bot réfléchit..."
            chatBody.removeChild(chatBody.lastChild);

            addMessage("🤖 " + data.reply, "bot-message");
        }
        catch (err) {
            chatBody.removeChild(chatBody.lastChild);
            addMessage("❌ Erreur serveur", "bot-message");
        }
    };

});


