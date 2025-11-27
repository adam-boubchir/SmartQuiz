async function loadUnread() {
    try {
        const res = await fetch("/Messages/GetUnreadCount");
        if (!res.ok) return;

        const count = await res.json();
        const badge = document.getElementById("unreadBadge");

        if (count > 0) {
            badge.style.display = "inline-block";
            badge.textContent = count;
        } else {
            badge.style.display = "none";
        }

    } catch (e) {
        console.error("Unread error", e);
    }
}

setInterval(loadUnread, 5000);
loadUnread();
